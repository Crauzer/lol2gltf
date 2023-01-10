﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;

namespace lol2gltf.ViewModels
{
    public sealed class SkinnedMeshToGltfViewModel : PageViewModel, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();

        [Reactive]
        public string SimpleSkinPath { get; set; }

        [Reactive]
        public string SkeletonPath { get; set; }

        [Reactive]
        public bool IsSkinnedMeshLoaded { get; set; }

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

                    if (skinnedMesh is null)
                    {
                        this.IsSkinnedMeshLoaded = false;
                        return;
                    }

                    // If skinnedMesh is not null we create primitives datagrid items and enable conversion
                    this.IsSkinnedMeshLoaded = true;
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
            if (extension is not ".glb" and not ".gltf")
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

    public class SkinnedMeshPrimitiveViewModel : ViewModelBase
    {
        public string Material { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }

        [Reactive]
        public string TexturePath { get; set; }

        public ReactiveCommand<Unit, Unit> OnSelectTextureCommand { get; }

        public SkinnedMeshPrimitiveViewModel()
        {
            this.OnSelectTextureCommand = ReactiveCommand.CreateFromTask(SelectTextureAsync);
        }

        private async Task SelectTextureAsync()
        {
            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "Select a Texture file",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "PNG files",
                            Extensions = new() { "png" }
                        },
                        new()
                        {
                            Name = "DDS files",
                            Extensions = new() { "dds" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            this.TexturePath = files?.FirstOrDefault();
        }
    }
}
