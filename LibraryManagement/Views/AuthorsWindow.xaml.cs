using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class AuthorsWindow : Window
    {
        private readonly LibraryContext _context;

        public AuthorsWindow(LibraryContext context)
        {
            InitializeComponent();
            _context = context;
            LoadData();
        }

        private void LoadData()
        {
            _context.Authors.Include(a => a.BookAuthors).ThenInclude(ba => ba.Book).Load();
            AuthorsDataGrid.ItemsSource = _context.Authors.Local.ToObservableCollection();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AuthorDialog();
            if (dialog.ShowDialog() == true)
            {
                _context.Authors.Add(dialog.Author);
                _context.SaveChanges();
                LoadData();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var author = AuthorsDataGrid.SelectedItem as Author;
            if (author != null)
            {
                var dialog = new AuthorDialog(author);
                if (dialog.ShowDialog() == true)
                {
                    _context.Entry(author).CurrentValues.SetValues(dialog.Author);
                    _context.SaveChanges();
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите автора для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var author = AuthorsDataGrid.SelectedItem as Author;
            if (author != null)
            {
                // Проверяем, есть ли у автора книги через связующую таблицу
                if (author.BookAuthors != null && author.BookAuthors.Any())
                {
                    MessageBox.Show("Нельзя удалить автора, у которого есть книги", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить автора {author.FullName}?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Authors.Remove(author);
                    _context.SaveChanges();
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите автора для удаления", "Информация",
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