using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace StoreManager.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "auth_token";
    private const string UserKey = "auth_user";

    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == "admin";
    public bool IsCashier => CurrentUser?.Role == "cashier";
    public string? Token => CurrentUser?.Token;

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;

        _ = RestoreSessionAsync();
    }

    public async Task<bool> LoginAsync(string email, string password, bool rememberMe = false)
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
            if (user == null || string.IsNullOrEmpty(user.Token))
            {
                return false;
            }

            CurrentUser = user;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", user.Token);

            if (rememberMe)
            {
                await SaveSessionAsync(user);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", UserKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing session: {ex.Message}");
        }
    }

    private async Task SaveSessionAsync(User user)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, user.Token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", UserKey, System.Text.Json.JsonSerializer.Serialize(user));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving session: {ex.Message}");
        }
    }

    private async Task RestoreSessionAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", TokenKey);
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", UserKey);
            if (string.IsNullOrEmpty(userJson))
            {
                return;
            }

            var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
            if (user == null)
            {
                return;
            }

            CurrentUser = user;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring session: {ex.Message}");
            await LogoutAsync();
        }
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Token { get; set; } = string.Empty;
} 