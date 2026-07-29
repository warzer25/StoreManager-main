using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class CategoryService(HttpClient http)
{
    // This service will handle CRUD operations for categories in the application.
    // It will communicate with the backend API to perform create, read, update, and delete operations for categories.
    // The service will use an HttpClient to send requests to the backend API, and it will return the results of the operations as Category objects or lists of Category objects.
    // The service will be registered as a singleton in the dependency injection container, so it will be available throughout the application.
    // The service will also provide a method to search for categories by name, which will send a GET request to the backend API with a query parameter for the search term.
    // The service will return the results of the search as a list of Category objects.
    private const string BasePath = "api/categories";

    public async Task<List<Category>> GetAllAsync(string? term = null)
    {
        var url = BasePath;
        if (!string.IsNullOrWhiteSpace(term))
        {
            url += $"?term={Uri.EscapeDataString(term)}";
        }
        return await http.GetFromJsonAsync<List<Category>>(url) ?? new List<Category>();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await http.GetFromJsonAsync<Category>($"{BasePath}/{id}");
    }

    public async Task<Category?> CreateAsync(Category category)
    {
        var response = await http.PostAsJsonAsync(BasePath, category);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Category>();
    }

    public async Task UpdateAsync(Category category)
    {
        var response = await http.PutAsJsonAsync($"{BasePath}/{category.Id}", category);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var response = await http.DeleteAsync($"{BasePath}/{id}");
        response.EnsureSuccessStatusCode();
    }
}
