public partial class AddCategoryWindow : Window
{
    public string CategoryName => CategoryNameBox.Text.Trim();

    public AddCategoryWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            MessageBox.Show("Введите название категории!");
            return;
        }
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
