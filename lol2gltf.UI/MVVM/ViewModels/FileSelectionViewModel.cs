using lol2gltf.UI.MVVM.Commands;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class FileSelectionViewModel : PropertyNotifier
    {
        public string FilePath
        {
            get => this._filePath;
            set
            {
                this._filePath = value;
                NotifyPropertyChanged();
            }
        }

        private string _filePath;

        private string _dialogName;
        private ICollection<CommonFileDialogFilter> _filters;

        private SelectionChanged _onSelectionChanged;

        public ICommand SelectFileCommand => new RelayCommand(SelectFile);

        public delegate void SelectionChanged(string filePath);

        public FileSelectionViewModel(string dialogName, SelectionChanged onSelectionChanged, ICollection<CommonFileDialogFilter> filters)
        {
            Contract.Ensures(onSelectionChanged is not null);

            this._dialogName = dialogName ?? "Open";
            this._filters = filters ?? new List<CommonFileDialogFilter>();
            this._onSelectionChanged = onSelectionChanged;
        }
        public FileSelectionViewModel(string dialogName, SelectionChanged onSelectionChanged, params CommonFileDialogFilter[] filters)
        {
            Contract.Ensures(onSelectionChanged is not null);

            this._dialogName = dialogName ?? "Open";
            this._filters = filters?.ToList() ?? new List<CommonFileDialogFilter>();
            this._onSelectionChanged = onSelectionChanged;
        }

        private void SelectFile(object o)
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog(this._dialogName);

            foreach (CommonFileDialogFilter filter in this._filters)
            {
                dialog.Filters.Add(filter);
            }

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.FilePath = dialog.FileName;

                this._onSelectionChanged(dialog.FileName);
            }
        }
    }
}
