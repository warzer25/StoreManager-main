using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class UserService(HttpClient http)
{
    private const string BasePath = "api/users";

    // GET: api/users
    public async Task<List<User>> GetAllAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<List<User>>(BasePath) ?? new List<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading users: {ex.Message}");
            throw;
        }
    }

    // POST: api/users
    public async Task<User?> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync(BasePath, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            throw;
        }
    }

    // DELETE: api/users/{id}
    public async Task DeleteAsync(int id)
    {
        try
        {
            var response = await http.DeleteAsync($"{BasePath}/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting user {id}: {ex.Message}");
            throw;
        }
    }
}