using BlogApp.Core.Constants;
using BlogApp.Core.Data;
using BlogApp.Core.Services;
using BlogApp.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
            var redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

            // Redis services
            services.AddSingleton<IRedisService>(sp => new RedisService(redisConnectionString));
            services.AddScoped<IRedisUserRepository, RedisUserRepository>();
            services.AddScoped<IRedisBlogRepository, RedisBlogRepository>();
            services.AddScoped<IRedisCommentRepository, RedisCommentRepository>();

            // Password hasher for user authentication
            services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

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

            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ICommentService, CommentService>();

            services.AddScoped<IJwtService, JwtService>();
            services.AddSingleton<IGuestNameGenerator, GuestNameGenerator>();
            services.AddScoped<IBitcoinPaymentService, BitcoinPaymentService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        }
    }
}
