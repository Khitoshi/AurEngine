using Editor.GameProject;
using Editor.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnMainWindowLoaded;
            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnMainWindowLoaded;
            OpenProjectBrowserDialog();
        }

        private void OpenProjectBrowserDialog()
        {
            var projectBrowser = new ProjectBrowserDialog();
            if (projectBrowser.ShowDialog() == false)
            {
                Application.Current.Shutdown();
            }
            else
            {

            }

        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            newsButton.IsChecked = false;
            libraryButton.IsChecked = false;
            MainFrame.Navigate(new Home());
        }

        private void NewsButton_Click(object sender, RoutedEventArgs e)
        {
            homeButton.IsChecked = false;
            libraryButton.IsChecked = false;
            MainFrame.Navigate(new News());
        }

        private void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            homeButton.IsChecked = false;
            newsButton.IsChecked = false;
            MainFrame.Navigate(new Library());
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateButtonSizes();

            if (MainFrame.Content is Library libraryPage)
            {
                libraryPage.SetParentWindowSize(this.ActualWidth, this.ActualHeight);
            }
        }

        private void UpdateButtonSizes()
        {
            double width = this.ActualWidth / 5;
            double height = this.ActualHeight / 9;

            homeButton.Width = width;
            homeButton.Height = height;

            libraryButton.Width = width;
            libraryButton.Height = height;

            newsButton.Width = width;
            newsButton.Height = height;

        }



    }
}