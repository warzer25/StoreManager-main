using System.Net.Http.Json;
using StoreManager.Web.Models;

namespace StoreManager.Web.Services;

public class ProductService(HttpClient http)
{
    private const string BasePath = "api/products";

    public async Task<List<Product>> GetAllAsync(string? term = null)
    {
        try
        {
            var url = BasePath;
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(term))
            {
                queryParams.Add($"term={Uri.EscapeDataString(term)}");
            }

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            return await http.GetFromJsonAsync<List<Product>>(url) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products: {ex.Message}");
            throw;
        }
    }
    public async Task<List<Product>> GetByCategoryAsync(int categoryId)
    {
        // Load products by category ID
        // This method fetches products that belong to a specific category.
        // It constructs the request URL with the categoryId as a query parameter and makes an HTTP GET request to retrieve the products.
        // If the request is successful, it returns the list of products; otherwise, it logs the error and rethrows the exception.
        // --
        // await is used to asynchronously wait for the HTTP request to complete, allowing the application to remain responsive during the operation.
        try
        {
            return await http.GetFromJsonAsync<List<Product>>($"{BasePath}?categoryId={categoryId}") ?? new List<Product>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading products for category {categoryId}: {ex.Message}");
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