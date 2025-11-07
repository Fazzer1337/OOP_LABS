using System.Windows;

namespace GraphEditor
{
    public partial class RotationInputWindow : Window
    {
        public double RotationAngle { get; private set; } = 0;

        public RotationInputWindow()
        {
            InitializeComponent();
            AngleTextBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(AngleTextBox.Text, out double angle))
            {
                RotationAngle = angle;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректное число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
