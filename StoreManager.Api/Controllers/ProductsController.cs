using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

namespace StoreManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll([FromQuery] int? categoryId = null,[FromQuery] string? term = null)
    {
        // this query will include the category information for each product
        // and will filter by category if provided, and search by term if provided
        // the search will be case-insensitive and will match the term against the product name, barcode, or category name
        var query = db.Products
            .Include(p => p.Category)
            .AsQueryable();
        // Filter by category if provided
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Search by term (name, barcode, or category name)
        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowerTerm = term.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(lowerTerm) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(lowerTerm)) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(lowerTerm))
            );
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse
            {
                // Map the product entity to the response DTO
                // If the category is null, we will return "Unknown" as the category name
                // This can happen if the category was deleted but the product still exists
                // We can also consider returning a 404 if the category is null, but for now we will return "Unknown"
                // This is a design decision and can be changed later if needed
                // p.id is the primary key of the product, and p.CategoryId is the foreign key to the category
                // the p. is the navigation property to the category, which we included in the query 
                Id = p.Id,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Unknown",
                Name = p.Name,
                Barcode = p.Barcode,
                Unit = p.Unit,
                CostPrice = p.CostPrice,
                SellingPrice = p.SellingPrice,
                StockQuantity = p.StockQuantity,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(new ProductResponse
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "Unknown",
            Name = product.Name,
            Barcode = product.Barcode,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Barcode = request.Barcode,
            Unit = request.Unit,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        await db.Entry(product).Reference(p => p.Category).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, new ProductResponse
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? "Unknown",
            Name = product.Name,
            Barcode = product.Barcode,
            Unit = product.Unit,
            CostPrice = product.CostPrice,
            SellingPrice = product.SellingPrice,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest("Route id and body id do not match.");
        }

        var existing = await db.Products.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.CategoryId = request.CategoryId;
        existing.Name = request.Name;
        existing.Barcode = request.Barcode;
        existing.Unit = request.Unit;
        existing.CostPrice = request.CostPrice;
        existing.SellingPrice = request.SellingPrice;
        existing.StockQuantity = request.StockQuantity;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return NoContent();
    }
}