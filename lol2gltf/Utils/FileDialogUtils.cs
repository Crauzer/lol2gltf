using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lol2gltf.Utils;

public static class FileDialogUtils
{
    public static IEnumerable<CommonFileDialogFilter> CreateGltfFilters()
    {
        yield return new("glTF Binary", "glb");
        yield return new("glTF", "gltf");
    }
}
