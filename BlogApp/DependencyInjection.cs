using Blazored.LocalStorage;
using BlogApp.Provider;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlogApp
{
    public static class DependencyInjection
    {
        public static void AddWeb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBlazoredLocalStorage();

            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        }
    }
}
