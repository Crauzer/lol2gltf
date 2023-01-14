using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
using CommunityToolkit.Diagnostics;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Textures.TextureFormats;
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
using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpTexture = SixLabors.ImageSharp.Textures.Texture;
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

        public SkinnedMesh SimpleSkin => this._simpleSkin?.Value;
        public Skeleton Skeleton => this._skeleton?.Value;

        [Reactive]
        public ObservableCollection<SkinnedMeshPrimitiveViewModel> SkinnedMeshPrimitives { get; set; } = new();

        [Reactive]
        public ObservableCollection<AnimationViewModel> Animations { get; set; } = new();

        private ObservableAsPropertyHelper<SkinnedMesh> _simpleSkin;
        private ObservableAsPropertyHelper<Skeleton> _skeleton;

        public ReactiveCommand<Unit, SkinnedMesh> LoadSimpleSkinCommand { get; }
        public ReactiveCommand<Unit, Skeleton> LoadSkeletonCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAnimationsCommand { get; }
        public ReactiveCommand<string, Unit> ExportGltfCommand { get; }

        public ReactiveCommand<IList, Unit> RemoveSelectedAnimationsCommand { get; }

        public Interaction<Unit, string> ShowSelectSimpleSkinDialog { get; }
        public Interaction<Unit, string> ShowSelectSkeletonDialog { get; }
        public Interaction<Unit, string[]> ShowAddAnimationsDialog { get; }
        public Interaction<string, string> ShowSelectSavedGltfDialog { get; }
        public Interaction<string, Unit> ShowSaveGltfDialog { get; }

        public string GlbExtension = "glb";
        public string GltfExtension = "glb";

        public SkinnedMeshToGltfViewModel()
            : this("Skinned Mesh to glTf", "Convert Skinned Meshes to glTF", Symbol.OpenFile) { }

        public SkinnedMeshToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon)
        {
            Guard.IsNotNullOrEmpty(name, nameof(name));
            Guard.IsNotNullOrEmpty(tooltip, nameof(tooltip));

            this.LoadSimpleSkinCommand = ReactiveCommand.CreateFromTask(LoadSimpleSkinAsync);
            this.LoadSkeletonCommand = ReactiveCommand.CreateFromTask(LoadSkeletonAsync);
            this.AddAnimationsCommand = ReactiveCommand.CreateFromTask(AddAnimationsAsync);
            this.ExportGltfCommand = ReactiveCommand.CreateFromObservable<string, Unit>(
                ExportGltfAsync,
                outputScheduler: RxApp.MainThreadScheduler
            );

            this.RemoveSelectedAnimationsCommand = ReactiveCommand.Create<IList>(RemoveSelectedAnimations);

            this.ShowSelectSimpleSkinDialog = new();
            this.ShowSelectSkeletonDialog = new();
            this.ShowAddAnimationsDialog = new();
            this.ShowSelectSavedGltfDialog = new();
            this.ShowSaveGltfDialog = new();

            this.WhenActivated(disposables =>
            {
                this._simpleSkin = this.LoadSimpleSkinCommand
                    .WhereNotNull()
                    .ToProperty(this, nameof(this.SimpleSkin), scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);

                this._skeleton = this.LoadSkeletonCommand
                    .WhereNotNull()
                    .ToProperty(this, nameof(this.Skeleton), scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);

                this.LoadSimpleSkinCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.LoadSkeletonCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.AddAnimationsCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.ExportGltfCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.RemoveSelectedAnimationsCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
            });
        }

        private async Task<SkinnedMesh> LoadSimpleSkinAsync()
        {
            string path = await this.ShowSelectSimpleSkinDialog.Handle(new());

            if (string.IsNullOrEmpty(path))
                return null;

            this.SimpleSkinPath = path;

            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(path);

            // Reset mesh primitives, skeleton and animations when mesh gets loaded
            this.SkeletonPath = null;
            this.SkinnedMeshPrimitives.Clear();
            this.Animations.Clear();

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

            return simpleSkin;
        }

        private async Task<Skeleton> LoadSkeletonAsync()
        {
            string path = await this.ShowSelectSkeletonDialog.Handle(new());

            if (string.IsNullOrEmpty(path))
                return null;

            this.SkeletonPath = path;

            return new(path);
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
                LeagueAnimation animation = new(path);

                this.Animations.Add(new(Path.GetFileNameWithoutExtension(path), animation));
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

                await this.ShowSaveGltfDialog.Handle(path);
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
        public float FPS => this.Animation.FPS;

        public LeagueAnimation Animation { get; }

        public AnimationViewModel(string name, LeagueAnimation animation)
        {
            this.Name = name;
            this.Animation = animation;
        }
    }
}
