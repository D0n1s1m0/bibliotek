using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LibraryManagement.Models;

namespace LibraryManagement.Views
{
    public partial class BookWindow : Window, INotifyPropertyChanged
    {
        // Свойства
        private Book _currentBook;
        public Book CurrentBook
        {
            get => _currentBook;
            set
            {
                _currentBook = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSave));
                UpdateIsbnValidation();
            }
        }

        public List<Author> AllAuthors { get; set; }
        public List<Genre> AllGenres { get; set; }

        private List<Author> _selectedAuthors;
        public List<Author> SelectedAuthors
        {
            get => _selectedAuthors;
            set
            {
                _selectedAuthors = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAuthorsDisplay));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private List<Genre> _selectedGenres;
        public List<Genre> SelectedGenres
        {
            get => _selectedGenres;
            set
            {
                _selectedGenres = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedGenresDisplay));
                OnPropertyChanged(nameof(CanSave));
            }
        }

        private string _windowTitle;
        public new string Title
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged();
                base.Title = value;
            }
        }

        public string SelectedAuthorsDisplay => SelectedAuthors != null && SelectedAuthors.Any()
            ? string.Join(", ", SelectedAuthors.Select(a => a.FullName))
            : "Не выбрано";

        public string SelectedGenresDisplay => SelectedGenres != null && SelectedGenres.Any()
            ? string.Join(", ", SelectedGenres.Select(g => g.Name))
            : "Не выбрано";

        private string _isbnValidationMessage;
        public string IsbnValidationMessage
        {
            get => _isbnValidationMessage;
            set { _isbnValidationMessage = value; OnPropertyChanged(); }
        }

        public Brush IsbnValidationColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentBook?.ISBN))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));

                var isValid = IsValidIsbn(CurrentBook.ISBN);
                return isValid
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
        }

        public string IsbnValidationIcon
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentBook?.ISBN))
                    return "⚪";

                var isValid = IsValidIsbn(CurrentBook.ISBN);
                return isValid ? "✓" : "✗";
            }
        }

        public Brush IsbnValidationMessageColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CurrentBook?.ISBN))
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));

                var isValid = IsValidIsbn(CurrentBook.ISBN);
                return isValid
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            }
        }

        public bool CanSave
        {
            get
            {
                return CurrentBook != null &&
                       !string.IsNullOrWhiteSpace(CurrentBook.Title) &&
                       SelectedAuthors != null && SelectedAuthors.Any() &&
                       SelectedGenres != null && SelectedGenres.Any() &&
                       CurrentBook.PublishYear >= 1000 && CurrentBook.PublishYear <= 2100 &&
                       CurrentBook.QuantityInStock >= 0 && CurrentBook.QuantityInStock <= 1000 &&
                       (string.IsNullOrWhiteSpace(CurrentBook.ISBN) || IsValidIsbn(CurrentBook.ISBN));
            }
        }

        public ICommand SelectAuthorsCommand { get; private set; }
        public ICommand SelectGenresCommand { get; private set; }

        public BookWindow(Book book, List<Author> allAuthors, List<Genre> allGenres)
        {
            InitializeComponent();

            CurrentBook = book ?? new Book();
            AllAuthors = allAuthors ?? new List<Author>();
            AllGenres = allGenres ?? new List<Genre>();
            SelectedAuthors = new List<Author>();
            SelectedGenres = new List<Genre>();

            // Загружаем существующие связи
            if (book != null && book.Id > 0)
            {
                if (book.BookAuthors != null)
                {
                    SelectedAuthors = book.BookAuthors
                        .Where(ba => ba.Author != null)
                        .Select(ba => ba.Author)
                        .ToList();
                }

                if (book.BookGenres != null)
                {
                    SelectedGenres = book.BookGenres
                        .Where(bg => bg.Genre != null)
                        .Select(bg => bg.Genre)
                        .ToList();
                }
            }

            SelectAuthorsCommand = new RelayCommand(param => OpenAuthorSelection());
            SelectGenresCommand = new RelayCommand(param => OpenGenreSelection());

            Title = book == null ? "➕ Добавление книги" : "✏️ Редактирование книги";
            DataContext = this;

            UpdateIsbnValidation();
        }

        private void OpenAuthorSelection()
        {
            var items = AllAuthors.Select(a => new MultiSelectItem
            {
                Id = a.Id,
                DisplayName = a.FullName
            }).ToList();

            var selectedIds = SelectedAuthors.Select(a => a.Id).ToList();

            var dialog = new MultiSelectWindow("Выбор авторов", items, selectedIds);
            if (dialog.ShowDialog() == true)
            {
                SelectedAuthors = dialog.SelectedItems
                    .Select(i => AllAuthors.First(a => a.Id == i.Id))
                    .ToList();
            }
        }

        private void OpenGenreSelection()
        {
            var items = AllGenres.Select(g => new MultiSelectItem
            {
                Id = g.Id,
                DisplayName = g.Name
            }).ToList();

            var selectedIds = SelectedGenres.Select(g => g.Id).ToList();

            var dialog = new MultiSelectWindow("Выбор жанров", items, selectedIds);
            if (dialog.ShowDialog() == true)
            {
                SelectedGenres = dialog.SelectedItems
                    .Select(i => AllGenres.First(g => g.Id == i.Id))
                    .ToList();
            }
        }

        private bool IsValidIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return true;

            string cleanIsbn = isbn.Replace("-", "").Replace(" ", "");

            // ISBN-10
            if (cleanIsbn.Length == 10)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(cleanIsbn, @"^\d{9}[\dX]$"))
                {
                    int sum = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (!char.IsDigit(cleanIsbn[i]))
                            return false;
                        sum += (i + 1) * int.Parse(cleanIsbn[i].ToString());
                    }

                    char lastChar = cleanIsbn[9];
                    int last = lastChar == 'X' ? 10 : (char.IsDigit(lastChar) ? int.Parse(lastChar.ToString()) : -1);

                    if (last == -1) return false;
                    sum += 10 * last;

                    return sum % 11 == 0;
                }
                return false;
            }

            // ISBN-13
            if (cleanIsbn.Length == 13)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(cleanIsbn, @"^\d{13}$"))
                {
                    int sum = 0;
                    for (int i = 0; i < 12; i++)
                    {
                        int digit = int.Parse(cleanIsbn[i].ToString());
                        sum += (i % 2 == 0) ? digit : digit * 3;
                    }

                    int checksum = (10 - (sum % 10)) % 10;
                    int lastDigit = int.Parse(cleanIsbn[12].ToString());

                    return checksum == lastDigit;
                }
                return false;
            }

            return false;
        }

        private void UpdateIsbnValidation()
        {
            if (string.IsNullOrWhiteSpace(CurrentBook?.ISBN))
            {
                IsbnValidationMessage = "ISBN не обязателен";
            }
            else
            {
                var isValid = IsValidIsbn(CurrentBook.ISBN);
                IsbnValidationMessage = isValid ? "ISBN корректен" : "Неверный формат ISBN (10 или 13 цифр)";
            }

            OnPropertyChanged(nameof(IsbnValidationColor));
            OnPropertyChanged(nameof(IsbnValidationIcon));
            OnPropertyChanged(nameof(IsbnValidationMessageColor));
            OnPropertyChanged(nameof(CanSave));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(CurrentBook.Title))
            {
                MessageBox.Show("Введите название книги", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedAuthors == null || !SelectedAuthors.Any())
            {
                MessageBox.Show("Выберите хотя бы одного автора", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedGenres == null || !SelectedGenres.Any())
            {
                MessageBox.Show("Выберите хотя бы один жанр", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentBook.PublishYear < 1000 || CurrentBook.PublishYear > 2100)
            {
                MessageBox.Show("Введите корректный год издания (1000-2100)", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CurrentBook.QuantityInStock < 0 || CurrentBook.QuantityInStock > 1000)
            {
                MessageBox.Show("Количество должно быть от 0 до 1000", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(CurrentBook.ISBN) && !IsValidIsbn(CurrentBook.ISBN))
            {
                MessageBox.Show("Неверный формат ISBN. Используйте ISBN-10 (10 цифр) или ISBN-13 (13 цифр)",
                    "Ошибка валидации ISBN", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}