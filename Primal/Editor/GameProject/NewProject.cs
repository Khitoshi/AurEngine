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
        public required string ProjectType { get; set; }
        [DataMember]
        public required string ProjectFile { get; set; }
        [DataMember]
        public required List<String> Folders { get; set; }

        public required Byte[] Icon { get; set; }
        public required string IconFilePath { get; set; }
        public required Byte[] Screenshot { get; set; }
        public required string ScreenshotFilePath { get; set; }
        public required string ProjectFilePath { get; set; }
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
                    SetupValidation();
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
                    SetupValidation();
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
                    OnPropertyChanged(nameof(IsVisible));
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

        private string _errorMessage = string.Empty;
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
                InitializeTemplates();
                SetupValidation();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create {ProjectName}");
                throw;
            }
        }

        private void InitializeTemplates()
        {
            var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
            Debug.Assert(templateFiles.Any());

            foreach (var file in templateFiles)
            {
                var template = LoadTemplate(file);
                if (template != null) _projectTemplates.Add(template);
            }
        }

        private ProjectTemplate LoadTemplate(string file)
        {
            if (String.IsNullOrEmpty(file)) return null;

            var filePath = Path.GetDirectoryName(file);
            if (String.IsNullOrEmpty(filePath)) return null;

            //get the icon ,screenshot and project file
            var template = Serializer.FromFile<ProjectTemplate>(file);
            template.IconFilePath = Path.GetFullPath(Path.Combine(filePath, "Icon.png"));
            template.Icon = File.ReadAllBytes(template.IconFilePath);
            template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(filePath, "Screenshot.png"));
            template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
            template.ProjectFilePath = Path.GetFullPath(Path.Combine(filePath, template.ProjectFile));

            return template;
        }

        private void SetupValidation()
        {
            ErrorMessage = string.Empty;
            ValidateProjectPath();
            ValidateProjectName();
        }

        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";

            IsPathValid = true;
            if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMessage = "Select a valid project folder.";
                IsPathValid = false;
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMessage = "Invalid character(s) used in project path.";
                IsPathValid = false;
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMessage = "Selected project folder already exists and is not empty.";
                IsPathValid = false;
            }
            return IsPathValid;
        }

        private bool ValidateProjectName()
        {
            var nameRegex = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

            IsNameValid = true;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMessage = "Type in a project name.";
                IsNameValid = false;
            }
            else if (!nameRegex.IsMatch(ProjectName))
            {
                ErrorMessage = "Invalid character(s) used in project name.";
                IsNameValid = false;
            }

            return IsNameValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            SetupValidation();
            if (!IsVisible) return string.Empty;

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
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to Create to {ProjectName}");
                throw;
            }
        }

    }
}
