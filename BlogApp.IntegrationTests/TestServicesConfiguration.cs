using BlogApp.Core.Context;
using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlogApp.IntegrationTests
{
    public class TestServicesConfiguration
    {
        public static TestSettings ConfigurePersistentDatabaseAndServices(bool createDatabaseTest = true)
        {
            var services = new ServiceCollection();

            var configurationBuilder = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);


            var configuration = configurationBuilder.Build();
            var connectionString = configuration["ConnectionStrings:TestDb"];
            var databaseName = "";
            if (createDatabaseTest)
            {
                databaseName = CreateDatabase(connectionString);
                connectionString = $"{connectionString}Database={databaseName};";
            }

            services.AddDbContext<IAppDbContext, AppDbContext>(
                            dbContextOptions => dbContextOptions
                                .UseSqlServer(connectionString));

            services = InjectServices(services, configuration);
            services = InjectApplicationUser(services);

            var serviceProvider = services.BuildServiceProvider();
            if (createDatabaseTest)
            {
                serviceProvider = services.BuildServiceProvider();
                var dbContext = serviceProvider.GetService<IAppDbContext>();
                var dbFacade = dbContext.GetDatabaseFacade();
                dbFacade.OpenConnection();
                dbFacade.Migrate();
                dbFacade.CloseConnection();
            }

            return new TestSettings()
            {
                DatabaseName = databaseName,
                ServiceProvider = serviceProvider,
                ConnectionString = connectionString
            };
        }

        public static TestSettings ConfigureInMemoryDatabaseAndServices()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();

            services.AddDbContext<IAppDbContext, AppDbContext>(options =>
                options.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}"));

            services = InjectServices(services, configuration);
            services = InjectApplicationUser(services);

            var serviceProvider = services.BuildServiceProvider();

            return new TestSettings
            {
                ServiceProvider = serviceProvider,
                ConnectionString = "InMemory"
            };
        }

        private static ServiceCollection InjectApplicationUser(ServiceCollection services)
        {
            services.AddIdentityCore<ApplicationUser>(config =>
            {
                config.Password.RequiredLength = 6;
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
              .AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<AppDbContext>();

            services.AddHttpContextAccessor();

            services.TryAddScoped<SignInManager<ApplicationUser>>();
            services.TryAddScoped<RoleManager<IdentityRole>>();

            services.AddTransient<UserManager<ApplicationUser>>();
            return services;
        }

        private static ServiceCollection InjectServices(ServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpContextAccessor();


            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }

        private static string CreateDatabase(string connectionString)
        {
            var databaseName = "NewTestDb_" + Guid.NewGuid().ToString().Replace("-", "");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE {databaseName}";
                command.ExecuteNonQuery();
            }
            return databaseName;
        }

        public static void TearDownDatabase(TestSettings testSettings)
        {
            if (!string.IsNullOrEmpty(testSettings.DatabaseName))
            {
                using (var connection = new SqlConnection(testSettings.ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $"DROP DATABASE {testSettings.DatabaseName}";
                    var result = command.ExecuteNonQuery();
                }
            }
        }
    }

    public class TestSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
    }
}
