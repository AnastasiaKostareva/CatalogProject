using Prometheus;

namespace CatalogProject.Metrics;

public class BookMetrics
{
    private static readonly Gauge PublishedBooksGauge = Prometheus.Metrics
        .CreateGauge("publish_processed_current", "Current number of published books");

    private static readonly Counter ViewBookCounter = Prometheus.Metrics
        .CreateCounter("view_books_total", "Count of viewed books", new CounterConfiguration
        {
            LabelNames = new[] { "genre" }
        });
    
    private static readonly Counter RequestListBooksCounter = Prometheus.Metrics
        .CreateCounter("request_list_books_total", "Count of request list books");
    
    private static readonly Counter BookNotFoundCounter = Prometheus.Metrics
        .CreateCounter("book_not_found_total", "Count of books not found");

    public static void RecordBook()
    {
        PublishedBooksGauge.Inc();
    }
    
    public static void DeleteBook()
    {
        PublishedBooksGauge.Dec();
    }

    public static void ViewBook(string? genre)
    {
        ViewBookCounter.WithLabels(genre ?? "unknown").Inc();
    }

    public static void RequestListBook()
    {
        RequestListBooksCounter.Inc();
    }

    public static void NotFound()
    {
        BookNotFoundCounter.Inc();
    }
}