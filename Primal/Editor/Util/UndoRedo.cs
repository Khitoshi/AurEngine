using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Editor.Util
{
    public interface IUndoRedo
    {
        string Name { get; }
        void Undo();
        void Redo();
    }

    class UndoRedoAction : IUndoRedo
    {
        public string Name { get; }

        private Action _undoAction;
        private Action _redoAction;

        public void Undo() => _undoAction();

        public void Redo() => _redoAction();

        public UndoRedoAction(string name)
        {
            Name = name;
        }

        public UndoRedoAction(Action undo, Action redo, string name) : this(name)
        {
            Debug.Assert(undo != null && redo != null);
            _undoAction = undo;
            _redoAction = redo;
        }

        public UndoRedoAction(string property, object instance, object undoValue, object redoValue, string name) :
            this(
                () => instance.GetType().GetProperty(property).SetValue(instance, undoValue),
                () => instance.GetType().GetProperty(property).SetValue(instance, redoValue),
                name)
        { }
    }


    class UndoRedo
    {
        private bool _isEnableAdd = true;
        private readonly ObservableCollection<IUndoRedo> _undoList = new ObservableCollection<IUndoRedo>();
        public ReadOnlyObservableCollection<IUndoRedo> UndoList { get; }

        private readonly ObservableCollection<IUndoRedo> _redoList = new ObservableCollection<IUndoRedo>();
        public ReadOnlyObservableCollection<IUndoRedo> RedoList { get; }

        public UndoRedo()
        {
            UndoList = new ReadOnlyObservableCollection<IUndoRedo>(_undoList);
            RedoList = new ReadOnlyObservableCollection<IUndoRedo>(_redoList);
        }

        public void Undo()
        {
            if (_undoList.Any())
            {
                var cmd = _undoList.Last();
                _undoList.RemoveAt(_undoList.Count - 1);
                _isEnableAdd = false;
                cmd.Undo();
                _isEnableAdd = true;
                _redoList.Insert(0, cmd);
            }
        }

        public void Redo()
        {
            if (_redoList.Any())
            {
                var cmd = _redoList.First();
                _redoList.RemoveAt(0);
                _isEnableAdd = false;
                cmd.Redo();
                _isEnableAdd = true;
                _undoList.Add(cmd);
            }
        }

        public void Add(IUndoRedo cmd)
        {
            if (!_isEnableAdd) return;
            _undoList.Add(cmd);
            _redoList.Clear();
        }

        public void Reset()
        {
            _undoList.Clear();
            _redoList.Clear();
        }

    }
}
