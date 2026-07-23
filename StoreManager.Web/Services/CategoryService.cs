using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class CategoryService(HttpClient http)
{
    private const string BasePath = "api/categories";

    public async Task<List<Category>> GetAllAsync()
    {
        return await http.GetFromJsonAsync<List<Category>>(BasePath) ?? new List<Category>();
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
