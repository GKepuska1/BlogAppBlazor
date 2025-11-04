using BlogApp.Core.Constants;
using BlogApp.Core.Context;
using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;

namespace BlogApp.Core
{
    public static class DependencyInjection
    {
        public static void AddCore(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["ConnectionString"];

            services.AddDbContext<IAppDbContext, AppDbContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Transient);

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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
                options.TokenValidationParameters = TokenValidationConstants.GetValidationParameters(key, configuration);
            });

            services.TryAddScoped<SignInManager<ApplicationUser>>();
            services.TryAddScoped<RoleManager<IdentityRole>>();

            services.AddTransient<UserManager<ApplicationUser>>();

            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ICommentService, CommentService>();

            services.AddScoped<IJwtService, JwtService>();
            services.AddSingleton<IGuestNameGenerator, GuestNameGenerator>();
            services.AddScoped<IBitcoinPaymentService, BitcoinPaymentService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
