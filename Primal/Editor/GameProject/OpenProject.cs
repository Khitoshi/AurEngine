using Editor.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor.GameProject
{
    [DataContract]
    public class ProjectData
    {
        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string ProjectPath { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }

        public string FullPath { get => $"{ProjectPath}{ProjectName}{Project.Extension}"; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects { get; set; }
    }


    class OpenProject
    {
        private static readonly string _applicationDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Editor\\";
        private static readonly string _projectDataPath;
        private static readonly ObservableCollection<ProjectData> _projects = new ObservableCollection<ProjectData>();
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

        static OpenProject()
        {
            try
            {
                // Create application data directory if it does not exist
                if (!Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_applicationDataPath);
                // Set the project data path
                _projectDataPath = $"{_applicationDataPath}ProjectData.xml";
                // Initialize ReadOnlyObservableCollection for reading project data
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                // Read project data
                ReadProjectData();
            }
            catch (Exception ex)
            {
                //TODO: log error
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Reads project data from files and sorts them in the order of last opened
        /// </summary>
        /// <returns>
        /// Returns null if project data file does not exist
        /// </returns>
        private static ReadOnlyObservableCollection<ProjectData> ReadProjectData()
        {
            if (!File.Exists(_projectDataPath)) return null;

            // Reads project data from files and sorts them in the order of last opened
            var projects = Serializer.FromFile<ProjectDataList>(_projectDataPath).Projects.OrderByDescending(p => p.LastOpened);
            _projects.Clear();

            // Load project data
            foreach (var project in projects)
            {
                // Remove project from list if project file does not exist
                if (!File.Exists(project.FullPath)) continue;

                // Load project icons and screenshots
                project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");

                _projects.Add(project);
            }
            return Projects;
        }


        /// <summary>
        /// Opens the specified project
        /// </summary>
        /// <param name="data"> specifies the project to open </param>
        /// <returns>
        /// Loads and returns the specified project
        /// </returns>
        public static Project Open(ProjectData data)
        {
            // Load existing project data
            ReadProjectData();

            // Search for projects matching the specified path
            var project = _projects.FirstOrDefault(p => p.FullPath == data.FullPath);

            if (project == null)
            {// If project not found, add as new project
                project = data;
                project.LastOpened = DateTime.Now;
                _projects.Add(project);
            }
            else
            {// If an existing project is found, update the date and time it was last opened
                project.LastOpened = DateTime.Now;
            }

            // Write changes to the project data file
            WriteProjectData();

            // Load and return projects
            return Project.Load(project.FullPath);
        }

        /// <summary>
        /// Writes project data to file
        /// </summary>
        private static void WriteProjectData()
        {
            // Sort projects in the order of last opened
            var projects = _projects.OrderBy(p => p.LastOpened).ToList();
            // Write project data to file
            Serializer.ToFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
        }

    }
}
