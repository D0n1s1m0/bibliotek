using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LibraryManagement.Models;
using System;

namespace LibraryManagement.Data
{
    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=LibraryDB.db");
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка сущности Author
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(e => e.Country)
                    .HasMaxLength(100);
            });

            // Настройка сущности Genre
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                
                entity.Property(e => e.Description)
                    .HasMaxLength(500);
            });

            // Настройка сущности Book
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.ISBN)
                    .HasMaxLength(20);
                
                entity.Property(e => e.PublishYear)
                    .IsRequired();
                
                entity.Property(e => e.QuantityInStock)
                    .IsRequired()
                    .HasDefaultValue(0);
                
                entity.HasIndex(e => e.Title);
            });

            // Настройка связующей таблицы BookAuthor
            modelBuilder.Entity<BookAuthor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasOne(e => e.Book)
                    .WithMany(e => e.BookAuthors)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Author)
                    .WithMany(e => e.BookAuthors)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.BookId, e.AuthorId }).IsUnique();
            });

            // Настройка связующей таблицы BookGenre
            modelBuilder.Entity<BookGenre>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                
                entity.HasOne(e => e.Book)
                    .WithMany(e => e.BookGenres)
                    .HasForeignKey(e => e.BookId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Genre)
                    .WithMany(e => e.BookGenres)
                    .HasForeignKey(e => e.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.BookId, e.GenreId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}