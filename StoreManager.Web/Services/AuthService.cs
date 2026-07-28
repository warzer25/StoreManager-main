using System.Net.Http.Json;

namespace StoreManager.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == "admin";
    public bool IsCashier => CurrentUser?.Role == "cashier";

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
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}