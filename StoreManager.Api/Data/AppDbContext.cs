using System.Text;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Models;

namespace StoreManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

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
