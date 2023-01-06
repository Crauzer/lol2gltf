using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.IO.SkeletonFile;
using lol2gltf.UI.MVVM.Commands;
using lol2gltf.UI.MVVM.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;

namespace lol2gltf.UI.Pages
{
    /// <summary>
    /// Interaction logic for ConvertModelPage.xaml
    /// </summary>
    public partial class ConvertModelToGltfPage : Page, INotifyPropertyChanged
    {
        public FileSelectionViewModel SimpleSkinFileSelection
        {
            get => this._simpleSkinFileSelection;
            set
            {
                this._simpleSkinFileSelection = value;
                NotifyPropertyChanged();
            }
        }
        public FileSelectionViewModel SkeletonFileSelection
        {
            get => this._skeletonFileSelection;
            set
            {
                this._skeletonFileSelection = value;
                NotifyPropertyChanged();
            }
        }

        public SimpleSkinViewModel SimpleSkinInfo
        {
            get => this._simpleSkinInfo;
            set
            {
                this._simpleSkinInfo = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(this.IsModelLoaded));
            }
        }
        public Skeleton Skeleton
        {
            get => this._skeleton;
            set
            {
                this._skeleton = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(this.IsSkeletonLoaded));
            }
        }
        public ObservableCollection<AnimationViewModel> Animations { get; private set; } = new ObservableCollection<AnimationViewModel>();

        public bool IsModelLoaded => this.SimpleSkinInfo is not null;
        public bool IsSkeletonLoaded => this.Skeleton is not null;
        public bool IsConverting
        {
            get => this._isConverting;
            set
            {
                this._isConverting = value;
                NotifyPropertyChanged();
            }
        }

        private FileSelectionViewModel _simpleSkinFileSelection;
        private FileSelectionViewModel _skeletonFileSelection;

        private SimpleSkinViewModel _simpleSkinInfo;
        private Skeleton _skeleton;

        private bool _isConverting;

        public ICommand LoadAnimationsCommand => new RelayCommand(LoadAnimations);
        public ICommand ConvertCommand => new RelayCommand(Convert);

        public event PropertyChangedEventHandler PropertyChanged;

        public ConvertModelToGltfPage()
        {
            InitializeComponent();

            this.DataContext = this;

            this.SimpleSkinFileSelection = new FileSelectionViewModel(
                "Select a Simple Skin (SKN) file",
                OnSimpleSkinSelectionChanged,
                new CommonFileDialogFilter("Simple Skin (SKN)", "*.skn"));

            this.SkeletonFileSelection = new FileSelectionViewModel(
                "Select a Skeleton (SKL) file",
                OnSkeletonSelectionChanged,
                new CommonFileDialogFilter("Skeleton (SKL)", "*.skl"));
        }

        public void OnSimpleSkinSelectionChanged(string filePath)
        {
            this.SimpleSkinInfo = new SimpleSkinViewModel(SkinnedMesh.ReadFromSimpleSkin(filePath));
        }
        public void OnSkeletonSelectionChanged(string filePath)
        {
            this.Skeleton = new Skeleton(filePath);
        }

        private void LoadAnimations(object o)
        {
            using CommonOpenFileDialog dialog = new CommonOpenFileDialog("Select the Animation files to laod");
            dialog.Multiselect = true;
            dialog.Filters.Add(new CommonFileDialogFilter("Animation (ANM)", "*.anm"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.Animations.Clear();

                foreach (string animationFilePath in dialog.FileNames)
                {
                    this.Animations.Add(new AnimationViewModel(animationFilePath));
                }
            }
        }

        private void Convert(object o)
        {
            using BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += ConvertWork;
            worker.RunWorkerCompleted += ConversionComplete;

            using CommonSaveFileDialog dialog = new CommonSaveFileDialog();
            dialog.Filters.Add(new CommonFileDialogFilter("glTF", "*.gltf"));
            dialog.Filters.Add(new CommonFileDialogFilter("glTF (Binary)", "*.glb"));

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                worker.RunWorkerAsync(dialog.FileName);
            }
        }

        private void ConversionComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsConverting = false;
        }

        private void ConvertWork(object sender, DoWorkEventArgs e)
        {
            this.IsConverting = true;

            // Create material texture map
            var materialTextureMap = new Dictionary<string, ReadOnlyMemory<byte>>();
            foreach (var submesh in this.SimpleSkinInfo.Submeshes)
                materialTextureMap.Add(submesh.Name, submesh.Texture);

            ModelRoot gltf = null;
            if (this.Skeleton is null)
            {
                gltf = this.SimpleSkinInfo.SimpleSkin.ToGltf(materialTextureMap);
            }
            else
            {
                // Create Animation list
                var animationList = new List<(string name, LeagueAnimation animation)>();
                foreach (var animation in this.Animations)
                {
                    animationList.Add(new(animation.Name, animation.Animation));
                }

                gltf = this.SimpleSkinInfo.SimpleSkin.ToGltf(this.Skeleton, materialTextureMap, animationList);
            }

            gltf?.Save((string)e.Argument);
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
