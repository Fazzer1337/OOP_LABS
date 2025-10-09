using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OOP_LAB1
{
    public partial class CategoryDeleteWindow : Window
    {
        public string SelectedCategory { get; private set; }
        public CategoryDeleteWindow(ObservableCollection<string> categories)
        {
            InitializeComponent();
            CategoryComboBox.ItemsSource = categories;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SelectedCategory = CategoryComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(SelectedCategory))
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}