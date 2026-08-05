using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

namespace StoreManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]  // 👈 Only Admin can access products
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]  // 👈 Only Admin can view list
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
    {
        var products = await db.Products
            .Include(p => p.Category)
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
    [Authorize(Roles = "admin")]  // 👈 Only Admin can view single product
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
    [Authorize(Roles = "admin")]  // ✅ Already correct
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
    [Authorize(Roles = "admin")]  // ✅ Already correct
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
    [Authorize(Roles = "admin")]  // ✅ Already correct
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