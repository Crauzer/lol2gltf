using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
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

        [Reactive]
        public bool IsSkinnedMeshLoaded { get; set; }

        public SkinnedMesh SkinnedMesh => this._skinnedMesh.Value;
        public Skeleton Skeleton => this._skeleton.Value;

        [Reactive]
        public ObservableCollection<SkinnedMeshPrimitiveViewModel> SkinnedMeshPrimitives { get; set; } = new();

        [Reactive]
        public ObservableCollection<AnimationViewModel> Animations { get; set; } = new();

        private ObservableAsPropertyHelper<SkinnedMesh> _skinnedMesh;
        private ObservableAsPropertyHelper<Skeleton> _skeleton;

        public ReactiveCommand<Unit, Unit> OnSelectSimpleSkinCommand { get; }
        public ReactiveCommand<Unit, Unit> OnSelectSkeletonCommand { get; }
        public ReactiveCommand<Unit, Unit> OnAddAnimationsCommand { get; }
        public ReactiveCommand<string, Unit> OnExportGltfCommand { get; }

        public ReactiveCommand<IList, Unit> OnRemoveSelectedAnimationsCommand { get; }

        public Interaction<Unit, string> ShowLoadSimpleSkinDialog { get; }
        public Interaction<Unit, string> ShowLoadSkeletonDialog { get; }
        public Interaction<Unit, string[]> ShowAddAnimationsDialog { get; }
        public Interaction<string, string> ShowExportGltfDialog { get; }

        public SkinnedMeshToGltfViewModel() : this("Skinned Mesh to glTf", "", Symbol.OpenFile) { }

        public SkinnedMeshToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon)
        {
            this.OnSelectSimpleSkinCommand = ReactiveCommand.CreateFromTask(LoadSimpleSkinAsync);
            this.OnSelectSkeletonCommand = ReactiveCommand.CreateFromTask(LoadSkeletonAsync);
            this.OnAddAnimationsCommand = ReactiveCommand.CreateFromTask(AddAnimationsAsync);
            this.OnExportGltfCommand = ReactiveCommand.CreateFromTask<string>(ExportGltfAsync);

            this.OnRemoveSelectedAnimationsCommand = ReactiveCommand.CreateFromTask<IList>(
                RemoveSelectedAnimationsAsync
            );

            this.ShowLoadSimpleSkinDialog = new();
            this.ShowLoadSkeletonDialog = new();
            this.ShowAddAnimationsDialog = new();
            this.ShowExportGltfDialog = new();

            this.WhenActivated(disposables =>
            {
                this._skinnedMesh = this.WhenAnyValue(x => x.SimpleSkinPath)
                    .Select(
                        x =>
                            x switch
                            {
                                null => null,
                                string simpleSkinPath => SkinnedMesh.ReadFromSimpleSkin(simpleSkinPath)
                            }
                    )
                    .ToProperty(this, nameof(this.SkinnedMesh))
                    .DisposeWith(disposables);

                this._skeleton = this.WhenAnyValue(x => x.SkeletonPath)
                    .Select(
                        x =>
                            x switch
                            {
                                null => null,
                                string skeletonPath => new Skeleton(skeletonPath)
                            }
                    )
                    .ToProperty(this, nameof(this.Skeleton))
                    .DisposeWith(disposables);

                this.WhenValueChanged(x => x.SkinnedMesh)
                    .Subscribe(skinnedMesh =>
                    {
                        // Clear mesh primitives, skeleton and animation when mesh is changed
                        this.SkeletonPath = null;
                        this.SkinnedMeshPrimitives.Clear();
                        this.Animations.Clear();

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
                    })
                    .DisposeWith(disposables);
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

        private async Task AddAnimationsAsync()
        {
            string[] paths = await this.ShowAddAnimationsDialog.Handle(new());

            if (paths is null)
                return;

            foreach (string path in paths)
            {
                LeagueAnimation animation = new(path);

                this.Animations.Add(new(Path.GetFileNameWithoutExtension(path), animation));
            }
        }

        private async Task RemoveSelectedAnimationsAsync(IList selectedItems)
        {
            List<AnimationViewModel> selectedAnimations = new(selectedItems.Cast<AnimationViewModel>());

            this.Animations.RemoveMany(selectedAnimations);

            await Task.CompletedTask;
        }

        private async Task ExportGltfAsync(string extension)
        {
            if (extension is not ".glb" and not ".gltf")
                throw new ArgumentException($"Invalid extension: {extension}", nameof(extension));

            string path = await this.ShowExportGltfDialog.Handle(extension);
            if (string.IsNullOrEmpty(path))
                return; // User canceled

            Dictionary<string, ReadOnlyMemory<byte>> materialTextures = await CreateMaterialTexturesAsync(
                this.SkinnedMeshPrimitives
            );
            List<(string name, LeagueAnimation animation)> animations =
                new(this.Animations.Select(animation => (animation.Name, animation.Animation)));

            ModelRoot gltfAsset = this.Skeleton switch
            {
                null => this.SkinnedMesh.ToGltf(materialTextures),
                Skeleton skeleton => this.SkinnedMesh.ToGltf(skeleton, materialTextures, animations)
            };

            gltfAsset.Save(path);
        }

        private static async Task<Dictionary<string, ReadOnlyMemory<byte>>> CreateMaterialTexturesAsync(
            IEnumerable<SkinnedMeshPrimitiveViewModel> primitives
        )
        {
            Dictionary<string, ReadOnlyMemory<byte>> materialTextures = new();

            IEnumerable<SkinnedMeshPrimitiveViewModel> texturedPrimitives = primitives.Where(
                x => string.IsNullOrEmpty(x.TexturePath) is false
            );
            foreach (SkinnedMeshPrimitiveViewModel primitive in texturedPrimitives)
            {
                using ImageSharpTexture texture = ImageSharpTexture.Load(primitive.TexturePath);

                if (texture is FlatTexture flatTexture)
                {
                    using MemoryStream imageStream = new();
                    using ImageSharpImage img = flatTexture.MipMaps[0].GetImage();

                    await img.SaveAsPngAsync(imageStream);

                    materialTextures.Add(primitive.Material, imageStream.ToArray());
                }
            }

            return materialTextures;
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
