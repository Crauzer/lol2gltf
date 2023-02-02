using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using BCnEncoder.Shared;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Toolkit;
using lol2gltf.ViewModels;
using ReactiveUI;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using LeagueTexture = LeagueToolkit.Core.Renderer.Texture;

namespace lol2gltf.Views
{
    public partial class SkinnedMeshToGltfView : ReactiveUserControl<SkinnedMeshToGltfViewModel>
    {
        public SkinnedMeshToGltfView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Bind commands
                this.BindCommand(this.ViewModel, vm => vm.SelectSimpleSkinPathCommand, v => v.LoadSimpleSkinButton);
                this.BindCommand(this.ViewModel, vm => vm.SelectSkeletonPathCommand, v => v.LoadSkeletonButton);
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

                // Bind interactions
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

                    List<(string, Stream)> textures = await CreateMaterialTexturesAsync(
                        this.ViewModel.SkinnedMeshPrimitives
                    );
                    IEnumerable<(string, IAnimationAsset)> animations = this.ViewModel.Animations
                        .Select(animation => (animation.Name, animation.Animation))
                        .DistinctBy(animation => animation.Name);

                    SkinnedMesh skinnedMesh = this.ViewModel.SimpleSkin;
                    RigResource skeleton = this.ViewModel.Skeleton;
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
                                null => skinnedMesh.ToGltf(textures),
                                RigResource skeleton => skinnedMesh.ToGltf(skeleton, textures, animations)
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

        private static async Task<List<(string Material, Stream Texture)>> CreateMaterialTexturesAsync(
            IEnumerable<SkinnedMeshPrimitiveViewModel> primitives
        )
        {
            List<(string, Stream)> textures = new();

            IEnumerable<SkinnedMeshPrimitiveViewModel> texturedPrimitives = primitives.Where(
                x => string.IsNullOrEmpty(x.TexturePath) is false
            );
            foreach (SkinnedMeshPrimitiveViewModel primitive in texturedPrimitives)
            {
                using FileStream textureFileStream = File.OpenRead(primitive.TexturePath);
                LeagueTexture texture = LeagueTexture.Load(textureFileStream);

                ReadOnlyMemory2D<ColorRgba32> mipMap = texture.Mips[0];
                using ImageSharpImage image = mipMap.ToImage();

                MemoryStream imageStream = new();
                await image.SaveAsPngAsync(imageStream);
                imageStream.Seek(0, SeekOrigin.Begin);

                textures.Add((primitive.Material, imageStream));
            }

            return textures;
        }
    }
}
