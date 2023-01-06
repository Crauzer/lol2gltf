using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.SimpleSkinFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class SimpleSkinViewModel
    {
        public int VertexCount { get; private set; }
        public int FaceCount { get; private set; }
        public ObservableCollection<SimpleSkinSubmeshViewModel> Submeshes { get; private set; } = new ObservableCollection<SimpleSkinSubmeshViewModel>();

        public SkinnedMesh SimpleSkin { get; }

        public SimpleSkinViewModel(SkinnedMesh simpleSkin)
        {
            this.SimpleSkin = simpleSkin;

            foreach (SkinnedMeshRange submesh in simpleSkin.Ranges)
            {
                this.Submeshes.Add(new SimpleSkinSubmeshViewModel(submesh));

                this.VertexCount += submesh.VertexCount;
                this.FaceCount += submesh.IndexCount / 3;
            }
        }
    }
}
