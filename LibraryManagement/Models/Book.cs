using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibraryManagement.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название книги обязательно")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Название должно содержать от 1 до 200 символов")]
        public string Title { get; set; }

        [Range(1000, 2100, ErrorMessage = "Год издания должен быть между 1000 и 2100")]
        [Display(Name = "Год издания")]
        public int PublishYear { get; set; }

        private string _isbn;

        [Display(Name = "ISBN")]
        public string ISBN
        {
            get => _isbn;
            set => _isbn = value?.Replace("-", "").Replace(" ", ""); // Убираем дефисы и пробелы
        }

        [Range(0, 1000, ErrorMessage = "Количество должно быть от 0 до 1000")]
        [Display(Name = "Количество в наличии")]
        public int QuantityInStock { get; set; }

        // Навигационные свойства для связи многие-ко-многим
        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
        public virtual ICollection<BookGenre> BookGenres { get; set; }

        // Вспомогательные свойства для отображения
        public string AuthorNames => BookAuthors != null
            ? string.Join(", ", BookAuthors.Select(ba => ba.Author?.FullName ?? "Unknown"))
            : string.Empty;

        public string GenreNames => BookGenres != null
            ? string.Join(", ", BookGenres.Select(bg => bg.Genre?.Name ?? "Unknown"))
            : string.Empty;

        // Метод валидации ISBN
        public static ValidationResult ValidateISBN(string isbn, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return ValidationResult.Success; // ISBN необязателен

            // Убираем дефисы и пробелы
            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

            // Проверка ISBN-10
            if (cleanIsbn.Length == 10)
            {
                if (Regex.IsMatch(cleanIsbn, @"^\d{9}[\dX]$"))
                {
                    int sum = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        sum += (i + 1) * int.Parse(cleanIsbn[i].ToString());
                    }

                    char lastChar = cleanIsbn[9];
                    int last = lastChar == 'X' ? 10 : int.Parse(lastChar.ToString());
                    sum += 10 * last;

                    if (sum % 11 == 0)
                        return ValidationResult.Success;
                }
                return new ValidationResult("Неверный формат ISBN-10");
            }

            // Проверка ISBN-13
            if (cleanIsbn.Length == 13)
            {
                if (Regex.IsMatch(cleanIsbn, @"^\d{13}$"))
                {
                    int sum = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        int digit = int.Parse(cleanIsbn[i].ToString());
                        sum += (i % 2 == 0) ? digit : digit * 3;
                    }

                    int checksum = (10 - (sum % 10)) % 10;
                    int lastDigit = int.Parse(cleanIsbn[12].ToString());

                    if (checksum == lastDigit)
                        return ValidationResult.Success;
                }
                return new ValidationResult("Неверный формат ISBN-13");
            }

            return new ValidationResult("ISBN должен содержать 10 или 13 цифр");
        }
    }
}