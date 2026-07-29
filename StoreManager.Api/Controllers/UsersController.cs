using Microsoft.AspNetCore.Authorization;  // 👈 IMPORTANT: Add this using
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

namespace StoreManager.Api.Controllers;

// this controller is responsible for managing users in the system.
// It provides endpoints for retrieving, creating, and deleting users.
// Access to these endpoints is controlled based on user roles (admin and cashier).

[ApiController]
[Route("api/[controller]")]
[Authorize]  
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "admin,cashier")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        // this endpoint returns a list of users in the system.
        // Only users with the "admin" or "cashier" role can access this endpoint.
        var users = await _context.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost]
    [Authorize(Roles = "admin")] 
    public async Task<ActionResult<UserResponse>> CreateUser(CreateUserRequest request)
    {
        // this endpoint allows an admin to create a new user in the system.
        // It checks if the email already exists, hashes the password, and saves the new user to the database.

        var existing = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (existing)
        {
            return BadRequest(new { message = "Email already exists." });
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, response);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]  
    public async Task<IActionResult> DeleteUser(int id)
    {
        // this endpoint allows an admin to delete a user from the system.
        // It checks if the user exists and removes them from the database.
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UserResponse
{
    // this class represents the response model for user data returned by the API.
    // It includes the user's ID, name, email, role, and creation date.
    // Note: PasswordHash is not included in the response for security reasons.
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    // this class represents the request model for creating a new user.
    
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}