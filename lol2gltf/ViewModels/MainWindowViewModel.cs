using Avalonia.Controls.Notifications;
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

        public MainWindowViewModel()
        {
            this.MainViewModel = new();
        }
    }
}
