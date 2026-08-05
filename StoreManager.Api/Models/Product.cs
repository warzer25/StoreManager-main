using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManager.Api.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("barcode")]
    [MaxLength(100)]
    public string? Barcode { get; set; }

    [Column("unit")]
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;  // 'piece', 'kg' or 'litre'

    [Column("cost_price")]
    [Required]
    public decimal CostPrice { get; set; }

    [Column("selling_price")]
    [Required]
    public decimal SellingPrice { get; set; }

    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    [ForeignKey(nameof(CategoryId))]
    public virtual Category? Category { get; set; }
}

// DTOs
public class ProductResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateProductRequest
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
}

public class UpdateProductRequest
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
}