using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Converters
{
    public class AuthorsToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<BookAuthor> bookAuthors && bookAuthors.Any())
            {
                return string.Join(", ", bookAuthors.Select(ba => ba.Author.FullName));
            }
            return "Нет авторов";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GenresToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection<BookGenre> bookGenres && bookGenres.Any())
            {
                return string.Join(", ", bookGenres.Select(bg => bg.Genre.Name));
            }
            return "Нет жанров";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}