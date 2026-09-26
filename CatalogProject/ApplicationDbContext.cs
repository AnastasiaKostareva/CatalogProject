using CatalogProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogProject;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book?> Books { get; set; }
}