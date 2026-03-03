using System.Windows;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class GenreDialog : Window
    {
        public Genre Genre { get; private set; }

        public GenreDialog(Genre genre = null)
        {
            InitializeComponent();
            
            Genre = genre == null ? new Genre() : new Genre
            {
                Id = genre.Id,
                Name = genre.Name,
                Description = genre.Description
            };
            
            DataContext = this;
            Title = Genre.Id == 0 ? "Добавление жанра" : "Редактирование жанра";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Genre.Name))
            {
                MessageBox.Show("Введите название жанра", "Ошибка", 
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