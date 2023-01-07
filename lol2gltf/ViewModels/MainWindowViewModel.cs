using System;
using System.Collections.Generic;
using System.Text;

namespace lol2gltf.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainViewModel MainViewModel { get; }

        public MainWindowViewModel()
        {
            this.MainViewModel = new();
        }
    }
}
