using Editor.Components;
using Editor.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Editor.GameProject
{
    [DataContract]
    class Scene : ViewModelBase
    {
        private string _name;
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        [DataMember]
        public Project Project { get; private set; }

        private bool _isActive;
        [DataMember]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        [DataMember(Name = nameof(GameEntities))]
        private ObservableCollection<GameEntity> _gameEntities = [];
        public ReadOnlyObservableCollection<GameEntity> GameEntities { get; private set; }

        public ICommand AddGameEntityCommand { get; private set; }
        public ICommand RemoveGameEntityCommand { get; private set; }


        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
            OnDeserialized(new StreamingContext());
        }



        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_gameEntities != null)
            {
                GameEntities = new ReadOnlyObservableCollection<GameEntity>(_gameEntities);
                OnPropertyChanged(nameof(GameEntities));
            }

            foreach (var entity in _gameEntities)
            {
                entity.IsActive = IsActive;
            }

            AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                AddGameEnity(x);
                var entityIndex = _gameEntities.Count - 1;
                Project.UndoRedo.Add(new UndoRedoAction(
                    () => RemoveGameEnity(x),
                    //() => _gameEntities.Insert(entityIndex, x),
                    () => AddGameEnity(x, entityIndex),
                    $"Add {x.Name} to {Name}"));
            });

            RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                var entityIndex = _gameEntities.IndexOf(x);
                RemoveGameEnity(x);
                Project.UndoRedo.Add(new UndoRedoAction(
                    () => _gameEntities.Insert(entityIndex, x),
                    () => RemoveGameEnity(x),
                    $"Remove {x.Name}"));
            });
        }

        private void AddGameEnity(GameEntity entity, int index = -1)
        {
            Debug.Assert(!_gameEntities.Contains(entity));
            //_gameEntities.Add(entity);
            entity.IsActive = IsActive;
            if (index == -1)
            {
                _gameEntities.Add(entity);
            }
            else
            {
                _gameEntities.Insert(index, entity);
            }
        }

        private void RemoveGameEnity(GameEntity entity)
        {
            Debug.Assert(_gameEntities.Contains(entity));
            entity.IsActive = false;
            _gameEntities.Remove(entity);
        }
    }
}
