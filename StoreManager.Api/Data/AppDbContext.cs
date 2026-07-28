using System.Text;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Models;

namespace StoreManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }
        }

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Beverages", Description = "Drinks and soft drinks" },
            new Category { Id = 2, Name = "Snacks", Description = "Chips and packaged snacks" },
            new Category { Id = 3, Name = "Dairy", Description = "Milk, cheese and eggs" },
            new Category { Id = 4, Name = "Cleaning", Description = "Household cleaning supplies" }
        );

        modelBuilder.Entity<User>().HasData(
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
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var builder = new StringBuilder(input.Length + 8);
        for (var i = 0; i < input.Length; i++)
        {
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