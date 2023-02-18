using KristofferStrube.Blazor.FileSystemAccess;

namespace lol2gltf.Data;

public enum GltfFormat
{
    Gltf,
    Glb
}

public static class GltfFileUtils
{
    public static FilePickerAcceptType[] CreateFilePickerAcceptTypes() =>
        new FilePickerAcceptType[]
        {
            new()
            {
                Description = "glTF Binary",
                Accept = new() { { "model/gltf-binary", new[] { ".glb" } } }
            }
        };
}
