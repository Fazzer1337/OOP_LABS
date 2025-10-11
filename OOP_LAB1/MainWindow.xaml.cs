using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OOP_LAB1
{
    public partial class MainWindow : Window
    {
        private const string DataFileName = "tasks.json";
        private const string CategoriesFileName = "categories.json";

        public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();

        private ICollectionView TasksView;

        public MainWindow()
        {
            InitializeComponent();

            // Загрузка категорий ДО назначения ItemsSource
            LoadCategories();
            if (Categories.Count == 0)
            {
                Categories.Add("Все");
                Categories.Add("Работа");
                Categories.Add("Учёба");
                Categories.Add("Дом");
                SaveCategories();
            }

            TasksList.ItemsSource = Tasks;
            CategoriesList.ItemsSource = Categories;
            CategoriesList.SelectedIndex = 0;

            TasksView = CollectionViewSource.GetDefaultView(Tasks);
            TasksView.Filter = TasksFilter;

            SearchBox.TextChanged += SearchBox_TextChanged;
            CategoriesList.SelectionChanged += CategoriesList_SelectionChanged;

            LoadTasks();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("Введите название новой категории:", "Новая категория", "");
            if (!string.IsNullOrWhiteSpace(input) && !Categories.Contains(input))
            {
                Categories.Add(input);
                SaveCategories();
                CategoriesList.SelectedItem = input;
                TasksView.Refresh();
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CategoryDeleteWindow(Categories) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                string category = dlg.SelectedCategory;
                if (!string.IsNullOrEmpty(category) && Categories.Contains(category))
                {
                    Categories.Remove(category);
                    SaveCategories();
                    if (Categories.Count > 0)
                    {
                        CategoriesList.SelectedItem = Categories[0];
                    }
                    TasksView.Refresh();
                }
            }
        }

        private bool TasksFilter(object item)
        {
            if (item is TaskItem task)
            {
                bool matchesCategory = CategoriesList.SelectedItem == null
                    || CategoriesList.SelectedItem.ToString() == "Все"
                    || task.Category == CategoriesList.SelectedItem.ToString();

                bool matchesSearch = string.IsNullOrWhiteSpace(SearchBox.Text)
                    || (task.Title != null && task.Title.ToLower().Contains(SearchBox.Text.ToLower()))
                    || (task.Description != null && task.Description.ToLower().Contains(SearchBox.Text.ToLower()));

                return matchesCategory && matchesSearch;
            }
            return false;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TasksView.Refresh();
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TasksView.Refresh();
        }

        private void CategoriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TasksView.Refresh();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var newTask = new TaskItem()
            {
                Title = "",
                Description = "",
                Category = Categories.Count > 0 ? Categories[0] : "Работа",
                Priority = "Средний",
                DueTime = DateTime.MinValue, 
                IsCompleted = false,
            };

            var editWindow = new TaskEditWindow(newTask, Categories) { Owner = this };
            if (editWindow.ShowDialog() == true)
            {
                Tasks.Add(newTask); 
                SaveTasks();
                TasksView.Refresh();
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                Tasks.Remove(task);
                SaveTasks();
                TasksView.Refresh();
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                var copy = new TaskItem
                {
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category,
                    Priority = task.Priority,
                    DueTime = task.DueTime,
                    IsCompleted = task.IsCompleted
                };

                var editWindow = new TaskEditWindow(copy, Categories) { Owner = this };
                if (editWindow.ShowDialog() == true)
                {
                    task.Title = copy.Title;
                    task.Description = copy.Description;
                    task.Category = copy.Category;
                    task.Priority = copy.Priority;
                    task.DueTime = copy.DueTime;
                    task.IsCompleted = copy.IsCompleted;

                    SaveTasks();
                    TasksView.Refresh();
                }
            }
        }

        private void SaveTasks()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Tasks, jsonOptions);
                File.WriteAllText(DataFileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }

        private void TaskCompleted_Changed(object sender, RoutedEventArgs e)
        {
            SaveTasks();
        }

        private void SaveCategories()
        {
            try
            {
                var json = JsonSerializer.Serialize(Categories);
                File.WriteAllText(CategoriesFileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения категорий: " + ex.Message);
            }
        }

        private void LoadCategories()
        {
            try
            {
                if (File.Exists(CategoriesFileName))
                {
                    var loaded = JsonSerializer.Deserialize<ObservableCollection<string>>(File.ReadAllText(CategoriesFileName));
                    if (loaded != null)
                    {
                        Categories.Clear();
                        foreach (var cat in loaded)
                        {
                            Categories.Add(cat);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message);
            }
        }

        private void LoadTasks()
        {
            try
            {
                if (File.Exists(DataFileName))
                {
                    string json = File.ReadAllText(DataFileName);
                    var loadedTasks = JsonSerializer.Deserialize<ObservableCollection<TaskItem>>(json);
                    if (loadedTasks != null)
                    {
                        Tasks.Clear();
                        foreach (var t in loadedTasks)
                            Tasks.Add(t);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
