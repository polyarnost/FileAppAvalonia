using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileApp.ViewModels;

namespace FileApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new FileViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}