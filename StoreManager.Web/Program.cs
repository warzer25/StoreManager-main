using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using StoreManager.Web;
using StoreManager.Web.Services;

// this is the entry point of the Blazor WebAssembly application. It sets up the application, registers services, and configures the root components.
// The application uses MudBlazor for UI components and styling, and it communicates with a backend API for data operations.
// The application is built using .NET 8.0 and C# 12.0, and it follows best practices for dependency injection and service registration.
// The application is designed to be modular and maintainable, with separate services for authentication, category management, and user management.

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5136/")
});

// Register Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<UserService>();  
builder.Services.AddMudServices();
builder.Services.AddScoped<ProductService>();

await builder.Build().RunAsync();