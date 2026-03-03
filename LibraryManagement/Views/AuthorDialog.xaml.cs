using System;
using System.Windows;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class AuthorDialog : Window
    {
        public Author Author { get; private set; }

        public AuthorDialog(Author author = null)
        {
            InitializeComponent();
            
            Author = author == null ? new Author { BirthDate = DateTime.Now } : new Author
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName,
                BirthDate = author.BirthDate,
                Country = author.Country
            };
            
            DataContext = this;
            Title = Author.Id == 0 ? "Добавление автора" : "Редактирование автора";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Author.FirstName))
            {
                MessageBox.Show("Введите имя автора", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Author.LastName))
            {
                MessageBox.Show("Введите фамилию автора", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}