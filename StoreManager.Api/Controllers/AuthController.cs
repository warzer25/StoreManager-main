using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StoreManager.Api.Data;
using StoreManager.Api.Models;

namespace StoreManager.Api.Controllers;
// This controller handles user authentication, including login and JWT token generation.
// It provides an endpoint for users to log in with their email and password, and returns a JWT token upon successful authentication.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Dependencies injected via constructor
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, ILogger<AuthController> logger, IConfiguration configuration)
    {
        // Initialize dependencies
        // Assign the injected dependencies to private fields for use in the controller methods
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
       
        _logger.LogInformation("Login attempt for email: {Email}", request?.Email);

        if (string.IsNullOrEmpty(request?.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user == null)
            {
                _logger.LogWarning("User not found: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify the password using BCrypt , which is a secure way to handle password hashing and verification
            bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password for: {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // this will Generate JWT Token
            var token = GenerateJwtToken(user);

            var response = new LoginResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token  // Include token in response
            };

            _logger.LogInformation("User logged in: {Email} [{Role}]", user.Email, user.Role);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    private string GenerateJwtToken(User user)
    {
        // Retrieve JWT settings from configuration
        // Ensure that the SecretKey is present; throw an exception if it's missing
        // Create claims for the JWT token, including user ID, name, email, and role
        // Create a symmetric security key using the secret key

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey missing"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)  // 👈 Role claim for authorization
        };

        var key = new SymmetricSecurityKey(secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}