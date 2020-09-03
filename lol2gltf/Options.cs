using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace lol2gltf
{
    [Verb("convert-simple-skin", HelpText = "Converts an SKN (Simple Skin) file to a glTF with optional textures")]
    public class SimpleSkinOptions
    {
        [Option("simple-skin", Default = null, Required = true, HelpText = "Path to the SKN (Simple Skin) file")]
        public string SimpleSkinPath { get; set; }

        [Option("material-names", Default = null, Required = false,
            HelpText =
            "Names of the materials you want to assign a texture to using the texture-paths argument\n" +
            "Textures will be applies to materials in sequential order specified by this argument")]
        public IEnumerable<string> TextureMaterialNames { get; set; }

        [Option("texture-paths", Default = null, Required = false, HelpText = "Paths to the textures used by the material-names argument")]
        public IEnumerable<string> MaterialTexturePaths { get; set; }
    }

    [Verb("convert-skinned-model", HelpText = "Converts a SKN (Simple Skin) file with a SKL (Skeleton) file to a glTF with optional textures and animations")]
    public class SkinnedModelOptions : SimpleSkinOptions
    {
        [Option("skeleton", Default = null, Required = true, HelpText = "Path to the SKL (Skeleton) file")]
        public string SkeletonPath { get; set; }

        [Option("animations-folder", Group = "animations", Default = null, HelpText = "Path to the folder which contains the ANM (Animation) files used for conversion")]
        public string AnimationsFolder { get; set; }

        [Option("animations", Group = "animations", Default = null, HelpText = "Paths to the ANM (Animation) files used for conversion")]
        public string AnimationPaths { get; set; }
    }
}
