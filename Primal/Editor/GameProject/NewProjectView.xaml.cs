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
    /// NewProject.xaml の相互作用ロジック
    /// </summary>
    public partial class NewProjectView : UserControl
    {
        public NewProjectView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create a new project using the selected template
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            //Get NewProject view model from DataContext
            var vm = DataContext as NewProject;

            //Create a new project using the selected template and get its path
            var projectPath = vm.CreateProject(templateListBox.SelectedItem as ProjectTemplate);

            //Get the window that contains this view
            var win = Window.GetWindow(this);

            //Set the dialog result to true if the project path is not null or empty
            bool dialogResult = false;
            if (!string.IsNullOrEmpty(projectPath))
            {//Open the project and set the window's DataContext to the project
                dialogResult = true;
                var project = OpenProject.Open(new ProjectData() { ProjectPath = projectPath, ProjectName = vm.ProjectName });
                win.DataContext = project;
            }

            //Close the window
            win.DialogResult = dialogResult;
            win.Close();
        }
    }
}
