using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StoreManager.Api.Data;

// i have problem runing new version of JwtBearer so i used older version hope this dont casue problem ahead
// this is the main entry point for the StoreManager API application.
// It configures the services and middleware for the application,
// including database context, CORS policy, JWT authentication, controllers, and Swagger documentation.
var builder = WebApplication.CreateBuilder(args);

// Database Configuration
var provider = builder.Configuration["DaPackage restore failed. Rolling back package changes for 'StoreManager.Web'.\r\nWarning As Error: Detected package downgrade: System.IdentityModel.Tokens.Jwt from 7.1.2 to 6.35.0. Reference the package directly from the project to select a different version. \r\n StoreManager.Api -> Microsoft.AspNetCore.Authentication.JwtBearer 8.0.19 -> Microsoft.IdentityModel.Protocols.OpenIdConnect 7.1.2 -> System.IdentityModel.Tokens.Jwt (>= 7.1.2) \r\n StoreManager.Api -> System.IdentityModel.Tokens.Jwt (>= 6.35.0)\r\nWarning As Error: Detected package downgrade: System.IdentityModel.Tokens.Jwt from 7.1.2 to 6.35.0. Reference the package directly from the project to select a different version. \r\n StoreManager.Web -> Microsoft.AspNetCore.Authentication.JwtBearer 8.0.19 -> Microsoft.IdentityModel.Protocols.OpenIdConnect 7.1.2 -> System.IdentityModel.Tokens.Jwt (>= 7.1.2) \r\n StoreManager.Web -> System.IdentityModel.Tokens.Jwt (>= 6.35.0)tabaseProvider"] ?? "SqlServer";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

// this is the CORS policy configuration for the StoreManager API application.
const string CorsPolicy = "AllowWebApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// this is the JWT authentication configuration for the StoreManager API application.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Add JWT Authorization to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter token: Bearer {your JWT token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();  // 👈 Add this
app.UseAuthorization();   // 👈 Add this
app.MapControllers();

app.Run();