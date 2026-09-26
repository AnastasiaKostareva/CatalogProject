namespace CatalogProject.Models;

public class Book
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Language { get; set; }
    public int Rating { get; set; }
    public string? Summary { get; set; }
}