using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StoreManager.Web.Services;



public class AuthService
{
    // this service will handle authentication and user management in the application.
    // It will communicate with the backend API to perform login and logout operations, and it will store the current user's information in memory.
    // It will also provide properties to check if the user is authenticated and if the user has a specific role (admin or cashier).
    // The service will use an HttpClient to send requests to the backend API, and it will set the Authorization header with the user's token for authenticated requests.
    // The service will also provide a method to log out the user, which will clear the current user's information and remove the Authorization header.
    // The service will be registered as a singleton in the dependency injection container, so it will be available throughout the application.

    private readonly HttpClient _httpClient;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == "admin";
    public bool IsCashier => CurrentUser?.Role == "cashier";
    public string? Token => CurrentUser?.Token;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var user = await response.Content.ReadFromJsonAsync<User>();
            if (user == null)
            {
                return false;
            }

            CurrentUser = user;
            // 👇 THIS IS CRITICAL - sends the token with every request
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", user.Token);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        CurrentUser = null;
    }
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}