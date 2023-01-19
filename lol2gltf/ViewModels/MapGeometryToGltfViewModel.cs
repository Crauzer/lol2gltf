using CommunityToolkit.Diagnostics;
using FluentAvalonia.UI.Controls;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.SkeletonFile;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SharpGLTF.Schema2;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
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

        [Reactive]
        public string MaterialsBinPath { get; set; }

        [Reactive]
        public string GameDataPath { get; set; }

        public MapGeometry MapGeometry => this._mapGeometry?.Value;
        private ObservableAsPropertyHelper<MapGeometry> _mapGeometry;

        public BinTree MaterialsBin => this._materialsBin?.Value;
        private ObservableAsPropertyHelper<BinTree> _materialsBin;

        public ReactiveCommand<Unit, string> SelectMapGeometryPathCommand { get; }
        public ReactiveCommand<Unit, string> SelectMaterialsBinPathCommand { get; }

        public ReactiveCommand<string, MapGeometry> LoadMapGeometryCommand { get; }
        public ReactiveCommand<string, BinTree> LoadMaterialsBinCommand { get; }
        public ReactiveCommand<Unit, Unit> SelectGameDataPathCommand { get; }
        public ReactiveCommand<string, Unit> ExportGltfCommand { get; }

        public Interaction<Unit, string> ShowSelectMapGeometryDialog { get; }
        public Interaction<Unit, string> ShowSelectMaterialsBinDialog { get; }
        public Interaction<Unit, string> ShowSelectGameDataDialog { get; }
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

            this.SelectMapGeometryPathCommand = ReactiveCommand.CreateFromTask(SelectMapGeometryPathAsync);
            this.SelectMaterialsBinPathCommand = ReactiveCommand.CreateFromTask(SelectMaterialsBinPathAsync);
            this.SelectGameDataPathCommand = ReactiveCommand.CreateFromTask(SelectGameDataPathAsync);

            this.LoadMapGeometryCommand = ReactiveCommand.Create<string, MapGeometry>(LoadMapGeometry);
            this.LoadMaterialsBinCommand = ReactiveCommand.Create<string, BinTree>(LoadMaterialsBin);

            this.ExportGltfCommand = ReactiveCommand.CreateFromObservable<string, Unit>(
                ExportGltfAsync,
                outputScheduler: RxApp.MainThreadScheduler
            );

            this.ShowSelectMapGeometryDialog = new();
            this.ShowSelectMaterialsBinDialog = new();
            this.ShowSelectGameDataDialog = new();
            this.ShowSelectExportedGltfDialog = new();
            this.ShowSaveGltfDialog = new();

            this.WhenActivated(disposables =>
            {
                // Handle path selection
                this.SelectMapGeometryPathCommand
                    .WhereNotNull()
                    .Subscribe(x => this.MapGeometryPath = x)
                    .DisposeWith(disposables);
                this.SelectMaterialsBinPathCommand
                    .WhereNotNull()
                    .Subscribe(x => this.MaterialsBinPath = x)
                    .DisposeWith(disposables);

                // Handle loading
                this._mapGeometry = this.LoadMapGeometryCommand
                    .ToProperty(this, nameof(this.MapGeometry), scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);
                this._materialsBin = this.LoadMaterialsBinCommand
                    .ToProperty(this, nameof(this.MaterialsBin), scheduler: RxApp.MainThreadScheduler)
                    .DisposeWith(disposables);

                // Re-load files when paths change
                this.WhenAnyValue(x => x.MapGeometryPath)
                    .InvokeCommand(this.LoadMapGeometryCommand)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.MaterialsBinPath)
                    .InvokeCommand(this.LoadMaterialsBinCommand)
                    .DisposeWith(disposables);

                // Reset materials bin path when map geometry path changes
                this.WhenAnyValue(x => x.MapGeometryPath)
                    .Subscribe(_ => this.MaterialsBinPath = null)
                    .DisposeWith(disposables);

                this.LoadMapGeometryCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.LoadMaterialsBinCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
                this.ExportGltfCommand.ThrownExceptions.Subscribe(ex => this.Log().Error(ex));
            });
        }

        private async Task<string> SelectMapGeometryPathAsync()
        {
            string path = await this.ShowSelectMapGeometryDialog.Handle(new());

            return path;
        }

        private async Task<string> SelectMaterialsBinPathAsync()
        {
            string path = await this.ShowSelectMaterialsBinDialog.Handle(new());

            return path;
        }

        private MapGeometry LoadMapGeometry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            return new(path);
        }

        private BinTree LoadMaterialsBin(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            return new(path);
        }

        private async Task SelectGameDataPathAsync()
        {
            string path = await this.ShowSelectGameDataDialog.Handle(new());

            if (string.IsNullOrEmpty(path))
                return;

            this.GameDataPath = path;
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
