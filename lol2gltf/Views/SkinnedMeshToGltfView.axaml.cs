using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SkeletonFile;
using lol2gltf.ViewModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace lol2gltf.Views
{
    public partial class SkinnedMeshToGltfView : ReactiveUserControl<SkinnedMeshToGltfViewModel>
    {
        public SkinnedMeshToGltfView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                d.Invoke(this.ViewModel.ShowLoadSimpleSkinDialog.RegisterHandler(DoShowLoadSimpleSkinDialogAsync));
                d.Invoke(this.ViewModel.ShowLoadSkeletonDialog.RegisterHandler(DoShowLoadSkeletonDialogAsync));
                d.Invoke(this.ViewModel.ShowExportGltfDialog.RegisterHandler(DoShowExportGltfDialogAsync));
            });
        }

        private async Task DoShowLoadSimpleSkinDialogAsync(InteractionContext<Unit, string> interaction)
        {
            OpenFileDialog dialog =
                new()
                {
                    AllowMultiple = false,
                    Title = "aaa",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Simple Skin files (.skn)",
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
                    Title = "aaa",
                    Filters = new()
                    {
                        new()
                        {
                            Name = "Skeleton files (.skl)",
                            Extensions = new() { "skl" }
                        }
                    }
                };

            string[] files = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(files?.FirstOrDefault());
        }

        private async Task DoShowExportGltfDialogAsync(InteractionContext<string, string> interaction)
        {
            SaveFileDialog dialog =
                new()
                {
                    DefaultExtension = interaction.Input,
                    Filters = new()
                    {
                        new()
                        {
                            Name = $"glTF files ({interaction.Input})",
                            Extensions = new() { interaction.Input }
                        }
                    }
                };

            string file = await dialog.ShowAsync(
                ((ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow
            );

            interaction.SetOutput(file);
        }
    }
}
