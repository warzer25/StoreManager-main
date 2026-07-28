using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

namespace StoreManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Log the attempt
        _logger.LogInformation("Login attempt for email: {Email}", request?.Email);

        if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        try
        {
            // Find user by email (case-insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify password with BCrypt
            bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Return user data (excluding password hash)
            var response = new LoginResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            _logger.LogInformation("User logged in successfully: {Email} [{Role}]", user.Email, user.Role);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}