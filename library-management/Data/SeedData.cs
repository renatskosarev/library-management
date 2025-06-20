using library_management.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace library_management.Data;

public static class SeedData
{
    public static async Task SeedDatabaseAsync(LibraryDbContext context)
    {
        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Add Publishers
        var publishers = new List<Publisher>
        {
            new Publisher { Name = "Penguin Books", Address = "London, UK" },
            new Publisher { Name = "HarperCollins", Address = "New York, USA" },
            new Publisher { Name = "Eksmo", Address = "Moscow, Russia" }
        };
        await context.Publishers.AddRangeAsync(publishers);
        await context.SaveChangesAsync();

        // Add Categories
        var categories = new List<Category>
        {
            new Category { Name = "Fiction", Description = "Fictional works that contain imaginary events and people." },
            new Category { Name = "Science Fiction", Description = "Stories based on advanced science, technology, and futuristic concepts." },
            new Category { Name = "Fantasy", Description = "Books set in imaginary worlds with magical elements." },
            new Category { Name = "Mystery", Description = "Novels focused on solving a crime or uncovering secrets." },
            new Category { Name = "Romance", Description = "Stories centered on love and romantic relationships." },
            new Category { Name = "Biography", Description = "Books that tell the life story of a real person." }
        };
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        // Add Authors
        var authors = new List<Author>
        {
            new Author { Name = "J.K. Rowling", Biography = "British author best known for the Harry Potter series" },
            new Author { Name = "George R.R. Martin", Biography = "American novelist and short story writer" },
            new Author { Name = "Agatha Christie", Biography = "English writer known for her detective novels" },
            new Author { Name = "Stephen King", Biography = "American author of horror and supernatural fiction" },
            new Author { Name = "Jane Austen", Biography = "English novelist known for romantic fiction" }
        };
        await context.Authors.AddRangeAsync(authors);
        await context.SaveChangesAsync();

        // Add Books
        var books = new List<Book>
        {
            new Book 
            { 
                Title = "Harry Potter and the Philosopher's Stone",
                Description = "The first book in the Harry Potter series",
                PublicationYear = 1997,
                PublisherId = publishers[0].Id
            },
            new Book 
            { 
                Title = "A Game of Thrones",
                Description = "The first book in A Song of Ice and Fire series",
                PublicationYear = 1996,
                PublisherId = publishers[1].Id
            },
            new Book 
            { 
                Title = "Murder on the Orient Express",
                Description = "A detective novel featuring Hercule Poirot",
                PublicationYear = 1934,
                PublisherId = publishers[0].Id
            },
            new Book 
            { 
                Title = "The Shining",
                Description = "A horror novel by Stephen King",
                PublicationYear = 1977,
                PublisherId = publishers[1].Id
            },
            new Book 
            { 
                Title = "Pride and Prejudice",
                Description = "A romantic novel of manners",
                PublicationYear = 1813,
                PublisherId = publishers[2].Id
            }
        };
        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();

        // Add Book-Author relationships
        var bookAuthors = new List<BookAuthor>
        {
            new BookAuthor { BookId = books[0].Id, AuthorId = authors[0].Id },
            new BookAuthor { BookId = books[1].Id, AuthorId = authors[1].Id },
            new BookAuthor { BookId = books[2].Id, AuthorId = authors[2].Id },
            new BookAuthor { BookId = books[3].Id, AuthorId = authors[3].Id },
            new BookAuthor { BookId = books[4].Id, AuthorId = authors[4].Id }
        };
        await context.BookAuthors.AddRangeAsync(bookAuthors);

        // Add Book-Category relationships
        var bookCategories = new List<BookCategory>
        {
            new BookCategory { BookId = books[0].Id, CategoryId = categories[2].Id }, // Harry Potter - Fantasy
            new BookCategory { BookId = books[1].Id, CategoryId = categories[2].Id }, // Game of Thrones - Fantasy
            new BookCategory { BookId = books[2].Id, CategoryId = categories[3].Id }, // Murder on the Orient Express - Mystery
            new BookCategory { BookId = books[3].Id, CategoryId = categories[0].Id }, // The Shining - Fiction
            new BookCategory { BookId = books[4].Id, CategoryId = categories[4].Id }  // Pride and Prejudice - Romance
        };
        await context.BookCategories.AddRangeAsync(bookCategories);

        // Add Readers
        var readers = new List<Reader>
        {
            new Reader { Name = "John Doe", Email = "john@example.com", Phone = "+1234567890" },
            new Reader { Name = "Jane Smith", Email = "jane@example.com", Phone = "+0987654321" },
            new Reader { Name = "Bob Johnson", Email = "bob@example.com", Phone = "+1122334455" }
        };
        await context.Readers.AddRangeAsync(readers);
        await context.SaveChangesAsync();

        // Add some Bookings
        var bookings = new List<Booking>
        {
            new Booking 
            { 
                BookId = books[0].Id,
                ReaderId = readers[0].Id,
                StartDate = DateTime.UtcNow.AddDays(-10),
                ReturnDate = DateTime.UtcNow.AddDays(-4)
            },
            new Booking 
            { 
                BookId = books[1].Id,
                ReaderId = readers[1].Id,
                StartDate = DateTime.UtcNow.AddDays(-5),
                ReturnDate = DateTime.UtcNow.AddDays(-2)
            },
            new Booking 
            { 
                BookId = books[2].Id,
                ReaderId = readers[2].Id,
                StartDate = DateTime.UtcNow.AddDays(-3),
                ReturnDate = DateTime.UtcNow.AddDays(-8)
            }
        };
        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();
    }
} 