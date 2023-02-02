﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
using CommunityToolkit.Diagnostics;
using DynamicData;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using lol2gltf.Helpers;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

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
        public ObservableCollection<SkinnedMeshPrimitiveViewModel> SkinnedMeshPrimitives { get; set; } = new();

        [Reactive]
        public ObservableCollection<AnimationViewModel> Animations { get; set; } = new();

        public SkinnedMesh SimpleSkin => this._simpleSkin?.Value;
        private ObservableAsPropertyHelper<SkinnedMesh> _simpleSkin;

        public RigResource Skeleton => this._skeleton?.Value;
        private ObservableAsPropertyHelper<RigResource> _skeleton;

        public ReactiveCommand<Unit, string> SelectSimpleSkinPathCommand { get; }
        public ReactiveCommand<Unit, string> SelectSkeletonPathCommand { get; }

        public ReactiveCommand<string, SkinnedMesh> LoadSimpleSkinCommand { get; }
        public ReactiveCommand<string, RigResource> LoadSkeletonCommand { get; }

        public ReactiveCommand<Unit, Unit> AddAnimationsCommand { get; }
        public ReactiveCommand<IList, Unit> RemoveSelectedAnimationsCommand { get; }

        public ReactiveCommand<string, Unit> ExportGltfCommand { get; }

        public Interaction<Unit, string> ShowSelectSimpleSkinDialog { get; }
        public Interaction<Unit, string> ShowSelectSkeletonDialog { get; }
        public Interaction<Unit, string[]> ShowAddAnimationsDialog { get; }
        public Interaction<string, string> ShowSelectSavedGltfDialog { get; }
        public Interaction<string, Unit> ShowSaveGltfDialog { get; }

        public string GlbExtension = "glb";
        public string GltfExtension = "gltf";

        public SkinnedMeshToGltfViewModel()
            : this("Skinned Mesh to glTf", "Convert Skinned Meshes to glTF", Symbol.OpenFile) { }

        public SkinnedMeshToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon)
        {
            Guard.IsNotNullOrEmpty(name, nameof(name));
            Guard.IsNotNullOrEmpty(tooltip, nameof(tooltip));

            this.SelectSimpleSkinPathCommand = ReactiveCommand.CreateFromTask(SelectSimpleSkinPathAsync);
            this.SelectSkeletonPathCommand = ReactiveCommand.CreateFromTask(SelectSkeletonPathAsync);

            this.LoadSimpleSkinCommand = ReactiveCommand.Create<string, SkinnedMesh>(LoadSimpleSkin);
            this.LoadSkeletonCommand = ReactiveCommand.Create<string, RigResource>(LoadSkeleton);

            this.AddAnimationsCommand = ReactiveCommand.CreateFromTask(AddAnimationsAsync);
            this.RemoveSelectedAnimationsCommand = ReactiveCommand.Create<IList>(RemoveSelectedAnimations);

            this.ExportGltfCommand = ReactiveCommand.CreateFromObservable<string, Unit>(
                ExportGltfAsync,
                outputScheduler: RxApp.MainThreadScheduler
            );

            this.ShowSelectSimpleSkinDialog = new();
            this.ShowSelectSkeletonDialog = new();
            this.ShowAddAnimationsDialog = new();
            this.ShowSelectSavedGltfDialog = new();
            this.ShowSaveGltfDialog = new();

            this.WhenActivated(disposables =>
            {
                // Handle path selection
                this.SelectSimpleSkinPathCommand.WhereNotNull().Subscribe(x => this.SimpleSkinPath = x);
                this.SelectSkeletonPathCommand.WhereNotNull().Subscribe(x => this.SkeletonPath = x);

                // Handle loading
                this._simpleSkin = this.LoadSimpleSkinCommand
                    .ToProperty(this, nameof(this.SimpleSkin))
                    .DisposeWith(disposables);
                this._skeleton = this.LoadSkeletonCommand
                    .ToProperty(this, nameof(this.Skeleton))
                    .DisposeWith(disposables);

                // When paths change, re-load files
                this.WhenAnyValue(x => x.SimpleSkinPath).InvokeCommand(LoadSimpleSkinCommand);
                this.WhenAnyValue(x => x.SkeletonPath).InvokeCommand(LoadSkeletonCommand);

                // Handle simple skin dependencies
                this.WhenAnyValue(x => x.SimpleSkin)
                    .Subscribe(simpleSkin =>
                    {
                        // Reset mesh primitives, skeleton and animations when mesh gets loaded
                        this.SkeletonPath = null;
                        this.SkinnedMeshPrimitives.Clear();
                        this.Animations.Clear();

                        if (simpleSkin is null)
                            return;

                        this.SkinnedMeshPrimitives.AddRange(
                            simpleSkin.Ranges.Select(
                                range =>
                                    new SkinnedMeshPrimitiveViewModel()
                                    {
                                        Material = range.Material,
                                        VertexCount = range.VertexCount,
                                        FaceCount = range.IndexCount / 3
                                    }
                            )
                        );

                        NotificationHelper.ShowSuccess("Loaded Simple Skin");
                    });

                this.WhenAnyValue(x => x.Skeleton)
                    .WhereNotNull()
                    .Subscribe(_ =>
                    {
                        NotificationHelper.ShowSuccess("Loaded Skeleton");
                    });

                this.ExportGltfCommand.Subscribe(_ => NotificationHelper.ShowSuccess("Exported glTF"));

                this.LoadSimpleSkinCommand.ThrownExceptions.Subscribe(ex =>
                {
                    this.SimpleSkinPath = null;
                    NotificationHelper.Show(ex);
                    this.Log().Error(ex);
                });
                this.LoadSkeletonCommand.ThrownExceptions.Subscribe(ex =>
                {
                    this.SkeletonPath = null;
                    NotificationHelper.Show(ex);
                    this.Log().Error(ex);
                });
                this.AddAnimationsCommand.ThrownExceptions.Subscribe(ex =>
                {
                    NotificationHelper.Show(ex);
                    this.Log().Error(ex);
                });
                this.ExportGltfCommand.ThrownExceptions.Subscribe(ex =>
                {
                    NotificationHelper.Show(ex);
                    this.Log().Error(ex);
                });
            });
        }

        private async Task<string> SelectSimpleSkinPathAsync()
        {
            string path = await this.ShowSelectSimpleSkinDialog.Handle(new());

            return path;
        }

        private async Task<string> SelectSkeletonPathAsync()
        {
            string path = await this.ShowSelectSkeletonDialog.Handle(new());

            return path;
        }

        private SkinnedMesh LoadSimpleSkin(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                return SkinnedMesh.ReadFromSimpleSkin(path);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to load Simple Skin", exception);
            }
        }

        private RigResource LoadSkeleton(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                using FileStream stream = File.OpenRead(path);
                return new(stream);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Failed to load skeleton", exception);
            }
        }

        private async Task AddAnimationsAsync()
        {
            string[] paths = await this.ShowAddAnimationsDialog.Handle(new());
            if (paths is null)
                return;

            // Filter out existing animations
            IEnumerable<string> uniqueNewPaths = paths.Where(
                x => !this.Animations.Any(animation => animation.Name == Path.GetFileNameWithoutExtension(x))
            );
            foreach (string path in uniqueNewPaths)
            {
                string animationName = Path.GetFileNameWithoutExtension(path);
                try
                {
                    using FileStream stream = File.OpenRead(path);
                    IAnimationAsset animation = AnimationAsset.Load(stream);
                    this.Animations.Add(new(animationName, animation));
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Failed to load animation: {animationName}", exception);
                }
            }
        }

        private void RemoveSelectedAnimations(IList selectedItems)
        {
            if (selectedItems is null)
                return;

            List<AnimationViewModel> selectedAnimations = new(selectedItems.Cast<AnimationViewModel>());

            this.Animations.RemoveMany(selectedAnimations);
        }

        public IObservable<Unit> ExportGltfAsync(string extension)
        {
            return Observable.StartAsync(async _ =>
            {
                Guard.IsNotNullOrEmpty(extension, nameof(extension));

                if (extension is not "glb" and not "gltf")
                    throw new ArgumentException($"Invalid extension: {extension}", nameof(extension));

                string path = await this.ShowSelectSavedGltfDialog.Handle(extension);
                if (string.IsNullOrEmpty(path))
                    return;

                try
                {
                    await this.ShowSaveGltfDialog.Handle(path);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Failed to export glTF", exception);
                }
            });
        }
    }

    public class SkinnedMeshPrimitiveViewModel : ViewModelBase, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();

        public string Material { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }

        [Reactive]
        public string TexturePath { get; set; }

        public ReactiveCommand<Unit, Unit> OnSelectTextureCommand { get; }
        public ReactiveCommand<Unit, Unit> OnRemoveTextureCommand { get; }

        public SkinnedMeshPrimitiveViewModel()
        {
            this.OnSelectTextureCommand = ReactiveCommand.CreateFromTask(SelectTextureAsync);
            this.OnRemoveTextureCommand = ReactiveCommand.Create(() =>
            {
                this.TexturePath = null;
            });
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
                            Name = "Texture files",
                            Extensions = new() { "png", "dds" }
                        },
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

    public class AnimationViewModel
    {
        public string Name { get; }
        public float FPS => this.Animation.Fps;

        public IAnimationAsset Animation { get; }

        public AnimationViewModel(string name, IAnimationAsset animation)
        {
            this.Name = name;
            this.Animation = animation;
        }
    }
}
