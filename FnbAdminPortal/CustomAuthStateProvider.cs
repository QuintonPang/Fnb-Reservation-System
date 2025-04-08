using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;          // For HttpClient
using System.Collections.Generic;  // For IEnumerable<>
using System.Linq;

namespace FnbAdminPortal
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            _localStorage = localStorage;
            _http = http;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
          //  string token = await _localStorage.GetItemAsStringAsync("token");
         var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IlNoYWhyaXlhciIsInJvbGUiOiJBZG1pbiIsImlhdCI6MTUxNjIzOTAyMn0.l9E7Oypb-ozndpFUkeVhOYzhtjGEuFmdYdAxhbpXAFY";

            var identity = new ClaimsIdentity();
//_http.DefaultRequestHeaders.Authorization = null;
/*
           if (!string.IsNullOrEmpty(token))
            {
                identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);
*/
       //   NotifyAuthenticationStateChanged(Task.FromResult(state));
       
                   var user = new ClaimsPrincipal(identity);

            var state = new AuthenticationState(user);

            return state;
        }

          public void NotifyUserAuthentication(string role)
{
  
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, role),
    new Claim(ClaimTypes.Role, role) // THIS IS REQUIRED
};

    // Create the ClaimsPrincipal with the claims
    var identity = new ClaimsIdentity(claims, "custom");
    var user = new ClaimsPrincipal(identity);

    // Create the AuthenticationState with the ClaimsPrincipal
    var authState = new AuthenticationState(user);

    // Notify that the authentication state has changed
    NotifyAuthenticationStateChanged(Task.FromResult(authState));
}

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}