using System.Text;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Models;

namespace StoreManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // this is a DbContext class for the StoreManager API, which represents the database context for the application.
    // It inherits from the DbContext class provided by Entity Framework Core and is configured with options passed in through the constructor.
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This method is called when the model for the context is being created.
        // It allows you to configure the model and its relationships using the ModelBuilder API.

        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // This loop iterates through all the entity types in the model and sets their table names and column names to snake_case format.
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }


        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.SellingPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Category>().HasData(
            // this is a seed data for the Category entity,
            // which creates four initial categories in the database: Beverages, Snacks, Dairy, and Cleaning.
            new Category { Id = 1, Name = "Beverages", Description = "Drinks and soft drinks" },
            new Category { Id = 2, Name = "Snacks", Description = "Chips and packaged snacks" },
            new Category { Id = 3, Name = "Dairy", Description = "Milk, cheese and eggs" },
            new Category { Id = 4, Name = "Cleaning", Description = "Household cleaning supplies" }
        );

        modelBuilder.Entity<User>().HasData(
            // this is a seed data for the User entity, which creates two initial users in the database: an admin user and a cashier user.
            new User
            {
                Id = 1,
                Name = "Admin User1",
                Email = "admin@store.com",
                PasswordHash = "$2b$11$JOKYDcWaehIEGxW1xr.PWOnPOmMyW6Boj/GuU.CLa.SjKfkOSPkji", // "Admin123!"
                Role = "admin",
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                Name = "Cashier User1",
                Email = "cashier@store.com",
                PasswordHash = "$2b$11$5HgYUs.stb2fg.5cDhrW8u8ze8ZkjXnX0LdbWkfghpulfg/71Xire", // "Cashier123!"
                Role = "cashier",
                CreatedAt = DateTime.UtcNow
            }
        );
        // 👇 ADD PRODUCT SEED DATA
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                CategoryId = 1,  // Beverages
                Name = "Cola Can",
                Barcode = "123456789",
                Unit = "pcs",
                CostPrice = 1.50m,
                SellingPrice = 2.50m,
                StockQuantity = 45,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = 2,
                CategoryId = 2,  // Snacks
                Name = "Potato Chips",
                Barcode = "987654321",
                Unit = "pcs",
                CostPrice = 1.80m,
                SellingPrice = 3.00m,
                StockQuantity = 8,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = 3,
                CategoryId = 3,  // Dairy
                Name = "Milk 1L",
                Barcode = "456789123",
                Unit = "liter",
                CostPrice = 3.00m,
                SellingPrice = 4.50m,
                StockQuantity = 12,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            },
            new Product
            {
                Id = 4,
                CategoryId = 4,  // Cleaning
                Name = "Cleaning Spray",
                Barcode = "789123456",
                Unit = "pcs",
                CostPrice = 4.00m,
                SellingPrice = 6.00m,
                StockQuantity = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            }
        );
    }

    private static string ToSnakeCase(string input)
    {
        // This method converts a given string to snake_case format.
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder(input.Length + 8);
        for (var i = 0; i < input.Length; i++)
        {
            // This loop iterates through each character in the input string and checks if it is an uppercase letter.
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    builder.Append('_');
                }
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}