using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using LibraryManagement.Views;
using System.Collections.Generic;
using System.Windows;

namespace LibraryManagement.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly LibraryContext _context;
        private ObservableCollection<Book> _books;
        private ObservableCollection<Author> _authors;
        private ObservableCollection<Genre> _genres;
        private ObservableCollection<Genre> _uniqueGenres;
        private Book _selectedBook;
        private string _searchText;
        private Author _selectedAuthor;
        private Genre _selectedGenre;
        private int _totalBooksCount;

        // Свойства для фильтрации (с элементом "Все")
        private ObservableCollection<Author> _authorsForFilter;
        public ObservableCollection<Author> AuthorsForFilter
        {
            get => _authorsForFilter;
            set { _authorsForFilter = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Genre> _genresForFilter;
        public ObservableCollection<Genre> GenresForFilter
        {
            get => _genresForFilter;
            set { _genresForFilter = value; OnPropertyChanged(); }
        }

        // Счетчики для статистики
        public int AuthorsCount => _authors?.Where(a => a.Id != 0).Count() ?? 0;
        public int GenresCount => _genres?.Where(g => g.Id != 0).Count() ?? 0;

        public MainViewModel()
        {
            _context = new LibraryContext();
            _context.Database.EnsureCreated();

            LoadData();
            InitializeCommands();

            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => OnPropertyChanged(nameof(CurrentDate));
            timer.Start();
        }

        public string CurrentDate => DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        public ObservableCollection<Book> Books
        {
            get => _books;
            set
            {
                _books = value;
                OnPropertyChanged();
                TotalBooksCount = _books?.Count ?? 0;
            }
        }

        public ObservableCollection<Author> Authors
        {
            get => _authors;
            set { _authors = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Genre> Genres
        {
            get => _genres;
            set { _genres = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Genre> UniqueGenres
        {
            get => _uniqueGenres;
            set { _uniqueGenres = value; OnPropertyChanged(); }
        }

        public Book SelectedBook
        {
            get => _selectedBook;
            set { _selectedBook = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        public Author SelectedAuthor
        {
            get => _selectedAuthor;
            set
            {
                _selectedAuthor = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        public Genre SelectedGenre
        {
            get => _selectedGenre;
            set
            {
                _selectedGenre = value;
                OnPropertyChanged();
                FilterBooks();
            }
        }

        public int TotalBooksCount
        {
            get => _totalBooksCount;
            set { _totalBooksCount = value; OnPropertyChanged(); }
        }

        public ICommand AddBookCommand { get; private set; }
        public ICommand EditBookCommand { get; private set; }
        public ICommand DeleteBookCommand { get; private set; }
        public ICommand ManageAuthorsCommand { get; private set; }
        public ICommand ManageGenresCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        private void InitializeCommands()
        {
            AddBookCommand = new RelayCommand(param => OpenBookWindow(null));
            EditBookCommand = new RelayCommand(param => OpenBookWindow(SelectedBook), param => SelectedBook != null);
            DeleteBookCommand = new RelayCommand(param => DeleteBook(), param => SelectedBook != null);
            ManageAuthorsCommand = new RelayCommand(param => OpenAuthorsWindow());
            ManageGenresCommand = new RelayCommand(param => OpenGenresWindow());
            RefreshCommand = new RelayCommand(param => RefreshData());
        }

        private void LoadData()
        {
            try
            {
                // Загружаем книги со всеми связями
                _context.Books
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .Load();

                // Загружаем авторов
                _context.Authors
                    .Include(a => a.BookAuthors)
                    .Load();

                // Загружаем жанры
                _context.Genres
                    .Include(g => g.BookGenres)
                    .Load();

                Books = _context.Books.Local.ToObservableCollection();

                // Создаем список авторов для фильтрации (с элементом "Все")
                var authorsList = _context.Authors.Local.ToList();
                authorsList.Insert(0, new Author { Id = 0, FirstName = "Все", LastName = "" });
                AuthorsForFilter = new ObservableCollection<Author>(authorsList);

                // Для обратной совместимости
                Authors = new ObservableCollection<Author>(authorsList);

                // Создаем список жанров для фильтрации (с элементом "Все жанры")
                var genresList = _context.Genres.Local
                    .OrderBy(g => g.Name)
                    .ToList();

                genresList.Insert(0, new Genre { Id = 0, Name = "Все жанры" });
                GenresForFilter = new ObservableCollection<Genre>(genresList);

                // Для уникальных жанров
                UniqueGenres = new ObservableCollection<Genre>(genresList);
                Genres = new ObservableCollection<Genre>(genresList);

                // Обновляем счетчики
                OnPropertyChanged(nameof(AuthorsCount));
                OnPropertyChanged(nameof(GenresCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterBooks()
        {
            try
            {
                var query = _context.Books
                    .Include(b => b.BookAuthors)
                        .ThenInclude(ba => ba.Author)
                    .Include(b => b.BookGenres)
                        .ThenInclude(bg => bg.Genre)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    query = query.Where(b => b.Title.Contains(SearchText));
                }

                if (SelectedAuthor != null && SelectedAuthor.Id != 0)
                {
                    query = query.Where(b => b.BookAuthors.Any(ba => ba.AuthorId == SelectedAuthor.Id));
                }

                if (SelectedGenre != null && SelectedGenre.Id != 0)
                {
                    query = query.Where(b => b.BookGenres.Any(bg => bg.GenreId == SelectedGenre.Id));
                }

                query = query.OrderBy(b => b.Title);

                Books = new ObservableCollection<Book>(query.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenBookWindow(Book book)
        {
            try
            {
                var allAuthors = Authors.Where(a => a.Id != 0).ToList();
                var allGenres = Genres.Where(g => g.Id != 0).ToList();

                // ИСПРАВЛЕНО: конструктор с 3 параметрами
                var window = new BookWindow(book, allAuthors, allGenres);

                if (window.ShowDialog() == true)
                {
                    if (book == null)
                    {
                        _context.Books.Add(window.CurrentBook);
                        _context.SaveChanges();

                        foreach (var author in window.SelectedAuthors)
                        {
                            _context.BookAuthors.Add(new BookAuthor
                            {
                                BookId = window.CurrentBook.Id,
                                AuthorId = author.Id
                            });
                        }

                        foreach (var genre in window.SelectedGenres)
                        {
                            _context.BookGenres.Add(new BookGenre
                            {
                                BookId = window.CurrentBook.Id,
                                GenreId = genre.Id
                            });
                        }

                        _context.SaveChanges();
                    }
                    else
                    {
                        _context.Entry(book).CurrentValues.SetValues(window.CurrentBook);

                        var oldAuthors = _context.BookAuthors.Where(ba => ba.BookId == book.Id);
                        _context.BookAuthors.RemoveRange(oldAuthors);

                        var oldGenres = _context.BookGenres.Where(bg => bg.BookId == book.Id);
                        _context.BookGenres.RemoveRange(oldGenres);

                        foreach (var author in window.SelectedAuthors)
                        {
                            _context.BookAuthors.Add(new BookAuthor
                            {
                                BookId = book.Id,
                                AuthorId = author.Id
                            });
                        }

                        foreach (var genre in window.SelectedGenres)
                        {
                            _context.BookGenres.Add(new BookGenre
                            {
                                BookId = book.Id,
                                GenreId = genre.Id
                            });
                        }

                        _context.SaveChanges();
                    }

                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка: {ex.Message}\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += $"Внутренняя ошибка: {ex.InnerException.Message}\n\n";

                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"Детали: {ex.InnerException.InnerException.Message}";
                    }
                }

                System.Windows.MessageBox.Show(errorMessage, "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        private void DeleteBook()
        {
            if (SelectedBook != null)
            {
                var result = MessageBox.Show(
                    $"Удалить книгу '{SelectedBook.Title}'?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Сначала удаляем связи
                        var bookAuthors = _context.BookAuthors.Where(ba => ba.BookId == SelectedBook.Id);
                        _context.BookAuthors.RemoveRange(bookAuthors);

                        var bookGenres = _context.BookGenres.Where(bg => bg.BookId == SelectedBook.Id);
                        _context.BookGenres.RemoveRange(bookGenres);

                        // Затем удаляем саму книгу
                        _context.Books.Remove(SelectedBook);
                        _context.SaveChanges();

                        RefreshData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OpenAuthorsWindow()
        {
            var window = new AuthorsWindow(_context);
            window.ShowDialog();
            RefreshData();
        }

        private void OpenGenresWindow()
        {
            var window = new GenresWindow(_context);
            window.ShowDialog();
            RefreshData();
        }

        private void RefreshData()
        {
            _context.ChangeTracker.Clear();
            LoadData();
            FilterBooks();
        }
    }
}