using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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

        public List<PageViewModel> Pages { get; } =
            new() { new SkinnedMeshToGltfViewModel(), new MapGeometryToGltfViewModel() };

        public MainViewModel()
        {
            this.CurrentPage = this.Pages[0];
        }
    }
}
