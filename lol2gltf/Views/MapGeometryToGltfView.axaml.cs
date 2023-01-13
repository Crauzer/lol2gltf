using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using CommunityToolkit.Diagnostics;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.IO.MapGeometryFile;
using lol2gltf.ViewModels;
using ReactiveUI;
using SharpGLTF.Schema2;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
                this.ViewModel.ShowSelectMapGeometryDialog
                    .RegisterHandler(ShowSelectMapGeometryDialogAsync)
                    .DisposeWith(disposables);
                this.ViewModel.ShowSelectExportedGltfDialog
                    .RegisterHandler(ShowSelectExportedGltfDialogAsync)
                    .DisposeWith(disposables);
                this.ViewModel.ShowExportGltfDialog.RegisterHandler(ShowExportGltfDialogAsync).DisposeWith(disposables);

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

        private async Task ShowSelectExportedGltfDialogAsync(InteractionContext<string, string> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            string extension = interaction.Input;
            if (extension is not "glb" and not "gltf")
                ThrowHelper.ThrowArgumentException($"Invalid extension: {extension}", nameof(extension));

            SaveFileDialog dialog =
                new()
                {
                    DefaultExtension = interaction.Input,
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

        private async Task ShowExportGltfDialogAsync(InteractionContext<string, Unit> interaction)
        {
            Guard.IsNotNull(interaction, nameof(interaction));

            string path = interaction.Input;
            if (string.IsNullOrEmpty(path))
                ThrowHelper.ThrowInvalidOperationException($"{nameof(path)} must be set");

            TaskDialog dialog =
                new()
                {
                    Title = "Exporting to glTF",
                    Content = "Exporting...",
                    ShowProgressBar = true,
                    XamlRoot = this.VisualRoot
                };

            MapGeometry mapGeometry = this.ViewModel.MapGeometry;

            dialog.Opened += async (dialog, e) =>
            {
                await Task.Run(() =>
                {
                    // Convert to glTF
                    dialog.SetProgressBarState(100, TaskDialogProgressState.Indeterminate);
                    Dispatcher.UIThread.Post(() =>
                    {
                        dialog.Content = "Converting to glTF...";
                    });
                    ModelRoot gltfAsset = mapGeometry.ToGLTF();

                    // Save glTF asset
                    dialog.SetProgressBarState(100, TaskDialogProgressState.Indeterminate);
                    Dispatcher.UIThread.Post(() =>
                    {
                        dialog.Content = "Exporting glTF...";
                    });
                    gltfAsset.Save(path);

                    // Finish
                    Dispatcher.UIThread.Post(() =>
                    {
                        dialog.Hide(TaskDialogStandardResult.OK);
                    });
                });
            };

            object result = await dialog.ShowAsync(true);
        }
    }
}
