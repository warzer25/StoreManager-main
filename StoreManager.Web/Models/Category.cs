using System.ComponentModel.DataAnnotations;

namespace StoreManager.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name can be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description can be at most 255 characters.")]
    public string? Description { get; set; }
}
