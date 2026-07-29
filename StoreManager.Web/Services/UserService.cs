using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class UserService(HttpClient http)
{
    // This service will handle CRUD operations for users in the application.
    // It will communicate with the backend API to perform create, read, update, and delete operations for users.
    // The service will use an HttpClient to send requests to the backend API, and it will return the results of the operations as User objects or lists of User objects.
    // The service will be registered as a singleton in the dependency injection container, so it will be available throughout the application.
    // The service will also provide a method to create a new user, which will send a POST request to the backend API with the user's information in the request body.
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