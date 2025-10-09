using System.Collections.ObjectModel;
using System.Windows;

namespace OOP_LAB1
{
    public partial class TaskEditWindow : Window
    {
        public TaskItem Task { get; private set; }
        public ObservableCollection<string> Categories { get; set; }

        public TaskEditWindow(TaskItem task, ObservableCollection<string> categories)
        {
            InitializeComponent();
            Task = task;
            Categories = categories;

            TitleTextBox.Text = Task.Title;
            DescriptionTextBox.Text = Task.Description;
            CategoryComboBox.ItemsSource = Categories;
            CategoryComboBox.SelectedItem = Task.Category ?? Categories[0]; // Установка категории

            HighPriorityCheckBox.IsChecked = Task.Priority == "Высокий";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка");
                return;
            }
            Task.Description = DescriptionTextBox.Text;
            Task.Title = TitleTextBox.Text;
            Task.Category = CategoryComboBox.SelectedItem?.ToString() ?? Categories[0];
            Task.Priority = HighPriorityCheckBox.IsChecked == true ? "Высокий" : "Средний";

            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
