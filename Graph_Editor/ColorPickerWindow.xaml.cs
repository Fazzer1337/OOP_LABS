using System.Windows;
using System.Windows.Media;

namespace GraphEditor
{
    public partial class ColorPickerWindow : Window
    {
        public Color SelectedColor { get; private set; }

        public ColorPickerWindow()
        {
            InitializeComponent();
            // Инициализируем HEX-поле при выборе цвета
            ColorPickerControl.SelectedColorChanged += (s, e) =>
            {
                if (ColorPickerControl.SelectedColor != null)
                    HexTextBox.Text = $"#{ColorPickerControl.SelectedColor.Value.R:X2}{ColorPickerControl.SelectedColor.Value.G:X2}{ColorPickerControl.SelectedColor.Value.B:X2}";
            };
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ColorPickerControl.SelectedColor != null)
            {
                SelectedColor = ColorPickerControl.SelectedColor.Value;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите цвет.");
            }
        }

        // Ручной ввод HEX
        private void HexButton_Click(object sender, RoutedEventArgs e)
        {
            string hex = HexTextBox.Text.Trim();
            if (!hex.StartsWith("#")) hex = "#" + hex;
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                ColorPickerControl.SelectedColor = color;
            }
            catch
            {
                MessageBox.Show("Некорректный HEX цвет.");
            }
        }
    }
}
