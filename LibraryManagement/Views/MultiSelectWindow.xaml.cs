using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibraryManagement.Views
{
    // Класс для элементов множественного выбора
    public class MultiSelectItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        // ИСПРАВЛЕНО: переопределяем ToString для отображения в ListBox
        public override string ToString()
        {
            return DisplayName ?? "Без названия";
        }
    }

    public partial class MultiSelectWindow : Window, INotifyPropertyChanged
    {
        private List<MultiSelectItem> _allItems;
        private List<MultiSelectItem> _filteredItems;
        private string _searchText;
        private List<MultiSelectItem> _selectedItems;

        public List<MultiSelectItem> AllItems
        {
            get => _allItems;
            set
            {
                _allItems = value;
                OnPropertyChanged();
                FilterItems();
            }
        }

        public List<MultiSelectItem> FilteredItems
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
            }
        }

        public List<MultiSelectItem> SelectedItems
        {
            get => _selectedItems ?? (_selectedItems = new List<MultiSelectItem>());
            private set
            {
                _selectedItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedCount));
            }
        }

        public string DialogTitle { get; set; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterItems();
            }
        }

        public string SelectedCount => _selectedItems?.Count.ToString() ?? "0";

        public MultiSelectWindow(string title, List<MultiSelectItem> allItems, List<int> selectedIds = null)
        {
            InitializeComponent();

            DialogTitle = title;
            AllItems = allItems ?? new List<MultiSelectItem>();

            _selectedItems = new List<MultiSelectItem>();

            if (selectedIds != null && selectedIds.Any())
            {
                _selectedItems = AllItems.Where(item => selectedIds.Contains(item.Id)).ToList();
            }

            FilteredItems = new List<MultiSelectItem>(AllItems);
            DataContext = this;

            Loaded += (s, e) =>
            {
                RestoreSelection();
            };
        }

        private void RestoreSelection()
        {
            if (ItemsListBox == null || _selectedItems == null) return;

            ItemsListBox.SelectedItems.Clear();
            foreach (var item in _selectedItems)
            {
                var index = FilteredItems.IndexOf(item);
                if (index >= 0)
                {
                    ItemsListBox.SelectedItems.Add(FilteredItems[index]);
                }
            }
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void FilterItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems = new List<MultiSelectItem>(AllItems);
            }
            else
            {
                var searchLower = SearchText.ToLower();
                FilteredItems = AllItems
                    .Where(item => item.DisplayName != null && item.DisplayName.ToLower().Contains(searchLower))
                    .ToList();
            }

            // ИСПРАВЛЕНО: принудительно обновляем отображение
            ItemsListBox.ItemsSource = null;
            ItemsListBox.ItemsSource = FilteredItems;
            ItemsListBox.DisplayMemberPath = "DisplayName"; // Явно указываем путь к отображаемому свойству

            RestoreSelection();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = ((TextBox)sender).Text;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = new List<MultiSelectItem>();
            foreach (MultiSelectItem item in ItemsListBox.SelectedItems)
            {
                selected.Add(item);
            }
            SelectedItems = selected;

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
}