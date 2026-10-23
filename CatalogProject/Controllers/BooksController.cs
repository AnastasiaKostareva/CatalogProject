using CatalogProject.Metrics;
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
        BookMetrics.RequestListBook();
        return context.Books.ToList();
    }
    
    [HttpGet("{id:int}")]
    public Book? GetBook(int id)
    {
        logger.LogInformation("Getting book {id}", id);
        var book = context.Books.Find(id);
        if (book is not null)
        {
            BookMetrics.ViewBook(book.Genre);
            return book;
        }
        Response.StatusCode = StatusCodes.Status404NotFound;
        logger.LogInformation("Book with {id} not found", id);
        BookMetrics.NotFound();
        return null;
    }

    [HttpPost]
    public ActionResult<Book> PostBook([FromBody] Book book)
    {
        logger.LogInformation("Posting book {id}", book.Id);
        context.Books.Add(book);
        context.SaveChanges();
        BookMetrics.RecordBook();
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Book> PutBook(int id, [FromBody] Book book)
    {
        if (id != book.Id)
        {
            logger.LogWarning("Book update rejected: id mismatch. UrlId={UrlId}, BodyId={BodyId}", id, book.Id);
            return BadRequest("Id in URL does not match Id in the body");
        }

        var existingBook = context.Books.Find(id);
        if (existingBook is null)
        {
            logger.LogInformation("Book with {id} not found", id);
            BookMetrics.NotFound();
            return NotFound();
        }

        logger.LogInformation("Updating book {id}", book.Id);
        context.Entry(existingBook).CurrentValues.SetValues(book);
        context.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public void DeleteBook(int id)
    {var existingBook = context.Books.Find(id);
        if (existingBook is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            logger.LogInformation("Book with {id} not found", id);
            BookMetrics.NotFound();
            return;
        }
        logger.LogInformation("Deleting book {id}", id);
        context.Books.Remove(existingBook);
        context.SaveChanges();
        BookMetrics.DeleteBook();
    }
}