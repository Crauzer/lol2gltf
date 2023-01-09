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
using System.Reactive.Linq;
using LeagueToolkit.IO.SkeletonFile;
using System.Collections.ObjectModel;
using Avalonia.Collections;
using DynamicData.Binding;
using LeagueToolkit.IO.SimpleSkinFile;
using CommunityToolkit.Diagnostics;
using SharpGLTF.Schema2;

using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;
using DynamicData;

namespace lol2gltf.ViewModels
{
    public sealed class SkinnedMeshToGltfViewModel : PageViewModel, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();

        [Reactive]
        public string SimpleSkinPath { get; set; }

        [Reactive]
        public string SkeletonPath { get; set; }

        public SkinnedMesh SkinnedMesh => this._skinnedMesh.Value;
        public Skeleton Skeleton => this._skeleton.Value;

        [Reactive]
        public ObservableCollection<SkinnedMeshPrimitiveViewModel> SkinnedMeshPrimitives { get; set; } = new();

        private ObservableAsPropertyHelper<SkinnedMesh> _skinnedMesh;
        private ObservableAsPropertyHelper<Skeleton> _skeleton;

        public ReactiveCommand<Unit, Unit> OnSelectSimpleSkinCommand { get; }
        public ReactiveCommand<Unit, Unit> OnSelectSkeletonCommand { get; }
        public ReactiveCommand<string, Unit> OnExportGltfCommand { get; }

        public Interaction<Unit, string> ShowLoadSimpleSkinDialog { get; }
        public Interaction<Unit, string> ShowLoadSkeletonDialog { get; }
        public Interaction<string, string> ShowExportGltfDialog { get; }

        public SkinnedMeshToGltfViewModel() : this("Skinned Mesh to glTf", "", Symbol.OpenFile) { }

        public SkinnedMeshToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon)
        {
            this.OnSelectSimpleSkinCommand = ReactiveCommand.CreateFromTask(LoadSimpleSkinAsync);
            this.OnSelectSkeletonCommand = ReactiveCommand.CreateFromTask(LoadSkeletonAsync);
            this.OnExportGltfCommand = ReactiveCommand.CreateFromTask<string>(ExportGltfAsync);

            this.ShowLoadSimpleSkinDialog = new();
            this.ShowLoadSkeletonDialog = new();
            this.ShowExportGltfDialog = new();

            this._skinnedMesh = this.WhenAnyValue(x => x.SimpleSkinPath)
                .Select(
                    x =>
                        x switch
                        {
                            null => null,
                            string simpleSkinPath => SkinnedMesh.ReadFromSimpleSkin(simpleSkinPath)
                        }
                )
                .ToProperty(this, nameof(this.SkinnedMesh));

            this._skeleton = this.WhenAnyValue(x => x.SkeletonPath)
                .Select(
                    x =>
                        x switch
                        {
                            null => null,
                            string skeletonPath => new Skeleton(skeletonPath)
                        }
                )
                .ToProperty(this, nameof(this.Skeleton));

            this.WhenValueChanged(x => x.SkinnedMesh)
                .Subscribe(skinnedMesh =>
                {
                    this.SkinnedMeshPrimitives.Clear();

                    if (skinnedMesh is not null)
                    {
                        this.SkinnedMeshPrimitives.AddRange(
                            skinnedMesh.Ranges.Select(
                                range =>
                                    new SkinnedMeshPrimitiveViewModel()
                                    {
                                        Material = range.Material,
                                        VertexCount = range.VertexCount,
                                        FaceCount = range.IndexCount / 3
                                    }
                            )
                        );
                    }
                });
        }

        private async Task LoadSimpleSkinAsync()
        {
            string path = await this.ShowLoadSimpleSkinDialog.Handle(new());

            this.SimpleSkinPath = path;
        }

        private async Task LoadSkeletonAsync()
        {
            string path = await this.ShowLoadSkeletonDialog.Handle(new());

            this.SkeletonPath = path;
        }

        private async Task ExportGltfAsync(string extension)
        {
            if (extension is not ".glb" or ".gltf")
                throw new ArgumentException($"Invalid extension: {extension}", nameof(extension));

            string path = await this.ShowExportGltfDialog.Handle(extension);
            if (string.IsNullOrEmpty(path))
                return; // User canceled

            Dictionary<string, ReadOnlyMemory<byte>> materialTextures = new();
            List<(string name, LeagueAnimation animation)> animations = new();

            ModelRoot gltfAsset = this.Skeleton switch
            {
                null => this.SkinnedMesh.ToGltf(materialTextures),
                Skeleton skeleton => this.SkinnedMesh.ToGltf(skeleton, materialTextures, animations)
            };

            gltfAsset.Save(path);
        }
    }

    public class SkinnedMeshPrimitiveViewModel
    {
        public string Material { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }
    }
}
