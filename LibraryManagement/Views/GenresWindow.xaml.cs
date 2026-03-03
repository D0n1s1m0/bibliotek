using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class GenresWindow : Window
    {
        private readonly LibraryContext _context;

        public GenresWindow(LibraryContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            _context.Genres.Include(g => g.BookGenres).Load();
            GenresDataGrid.ItemsSource = _context.Genres.Local.ToObservableCollection();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new GenreDialog();
            if (dialog.ShowDialog() == true)
            {
                var existingGenre = _context.Genres.Local
                    .FirstOrDefault(g => g.Name.ToLower() == dialog.Genre.Name.ToLower());

                if (existingGenre != null)
                {
                    MessageBox.Show("Жанр с таким названием уже существует!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Genres.Add(dialog.Genre);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var genre = GenresDataGrid.SelectedItem as Genre;
            if (genre != null)
            {
                var dialog = new GenreDialog(genre);
                if (dialog.ShowDialog() == true)
                {
                    var existingGenre = _context.Genres.Local
                        .FirstOrDefault(g => g.Name.ToLower() == dialog.Genre.Name.ToLower()
                            && g.Id != genre.Id);

                    if (existingGenre != null)
                    {
                        MessageBox.Show("Жанр с таким названием уже существует!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _context.Entry(genre).CurrentValues.SetValues(dialog.Genre);
                    _context.SaveChanges();
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите жанр для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var genre = GenresDataGrid.SelectedItem as Genre;
            if (genre != null)
            {
                if (genre.BookGenres != null && genre.BookGenres.Any())
                {
                    MessageBox.Show("Нельзя удалить жанр, у которого есть книги", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить жанр {genre.Name}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Genres.Remove(genre);
                    _context.SaveChanges();
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите жанр для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ДОБАВЛЕНО: метод для кнопки закрытия
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}