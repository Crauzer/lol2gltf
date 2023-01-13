using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lol2gltf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        [Reactive]
        public PageViewModel CurrentPage { get; set; }

        public List<PageViewModel> Pages { get; }

        public MainViewModel()
        {
            this.Pages = new() { new SkinnedMeshToGltfViewModel(), new MapGeometryToGltfViewModel() };

            this.CurrentPage = this.Pages[0];
        }
    }
}
