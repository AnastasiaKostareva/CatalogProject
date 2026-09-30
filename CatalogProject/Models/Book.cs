using System.ComponentModel.DataAnnotations;

namespace CatalogProject.Models;

public class Book
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название обязательно")]
    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;
    
    [Url(ErrorMessage = "Некорректный URL")]
    [MaxLength(256)]
    public string? ImageUrl { get; set; }
    
    [Required(ErrorMessage = "Язык обязателен")]
    [MaxLength(32)]
    public string Language { get; set; } = string.Empty;
    
    [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
    public int Rating { get; set; }
    
    public string? Summary { get; set; }
    
    [Required(ErrorMessage = "Автор обязателен")]
    [MaxLength(64)]
    public string Author { get; set; } = string.Empty;
    
    [Range(1, 2026, ErrorMessage = "Год должен быть от 1 до 2026")]
    public int? ReleaseYear { get; set; }
    
    public bool InProcess { get; set; }
    
    [MaxLength(32)]
    public string? Genre { get; set; }
}