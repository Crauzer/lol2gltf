using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SkeletonFile;
using lol2gltf.ViewModels;
using ReactiveUI;
using SharpGLTF.Schema2;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Textures.TextureFormats;
using System.IO;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using ImageSharpTexture = SixLabors.ImageSharp.Textures.Texture;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;
using CommunityToolkit.Diagnostics;
using FluentAvalonia.UI.Controls;
using SixLabors.ImageSharp;
using Avalonia.Controls.Shapes;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.MapGeometryFile;
using System.Reactive.Concurrency;

namespace lol2gltf.Views
{
    public partial class SkinnedMeshToGltfView : ReactiveUserControl<SkinnedMeshToGltfViewModel>
    {
        public SkinnedMeshToGltfView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.BindCommand(this.ViewModel, vm => vm.LoadSimpleSkinCommand, v => v.LoadSimpleSkinButton);
                this.BindCommand(this.ViewModel, vm => vm.LoadSkeletonCommand, v => v.LoadSkeletonButton);
                this.BindCommand(this.ViewModel, vm => vm.AddAnimationsCommand, v => v.AddAnimationsButton);
                this.BindCommand(
                        this.ViewModel,
                        vm => vm.ExportGltfCommand,
                        v => v.ExportGlbButton,
                        vm => vm.GlbExtension
                    )
                    .DisposeWith(disposables);
                this.BindCommand(
                        this.ViewModel,
                        vm => vm.ExportGltfCommand,
                        v => v.ExportGltfButton,
                        vm => vm.GltfExtension
                    )
                    .DisposeWith(disposables);

                this.BindInteraction(
                        this.ViewModel,
                        vm => vm.ShowSelectSimpleSkinDialog,
                        DoShowLoadSimpleSkinDialogAsync
                    )
                    .DisposeWith(disposables);
                this.BindInteraction(this.ViewModel, vm => vm.ShowSelectSkeletonDialog, DoShowLoadSkeletonDialogAsync)
                    .DisposeWith(disposables);
                this.BindInteraction(this.ViewModel, vm => vm.ShowAddAnimationsDialog, DoShowAddAnimationsDialogAsync)
                    .DisposeWith(disposables);
                this.BindInteraction(this.ViewModel, vm => vm.ShowSelectSavedGltfDialog, ShowSelectSavedGltfDialogAsync)
                    .DisposeWith(disposables);
                this.BindInteraction(this.ViewModel, vm => vm.ShowSaveGltfDialog, ShowSaveGltfDialogAsync)
                    .DisposeWith(disposables);
            });
        }

        private async Task DoShowLoadSimpleSkinDialogAsync(InteractionContext<Unit, string> interaction)
        {
            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "Select Simple Skin (.skn) files",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Simple Skin files",
                            Extensions = new() { "skn" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files?.FirstOrDefault());
        }

        private async Task DoShowLoadSkeletonDialogAsync(InteractionContext<Unit, string> interaction)
        {
            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "Select Skeleton (.skl) files",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Skeleton files",
                            Extensions = new() { "skl" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files?.FirstOrDefault());
        }

        private async Task DoShowAddAnimationsDialogAsync(InteractionContext<Unit, string[]> interaction)
        {
            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = true,
                    Title = "Select Animation files",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Animation files",
                            Extensions = new() { "anm" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files);
        }

        private async Task ShowSelectSavedGltfDialogAsync(InteractionContext<string, string> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            string extension = interaction.Input;
            if (extension is not "glb" and not "gltf")
                ThrowHelper.ThrowArgumentException($"Invalid extension: {extension}", nameof(extension));

            SaveFileDialog dialog =
                new()
                {
                    DefaultExtension = extension,
                    Filters = new()
                    {
                        new()
                        {
                            Name = $"glTF files",
                            Extensions = new() { extension }
                        }
                    }
                };

            string file = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(file);
        }

        public IObservable<Unit> ShowSaveGltfDialogAsync(InteractionContext<string, Unit> interaction)
        {
            return Observable.StartAsync(
                async _ =>
                {
                    string path = interaction.Input;
                    if (string.IsNullOrEmpty(path))
                        ThrowHelper.ThrowInvalidOperationException($"{nameof(path)} must be set");

                    TaskDialogProgressState progressState =
                        TaskDialogProgressState.Normal | TaskDialogProgressState.Indeterminate;

                    Dictionary<string, ReadOnlyMemory<byte>> materialTextures = await CreateMaterialTexturesAsync(
                        this.ViewModel.SkinnedMeshPrimitives
                    );
                    List<(string name, LeagueAnimation animation)> animations =
                        new(
                            this.ViewModel.Animations
                                .Select(animation => (animation.Name, animation.Animation))
                                .DistinctBy(animation => animation.Name)
                        );

                    SkinnedMesh skinnedMesh = this.ViewModel.SimpleSkin;
                    Skeleton skeleton = this.ViewModel.Skeleton;
                    TaskDialog dialog =
                        new()
                        {
                            Title = "Map Geometry to glTF",
                            IconSource = new SymbolIconSource { Symbol = Symbol.SaveAsFilled },
                            ShowProgressBar = true
                        };

                    dialog.Opened += async (dialog, e) =>
                    {
                        await Task.Run(() =>
                        {
                            // For some reason when "showHosted" is true, the progress bar
                            // doesn't update properly, if it's false, it does
                            dialog.SetProgressBarState(50, progressState);

                            // Convert to glTF
                            RxApp.MainThreadScheduler.Schedule(() =>
                            {
                                dialog.Content = "Converting to glTF...";
                            });

                            ModelRoot gltfAsset = skeleton switch
                            {
                                null => skinnedMesh.ToGltf(materialTextures),
                                Skeleton skeleton => skinnedMesh.ToGltf(skeleton, materialTextures, animations)
                            };

                            // Save glTF asset
                            RxApp.MainThreadScheduler.Schedule(() =>
                            {
                                dialog.Content = "Saving glTF...";
                            });

                            gltfAsset.Save(path);

                            // Finish
                            RxApp.MainThreadScheduler.Schedule(() =>
                            {
                                dialog.Hide(TaskDialogStandardResult.OK);
                            });
                        });
                    };

                    dialog.XamlRoot = this.VisualRoot;
                    await dialog.ShowAsync(true);

                    interaction.SetOutput(Unit.Default);
                },
                RxApp.MainThreadScheduler
            );
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
}
