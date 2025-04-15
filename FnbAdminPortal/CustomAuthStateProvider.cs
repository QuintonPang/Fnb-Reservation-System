using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;          // For HttpClient
using System.Collections.Generic;  // For IEnumerable<>
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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
    ClaimsIdentity identity = new();

    try
    {
        var token = await _localStorage.GetItemAsStringAsync("token");
        token = token?.Replace("\"", "").Trim(); // Clean quotes and whitespace

        if (!string.IsNullOrWhiteSpace(token))
        {
            
            identity = new ClaimsIdentity(ParseToken(token), "jwt");
            Console.WriteLine($"[DEBUG] Identity authenticated: {identity.IsAuthenticated}");

         foreach (var claim in ParseToken(token))
    {
        Console.WriteLine($"[DEBUG] Claim Type: {claim.Type}, Value: {claim.Value}");
    }

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("\"", ""));

        }
    }
    catch (InvalidOperationException)
    {
        // JSInterop is not available during prerendering
        // Return unauthenticated state for now
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    // // // Hardcoded claims for always-authenticated user
    // var claims = new List<Claim>
    // {
    //     new Claim(ClaimTypes.Name, "Admin User"),
    //     new Claim(ClaimTypes.Role, "Admin"),
    //     new Claim("outletId", "1")
    // };

    // var identity = new ClaimsIdentity(claims, "jwt"); // <- "jwt" makes the user authenticated
    var user = new ClaimsPrincipal(identity);
    var state = new AuthenticationState(user);

    return new AuthenticationState(new ClaimsPrincipal(identity));
}

          public async void NotifyUserAuthentication(string role, int outletId)
{
  
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, role),
    new Claim(ClaimTypes.Role, role), // THIS IS REQUIRED
    new Claim("outletId", outletId.ToString()) // Custom claim for Outlet ID

};

    // Create the ClaimsPrincipal with the claims
    var identity = new ClaimsIdentity(claims, "jwt");
    var user = new ClaimsPrincipal(identity);

    // Create the AuthenticationState with the ClaimsPrincipal
    var authState = new AuthenticationState(user);
        await _localStorage.SetItemAsync("token", GenerateTokenFromClaims(claims)); // Store the token in local storage

    // Notify that the authentication state has changed
    NotifyAuthenticationStateChanged(Task.FromResult(authState));
}public List<Claim> ParseToken(string token)
{
    Console.WriteLine("[DEBUG] Inside ParseToken");

    var tokenHandler = new JwtSecurityTokenHandler();

    if (tokenHandler.CanReadToken(token))
    {
        Console.WriteLine("[DEBUG] Can read token");

        var jwtToken = tokenHandler.ReadJwtToken(token);

        Console.WriteLine($"[DEBUG] Claims found: {jwtToken.Claims.Count()}");

        return jwtToken.Claims.ToList();
    }
    else
    {
        Console.WriteLine("[DEBUG] Cannot read token");
    }

    return new List<Claim>();
}

private byte[] ParseBase64WithoutPadding(string base64)
{
    switch (base64.Length % 4)
    {
        case 2: base64 += "=="; break;
        case 3: base64 += "="; break;
    }

    return Convert.FromBase64String(base64);
}


        

        public async void NotifyUserLogout()
{
            await _localStorage.SetItemAsync("token",""); // Store the token in local storage

    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
}
 // Method to generate JWT token from claims
    public string GenerateTokenFromClaims(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("qD0b3ERlzPYK8XBz0H5np1f9hNq9LO00pXGVeDhZIg4=")); // Secret key for signing
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create a JWT token
        var token = new JwtSecurityToken(
            issuer: "your-app",
            audience: "your-users",
            claims: claims,  // Pass the claims to the token
            expires: DateTime.UtcNow.AddHours(24),  // Token expiration time
            signingCredentials: creds
        );

        // Convert the token to a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    }
}