using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StoreManager.Api.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }
}
