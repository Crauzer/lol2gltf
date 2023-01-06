using LeagueToolkit.Core.Mesh;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class SimpleSkinSubmeshViewModel : PropertyNotifier
    {
        public string Name { get; private set; }
        public int VertexCount { get; private set; }
        public int FaceCount { get; private set; }

        public FileSelectionViewModel TextureFileSelection
        {
            get => this._textureFileSelection;
            set
            {
                this._textureFileSelection = value;
                NotifyPropertyChanged();
            }
        }
        public ReadOnlyMemory<byte> Texture { get; private set; }

        private FileSelectionViewModel _textureFileSelection;

        public SimpleSkinSubmeshViewModel(SkinnedMeshRange submesh)
        {
            this.Name = submesh.Material;
            this.VertexCount = submesh.VertexCount;
            this.FaceCount = submesh.VertexCount / 3;

            this.TextureFileSelection = new FileSelectionViewModel(
                "Select a DDS texture",
                OnSelectedTextureChanged,
                new CommonFileDialogFilter("DDS", "*.dds"));
        }

        private void OnSelectedTextureChanged(string filePath)
        {
            this.Texture = File.ReadAllBytes(filePath);
        }
    }
}
