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
        public required string ProjectName { get; set; }

        [DataMember]
        public required string ProjectPath { get; set; }

        [DataMember]
        public DateTime LastOpened { get; set; }

        public string FullPath { get => $"{ProjectPath}{ProjectName}{Project.Extension}"; }

        public byte[] Icon { get; set; } = [];
        public byte[] Screenshot { get; set; } = [];
    }

    [DataContract]
    public class ProjectDataList
    {
        [DataMember]
        public List<ProjectData> Projects { get; set; } = [];
    }


    class OpenProject
    {
        private static readonly string _applicationDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Editor\\";
        private static readonly string _projectDataPath;
        private static readonly ObservableCollection<ProjectData> _projects = [];
        public static ReadOnlyObservableCollection<ProjectData> Projects { get; }

        static OpenProject()
        {
            try
            {
                if (!Directory.Exists(_applicationDataPath)) Directory.CreateDirectory(_applicationDataPath);
                _projectDataPath = $"{_applicationDataPath}ProjectData.xml";
                Projects = new ReadOnlyObservableCollection<ProjectData>(_projects);
                ReadProjectData();
            }
            catch (Exception ex)
            {
                //TODO: log error
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private static ReadOnlyObservableCollection<ProjectData> ReadProjectData()
        {
            if (!File.Exists(_projectDataPath)) return ReadOnlyObservableCollection<ProjectData>.Empty;

            var projects = Serializer.FromFile<ProjectDataList>(_projectDataPath).Projects.OrderByDescending(p => p.LastOpened);
            _projects.Clear();

            // Load project data
            foreach (var project in projects)
            {
                if (!File.Exists(project.FullPath)) continue;

                project.Icon = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Icon.png");
                project.Screenshot = File.ReadAllBytes($@"{project.ProjectPath}\.Primal\Screenshot.png");

                _projects.Add(project);
            }
            return Projects;
        }

        public static Project Open(ProjectData data)
        {
            ReadProjectData();

            var project = _projects.FirstOrDefault(p => p.FullPath == data.FullPath);
            if (project == null)
            {
                project = data;
                project.LastOpened = DateTime.Now;
                _projects.Add(project);
            }
            else
            {
                project.LastOpened = DateTime.Now;
            }

            WriteProjectData();

            return Project.Load(project.FullPath);
        }

        private static void WriteProjectData()
        {
            var projects = _projects.OrderBy(p => p.LastOpened).ToList();
            Serializer.ToFile(new ProjectDataList() { Projects = projects }, _projectDataPath);
        }
    }
}
