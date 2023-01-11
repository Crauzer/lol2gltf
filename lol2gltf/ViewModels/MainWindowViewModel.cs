using HanumanInstitute.MvvmDialogs;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace lol2gltf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive]
        public MainViewModel MainViewModel { get; set; }

        private IDialogService _dialogService;

        public MainWindowViewModel(IDialogService dialogService)
        {
            this._dialogService = dialogService;

            this.MainViewModel = new();
        }
    }
}
