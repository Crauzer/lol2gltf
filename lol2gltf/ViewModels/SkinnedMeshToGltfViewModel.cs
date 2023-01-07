using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using LeagueToolkit.Core.Mesh;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using lol2gltf.Utilities;
using lol2gltf.Views;
using Splat;
using Avalonia;
using System.Windows;

namespace lol2gltf.ViewModels
{
    public sealed class SkinnedMeshToGltfViewModel : PageViewModel
    {
        [Reactive]
        public string SimpleSkinPath { get; set; }

        [Reactive]
        public string SkeletonPath { get; set; }

        public ReactiveCommand<Unit, (string path, SkinnedMesh skinnedMesh)> OnSelectSimpleSkinCommand { get; }

        private readonly IDialogService _dialogService;

        public SkinnedMeshToGltfViewModel(IDialogService dialogService = null)
            : this(
                dialogService ?? Locator.Current.GetService<IDialogService>(),
                "Skinned Mesh to glTf",
                "",
                Symbol.OpenFile
            ) { }

        public SkinnedMeshToGltfViewModel(IDialogService dialogService, string name, string tooltip, Symbol icon)
            : base(name, tooltip, icon)
        {
            this._dialogService = dialogService;
            this.OnSelectSimpleSkinCommand = ReactiveCommand.CreateFromTask(LoadSimpleSkin);
        }

        private async Task<(string path, SkinnedMesh skinnedMesh)> LoadSimpleSkin()
        {
            var x = this._dialogService.ShowLoadSkinnedMeshViewAsync(App.MainWindow);

            return await x;
        }
    }
}
