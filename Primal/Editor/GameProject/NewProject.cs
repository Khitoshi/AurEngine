using Editor.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Editor.GameProject
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string ProjectFile { get; set; }
        [DataMember]
        public List<String> Folders { get; set; }

        public Byte[] Icon { get; set; }
        public string IconFilePath { get; set; }
        public Byte[] Screenshot { get; set; }
        public string ScreenshotFilePath { get; set; }
        public string ProjectFilePath { get; set; }
    }

    internal class NewProject : ViewModelBase
    {
        //TODO: get the path from the installation location
        private readonly string _templatePath = @"..\..\ProjectTemplates\";

        private string _projectName = "NewProject";
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    //TODO This code ignores the DRY principle.
                    ErrorMessage = string.Empty;
                    ValidateProjectPath();
                    ValidateProjectName();
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\AurEngine\PrimamlProject\";
        public string ProjectPath
        {
            get { return _projectPath; }
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    //TODO This code ignores the DRY principle.
                    ErrorMessage = string.Empty;
                    ValidateProjectPath();
                    ValidateProjectName();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = new ObservableCollection<ProjectTemplate>();
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }

        // validation path
        private bool _isPathValid;
        public bool IsPathValid
        {
            get { return _isPathValid; }
            set
            {
                if (_isPathValid != value)
                {
                    _isPathValid = value;
                    OnPropertyChanged(nameof(IsPathValid));
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        private bool _isNameValid;
        public bool IsNameValid
        {
            get { return _isNameValid; }
            set
            {
                if (_isNameValid != value)
                {
                    _isNameValid = value;
                    OnPropertyChanged(nameof(_isNameValid));
                    OnPropertyChanged(nameof(IsVisible)); // IsVisibleの更新を通知
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                return IsPathValid && IsNameValid;
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }



        public NewProject()
        {

            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);

            try
            {
                //project templates are stored in a folder structure
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Any());
                foreach (var file in templateFiles)
                {
                    //deserialize the template
                    var template = Serializer.FromFile<ProjectTemplate>(file);

                    //get the folder path of the template
                    var filePath = Path.GetDirectoryName(file);
                    if (filePath == String.Empty || String.IsNullOrEmpty(filePath)) continue;

                    //get the icon and screenshot
                    template.IconFilePath = Path.GetFullPath(Path.Combine(filePath, "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(filePath, "Screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);

                    //get the project file
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(filePath, template.ProjectFile));

                    _projectTemplates.Add(template);
                }
                //TODO This code ignores the DRY principle.
                ErrorMessage = string.Empty;
                ValidateProjectPath();
                ValidateProjectName();
            }
            catch (Exception ex)
            {
                //TODO: log error
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// check if the project path is valid
        /// </summary>
        /// <returns>
        ///  success: true
        ///  failure: false
        /// </returns>
        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

            IsPathValid = false;
            if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {//null or whitespace exists
                ErrorMessage = "Select a valid project folder.";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {//invalid characters in the path
                ErrorMessage = "Invalid character(s) used in project path.";
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {//project folder exists and is not empty
                ErrorMessage = "Selected project folder already exists and is not empty.";
            }
            else if (!nameRegex.IsMatch(ProjectPath))
            {//invalid characters in the path
                ErrorMessage = "Invalid character(s) used in project path.";
            }
            else
            {//success
                IsPathValid = true;
            }
            return IsPathValid;
        }

        /// <summary>
        /// check if the project name is valid
        /// </summary>
        /// <returns>
        ///  success: true
        ///  failure: false
        /// </returns>
        private bool ValidateProjectName()
        {
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

            IsNameValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {// null or whitespace exists
                ErrorMessage = "Type in a project name.";
            }
            else if (!nameRegex.IsMatch(ProjectName))
            {//invalid characters in the name
                ErrorMessage = "Invalid character(s) used in project name.";
            }
            else
            {//success
                IsNameValid = true;
            }
            return IsNameValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            //check if the project path and name is valid
            //TODO This code ignores the DRY principle.
            ErrorMessage = string.Empty;
            ValidateProjectPath();
            ValidateProjectName();
            if (!IsPathValid || !IsNameValid) return string.Empty;

            //create the project folder structure
            if (!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";
            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                //create the project folder structure
                if (Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach (var folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }

                //copy the icon, screenshot and project file
                var dirInfo = new DirectoryInfo(path + @".Primal\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                File.Copy(template.IconFilePath, Path.GetFullPath((Path.Combine(Path.GetDirectoryName(dirInfo.FullName), "Icon.png"))));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath((Path.Combine(Path.GetDirectoryName(dirInfo.FullName), "Screenshot.png"))));
                var projectXml = File.ReadAllText(template.ProjectFilePath);
                projectXml = string.Format(projectXml, ProjectName, ProjectPath);
                var projectPath = Path.GetFullPath(Path.Combine(Path.Combine(path, $"{ProjectName}{Project.Extension}")));
                File.WriteAllText(projectPath, projectXml);
                return path;
            }
            catch (Exception ex)
            {
                //TODO log error
                return string.Empty;
            }
        }

    }
}
