using CatalogProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogProject.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController(ApplicationDbContext context, ILogger<BooksController> logger) : ControllerBase
{
    [HttpGet]
    public List<Book?> GetBooks()
    {
        logger.LogInformation("Getting all books");
        return context.Books.ToList();
    }
    
    [HttpGet("{id:int}")]
    public Book? GetBook(int id)
    {
        logger.LogInformation("Getting book {id}", id);
        var book = context.Books.Find(id);
        if (book is not null) return book;
        Response.StatusCode = StatusCodes.Status404NotFound;
        logger.LogInformation("Book with {id} not found", id);
        return null;
    }

    [HttpPost]
    public void PostBook([FromBody] Book book)
    {
        logger.LogInformation("Posting book {id}", book.Id);
        context.Books.Add(book);
        context.SaveChanges();
    }

    [HttpPut("{id:int}")]
    public void PutBook(int id, [FromBody] Book book)
    {
        if (id != book.Id)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            logger.LogInformation("Book with {id} not found", id);
            return;
        }

        var existingBook = context.Books.Find(id);
        if (existingBook is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            logger.LogInformation("Book with {id} not found", id);
            return;
        }

        logger.LogInformation("Updating book {id}", book.Id);
        context.Entry(existingBook).CurrentValues.SetValues(book);
        context.SaveChanges();
        Response.StatusCode = StatusCodes.Status204NoContent;
    }

    [HttpDelete("{id:int}")]
    public void DeleteBook(int id)
    {
        logger.LogInformation("Deleting book {id}", id);
        context.Books.Remove(context.Books.Find(id));
        context.SaveChanges();
    }
}