using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace BlogApp.Provider
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            try
            {
                var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                await _localStorageService.RemoveItemAsync("authToken");
                var anonymousIdentity = new ClaimsIdentity();
                var anonymousUser = new ClaimsPrincipal(anonymousIdentity);
                return new AuthenticationState(anonymousUser);
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private List<Claim> ParseClaimsFromJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token), "JWT token is required.");

            var parts = token.Split('.');
            if (parts.Length != 3)
                throw new ArgumentException("Invalid JWT token format.");

            var payload = parts[1];
            var jsonBytes = Base64UrlDecode(payload);
            var claimsDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (claimsDictionary != null && claimsDictionary.TryGetValue("exp", out var exp))
            {
                var expiration = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp.ToString()));
                if (expiration < DateTime.UtcNow)
                    throw new Exception("Token has expired.");
            }

            return claimsDictionary?.Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty)).ToList()
                   ?? new List<Claim>();
        }

        private static byte[] Base64UrlDecode(string base64Url)
        {
            var padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + new string('=', 4 - base64Url.Length % 4);
            var base64 = padded.Replace('-', '+').Replace('_', '/');
            return Convert.FromBase64String(base64);
        }
    }
}
