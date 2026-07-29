using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

// this is a minimal API controller for managing categories in the StoreManager application.
// It provides endpoints for CRUD operations and supports searching categories by name or description.

namespace StoreManager.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAll([FromQuery] string? term = null)
    {
        // this is our base query, it will be modified based on the search term if provided
        IQueryable<Category> query = db.Categories;

        
        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowerTerm = term.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lowerTerm) ||
                                     (c.Description != null && c.Description.ToLower().Contains(lowerTerm)));
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        // Use FindAsync to retrieve the category by its primary key (id)
        var category = await db.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create(Category category)
    {
        // Ensure the Id is set to 0 so that EF Core treats this as a new entity
        category.Id = 0;
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Category category)
    {
        // Check if the route id matches the category id in the body
        if (id != category.Id)
        {
            return BadRequest("Route id and body id do not match.");
        }

        var existing = await db.Categories.FindAsync(id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Name = category.Name;
        existing.Description = category.Description;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Use FindAsync to retrieve the category by its primary key (id)
        var category = await db.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
