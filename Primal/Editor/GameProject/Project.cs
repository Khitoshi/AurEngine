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
using System.Windows;

namespace Editor.GameProject
{
    [DataContract(Name = "Game")]
    class Project : ViewModelBase
    {
        public static string Extension { get; } = ".primal";

        [DataMember]
        public String Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }

        public string FullPath => $"{Path} {Name}{Extension}";

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public static Project Current => Application.Current.MainWindow.DataContext as Project;

        public Project(string name, string path)
        {
            Name = name;
            Path = path;
            OnDeserialized(new StreamingContext());
        }

        /// <summary>
        /// Load a project from a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns>
        /// The loaded project
        /// </returns>
        public static Project Load(string path)
        {
            Debug.Assert(File.Exists(path));
            return Serializer.FromFile<Project>(path);
        }

        public void Unload()
        {
        }

        /// <summary>
        /// Save a project to a file
        /// </summary>
        /// <param name="project"></param>
        public void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
        }

        /// <summary>
        /// Method to be executed after deserialization is complete
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_scenes == null) return;

            // Convert a list of deserialized scenes into a RedOnlyObservableCollection
            Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
            OnPropertyChanged(nameof(_scenes));

            // Set the active scene to the first scene that is active
            Scenes.FirstOrDefault(p => p.IsActive);
        }


    }
}
