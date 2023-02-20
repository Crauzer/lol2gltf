using Microsoft.WindowsAPICodePack.Dialogs;

namespace lol2gltf.Utils;

public static class FileDialogUtils
{
    public static IEnumerable<CommonFileDialogFilter> CreateGltfFilters()
    {
        yield return new("glTF Binary", "glb");
        yield return new("glTF", "gltf");
    }
}
