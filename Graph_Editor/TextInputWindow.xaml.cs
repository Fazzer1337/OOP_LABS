using System.Windows;

namespace GraphEditor
{
    public partial class TextInputWindow : Window
    {
        public string InputText { get; private set; } = "";

        public TextInputWindow()
        {
            InitializeComponent();
            TextInputBox.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            InputText = TextInputBox.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
