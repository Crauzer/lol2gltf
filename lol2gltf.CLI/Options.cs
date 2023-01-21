using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace lol2gltf.CLI
{
    [Verb("skn2gltf", HelpText = "Converts a Skinned Mesh (skn, skl, anm) into a glTF asset")]
    public class SkinnedMeshToGltfOptions
    {
        [Option('m', "skn", Required = true, HelpText = "Simple Skin (.skn) path")]
        public string SimpleSkinPath { get; set; }

        [Option('s', "skl", Required = true, HelpText = "Skeleton (.skl) path")]
        public string SkeletonPath { get; set; }

        [Option(
            'g',
            "gltf",
            Required = true,
            HelpText = "Path of the generated glTF file (Use .glb extension for binary format)"
        )]
        public string GltfPath { get; set; }

        [Option('a', "anm", Required = false, HelpText = "Animations (.anm) folder path")]
        public string AnimationsPath { get; set; }

        [Option("materials", Required = false, HelpText = "Simple Skin material names for textures")]
        public IEnumerable<string> MaterialNames { get; set; }

        [Option("textures", Required = false, HelpText = "Texture paths for the specified materials")]
        public IEnumerable<string> TexturePaths { get; set; }
    }
}
