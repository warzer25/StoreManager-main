using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class ProductService(HttpClient http)
{
    private const string BasePath = "api/products";

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<List<Product>>(BasePath) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            return await http.GetFromJsonAsync<Product>($"{BasePath}/{id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading product {id}: {ex.Message}");
            throw;
        }
    }

    public async Task<Product?> CreateAsync(CreateProductRequest request)
    {
        try
        {
            var response = await http.PostAsJsonAsync(BasePath, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating product: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateProductRequest request)
    {
        try
        {
            var response = await http.PutAsJsonAsync($"{BasePath}/{request.Id}", request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating product {request.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            var response = await http.DeleteAsync($"{BasePath}/{id}");
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting product {id}: {ex.Message}");
            throw;
        }
    }
}