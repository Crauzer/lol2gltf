using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lol2gltf.ViewModels
{
    public class MapGeometryToGltfViewModel : PageViewModel
    {
        public MapGeometryToGltfViewModel() : this("Map Geometry to glTf", "", Symbol.Map) { }

        public MapGeometryToGltfViewModel(string name, string tooltip, Symbol icon) : base(name, tooltip, icon) { }
    }
}
