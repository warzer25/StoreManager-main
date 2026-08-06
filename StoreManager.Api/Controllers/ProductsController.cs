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
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(
    [FromQuery] int? categoryId = null,
    [FromQuery] string? term = null,
    [FromQuery] string? stockFilter = null)  // 👈 NEW: stock filter
    {
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

        // 👇 NEW: Stock filter
        if (!string.IsNullOrWhiteSpace(stockFilter))
        {
            switch (stockFilter.ToLower())
            {
                case "instock":
                    query = query.Where(p => p.StockQuantity > 10);
                    break;
                case "lowstock":
                    query = query.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 10);
                    break;
                case "outofstock":
                    query = query.Where(p => p.StockQuantity <= 0);
                    break;
                    // "all" - no filter
            }
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse
            {
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

    // This endpoint retrieves a single product by its ID, including its category information.
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        // If the product is not found, return a 404 Not Found response

        if (product is null)
        {
            return NotFound();
        }

        // Map the product entity to the response DTO and return it
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

    // This endpoint creates a new product based on the provided request data. It returns the created product with its ID and category information.
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request)
    {

        // Check if the category exists before creating the product
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


        // Add the new product to the database and save changes
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

    // This endpoint updates an existing product based on the provided ID and request data. It returns a 204 No Content response if the update is successful, or appropriate error responses if the product is not found or if the route ID and body ID do not match.
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {

        // Check if the route ID and body ID match to ensure data integrity
        if (id != request.Id)
        {
            return BadRequest("Route id and body id do not match.");
        }
        // Find the existing product in the database by its ID
        var existing = await db.Products.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        // Update the existing product's properties with the new values from the request
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

    //  This endpoint deletes an existing product based on the provided ID. It returns a 204 No Content response if the deletion is successful, or a 404 Not Found response if the product does not exist.
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        // If the product is not found, return a 404 Not Found response
        if (product is null)
        {
            return NotFound();
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();

        return NoContent();
    }
}