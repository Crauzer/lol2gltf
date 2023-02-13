using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using CommunityToolkit.Diagnostics;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.Core.Environment;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.Meta;
using lol2gltf.ViewModels;
using ReactiveUI;
using SharpGLTF.Schema2;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace lol2gltf.Views
{
    public partial class MapGeometryToGltfView : ReactiveUserControl<MapGeometryToGltfViewModel>
    {
        public MapGeometryToGltfView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                // Bind Commands
                this.BindCommand(this.ViewModel, vm => vm.SelectMapGeometryPathCommand, v => v.LoadMapGeometryButton)
                    .DisposeWith(disposables);
                this.BindCommand(this.ViewModel, vm => vm.SelectMaterialsBinPathCommand, v => v.LoadMaterialsBinButton)
                    .DisposeWith(disposables);
                this.BindCommand(this.ViewModel, vm => vm.SelectGameDataPathCommand, v => v.SelectGameDataPathButton)
                    .DisposeWith(disposables);
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

                // Bind Interactions
                this.BindInteraction(
                        this.ViewModel,
                        vm => vm.ShowSelectMapGeometryDialog,
                        ShowSelectMapGeometryDialogAsync
                    )
                    .DisposeWith(disposables);
                this.BindInteraction(
                        this.ViewModel,
                        vm => vm.ShowSelectMaterialsBinDialog,
                        ShowSelectMaterialsBinDialogAsync
                    )
                    .DisposeWith(disposables);
                this.BindInteraction(
                        this.ViewModel,
                        vm => vm.ShowSelectGameDataDialog,
                        ShowSelectGameDataPathDialogAsync
                    )
                    .DisposeWith(disposables);
                this.BindInteraction(
                        this.ViewModel,
                        vm => vm.ShowSelectExportedGltfDialog,
                        ShowSelectExportedGltfDialogAsync
                    )
                    .DisposeWith(disposables);
                this.BindInteraction(this.ViewModel, vm => vm.ShowSaveGltfDialog, ShowSaveGltfDialogAsync)
                    .DisposeWith(disposables);
            });
        }

        private async Task ShowSelectMapGeometryDialogAsync(InteractionContext<Unit, string> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "Select a Map Geometry (.mapgeo) file",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Map Geometry files",
                            Extensions = new() { "mapgeo" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files?.FirstOrDefault());
        }

        private async Task ShowSelectMaterialsBinDialogAsync(InteractionContext<Unit, string> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "Select a Materials (.bin) file",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Materials Bin files",
                            Extensions = new() { "bin" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files?.FirstOrDefault());
        }

        private async Task ShowSelectGameDataPathDialogAsync(InteractionContext<Unit, string> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            OpenFolderDialog dialog = new() { Title = "Select a Game Data path" };

            string path = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(path);
        }

        private async Task ShowSelectExportedGltfDialogAsync(InteractionContext<string, string> interaction)
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

        private IObservable<Unit> ShowSaveGltfDialogAsync(InteractionContext<string, Unit> interaction)
        {
            return Observable
                .Start(() => interaction.Input, RxApp.MainThreadScheduler)
                .SelectMany(path =>
                {
                    if (string.IsNullOrEmpty(path))
                        ThrowHelper.ThrowInvalidOperationException($"{nameof(path)} must be set");

                    TaskDialogProgressState progressState =
                        TaskDialogProgressState.Normal | TaskDialogProgressState.Indeterminate;

                    EnvironmentAsset mapGeometry = this.ViewModel.MapGeometry;
                    BinTree materialsBin = this.ViewModel.MaterialsBin;

                    MapGeometryGltfConversionContext conversionContext =
                        new(
                            MetaEnvironment.Create(
                                Assembly.Load("LeagueToolkit.Meta.Classes").GetExportedTypes().Where(x => x.IsClass)
                            ),
                            new()
                            {
                                FlipAcrossX = this.ViewModel.FlipAcrossX,
                                GameDataPath = this.ViewModel.GameDataPath,
                                LayerGroupingPolicy = this.ViewModel.SelectedLayerGroupingPolicy,
                                TextureQuality = this.ViewModel.SelectedTextureQuality
                            }
                        );

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

                            ModelRoot gltfAsset = mapGeometry.ToGltf(materialsBin, conversionContext);

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
                    return dialog.ShowAsync(true);
                })
                .Do(_ => interaction.SetOutput(new()))
                .Select(_ => Unit.Default);
        }
    }
}
