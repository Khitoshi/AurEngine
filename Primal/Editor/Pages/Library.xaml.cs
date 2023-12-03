using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Editor.Pages
{
    /// <summary>
    /// Library.xaml の相互作用ロジック
    /// </summary>
    public partial class Library : Page
    {
        public Library()
        {
            InitializeComponent();
        }

        public void SetParentWindowSize(double width, double height)
        {
            double buttonWidth = width / 5;
            double buttonHeight = height / 9;

            allButton.Width = buttonWidth;
            allButton.Height = buttonHeight;

            favoritesButton.Width = buttonWidth;
            favoritesButton.Height = buttonHeight;

            addTabButton.Width = buttonWidth;
            addTabButton.Height = buttonHeight;

            createProjectButton.Width = buttonWidth;
            createProjectButton.Height = buttonHeight;

            existingProjectButton.Width = buttonWidth;
            existingProjectButton.Height = buttonHeight;
        }

        private void AllButton_Click(object sender, RoutedEventArgs e)
        {
            favoritesButton.IsChecked = false;
            addTabButton.IsChecked = false;
            createProjectButton.IsChecked = false;
            existingProjectButton.IsChecked = false;

            //LibraryFrame.Navigate(new NewProject());
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            allButton.IsChecked = false;
            favoritesButton.IsChecked = false;
            addTabButton.IsChecked = false;
            existingProjectButton.IsChecked = false;

            //LibraryFrame.Navigate(new NewProject());
        }

        private void ExistingProjectButton_Click(object sender, RoutedEventArgs e)
        {
            allButton.IsChecked = false;
            favoritesButton.IsChecked = false;
            addTabButton.IsChecked = false;
            createProjectButton.IsChecked = false;

            //LibraryFrame.Navigate(new ExistingProject());
        }
    }
}
