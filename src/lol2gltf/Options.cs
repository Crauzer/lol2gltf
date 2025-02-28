using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using LeagueToolkit.IO.MapGeometryFile;

namespace lol2gltf
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

    [Verb("gltf2skn", HelpText = "Converts a glTF asset into a Skinned Mesh (.skn, .skl)")]
    public class GltfToSkinnedMeshOptions
    {
        [Option('g', "gltf", Required = true, HelpText = "glTF Asset (.glb, .gltf) path")]
        public string GltfPath { get; set; }

        [Option('m', "skn", Required = true, HelpText = "Simple Skin (.skn) path")]
        public string SimpleSkinPath { get; set; }

        [Option(
            's',
            "skl",
            Required = false,
            HelpText = "Skeleton (.skl) path (if not specified, will be saved under the same name as the Simple Skin)"
        )]
        public string SkeletonPath { get; set; }
    }

    [Verb("mapgeo2gltf", HelpText = "Converts Map Geometry into a glTF asset")]
    public class MapGeometryToGltfOptions
    {
        [Option('m', "mgeo", Required = true, HelpText = "Map Geometry (.mapgeo) path")]
        public string MapGeometryPath { get; set; }

        [Option('b', "matbin", Required = true, HelpText = "Materials Bin (.materials.bin) path")]
        public string MaterialsBinPath { get; set; }

        [Option(
            'g',
            "gltf",
            Required = true,
            HelpText = "Path of the generated glTF file (Use .glb extension for binary format)"
        )]
        public string GltfPath { get; set; }

        [Option('d', "gamedata", Required = false, HelpText = "Game Data path (required for bundling textures)")]
        public string GameDataPath { get; set; }

        [Option('x', "flipX", Required = false, Default = true, HelpText = "Whether to flip the map node's X axis")]
        public bool FlipAcrossX { get; set; }

        [Option(
            'l',
            "layerGroupingPolicy",
            Required = false,
            Default = MapGeometryGltfLayerGroupingPolicy.Default,
            HelpText = $"The layer grouping policy for meshes (use `Ignore` if you don't want to group meshes based on layers)"
        )]
        public MapGeometryGltfLayerGroupingPolicy LayerGroupingPolicy { get; set; }

        [Option(
            'q',
            "textureQuality",
            Required = false,
            Default = MapGeometryGltfTextureQuality.Low,
            HelpText = $"The quality of textures to bundle (Low = 4x, Medium = 2x)"
        )]
        public MapGeometryGltfTextureQuality TextureQuality { get; set; }
    }

    public class GltfToStaticMeshOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to the input GLTF file")]
        public string GltfPath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to the output SCB/SCO file")]
        public string OutputPath { get; set; }
    }
}
