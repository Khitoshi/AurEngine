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

namespace Editor.GameProject
{
    /// <summary>
    /// OpenProject.xaml の相互作用ロジック
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();
        }

        private void OnOpen_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedProject();
        }
        private void OnListBoxItem_Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }

        /// <summary>
        /// Open the selected project
        /// </summary>
        private void OpenSelectedProject()
        {
            //Get OpenProject view model from DataContext
            var project = OpenProject.Open(projectListBox.SelectedItem as ProjectData);

            //Get the window that contains this view
            var win = Window.GetWindow(this);

            bool dialogResult = false;
            if (project != null)
            {//Open the project and set the window's DataContext to the project
                dialogResult = true;
                win.DataContext = project;
            }

            //Close the window
            win.DialogResult = dialogResult;
            win.Close();
        }

    }
}
