using CommunityToolkit.Diagnostics;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SkeletonFile;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SharpGLTF.Schema2;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lol2gltf.ViewModels
{
    public class MapGeometryToGltfViewModel : PageViewModel, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; } = new();

        [Reactive]
        public string MapGeometryPath { get; set; }

        public MapGeometry MapGeometry => this._mapGeometry?.Value;
        private ObservableAsPropertyHelper<MapGeometry> _mapGeometry;

        public ReactiveCommand<Unit, MapGeometry> LoadMapGeometryCommand { get; }
        public ReactiveCommand<string, Unit> ExportGltfCommand { get; }

        public Interaction<Unit, string> ShowSelectMapGeometryDialog { get; }
        public Interaction<string, string> ShowSelectExportedGltfDialog { get; }
        public Interaction<string, Unit> ShowSaveGltfDialog { get; }

        public string GlbExtension = "glb";
        public string GltfExtension = "gltf";

        public MapGeometryToGltfViewModel() : this("Map Geometry to glTf", "Convert .mapgeo files to glTF", Symbol.Map)
        { }

        public MapGeometryToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon)
        {
            Guard.IsNotNullOrEmpty(name, nameof(name));
            Guard.IsNotNullOrEmpty(tooltip, nameof(tooltip));

            this.LoadMapGeometryCommand = ReactiveCommand.CreateFromTask(LoadMapGeometryAsync);
            this.ExportGltfCommand = ReactiveCommand.CreateFromObservable<string, Unit>(
                ExportGltfAsync,
                outputScheduler: RxApp.MainThreadScheduler
            );

            this.ShowSelectMapGeometryDialog = new();
            this.ShowSelectExportedGltfDialog = new();
            this.ShowSaveGltfDialog = new();

            this.WhenActivated(disposables =>
            {
                this._mapGeometry = this.LoadMapGeometryCommand
                    .WhereNotNull()
                    .ToProperty(this, nameof(this.MapGeometry), scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);

                this.LoadMapGeometryCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.ExportGltfCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
            });
        }

        private async Task<MapGeometry> LoadMapGeometryAsync()
        {
            string path = await this.ShowSelectMapGeometryDialog.Handle(new());

            if (string.IsNullOrEmpty(path))
                return null;

            this.MapGeometryPath = path;

            return new(path);
        }

        public IObservable<Unit> ExportGltfAsync(string extension)
        {
            return Observable.StartAsync(async _ =>
            {
                Guard.IsNotNullOrEmpty(extension, nameof(extension));

                if (extension is not "glb" and not "gltf")
                    throw new ArgumentException($"Invalid extension: {extension}", nameof(extension));

                string path = await this.ShowSelectExportedGltfDialog.Handle(extension);
                if (string.IsNullOrEmpty(path))
                    return;

                await this.ShowSaveGltfDialog.Handle(path);
            });
        }
    }
}
