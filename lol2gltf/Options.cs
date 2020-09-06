using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace lol2gltf
{
    public interface IBaseSimpleSkinToGltfOptions
    {
        [Option('m', "skn", Required = true, HelpText = "Path to the SKN (Simple Skin) file")]
        public string SimpleSkinPath { get; set; }

        [Option('t', "material-textures", Required = false, HelpText = "Specifies what textures to assign to what materials. Example: Body:aatrox_base_tx_cm.dds")]
        public IEnumerable<string> MaterialTextures { get; set; }

        [Option('o', "output-path", Required = true,
            HelpText = 
            "Where the glTF file should be saved.\n" +
            "Use .gltf extension to save a JSON .glTF file with a resoruce file (.bin)\n" +
            "Use .glb extension to save a binary glTF gile")]
        public string OutputPath { get; set; }
    }

    public interface IBaseMapGeometryToGltfOptions
    {
        [Option('m', "mapgeo", Required = true, HelpText = "Path to a Map Geometry file")]
        public string MapGeometryPath { get; set; }
    
        [Option('o', "output", Required = true, HelpText =
            "Where the glTF file should be saved.\n" +
            "Use .gltf extension to save a JSON .glTF file with a resoruce file (.bin)\n" +
            "Use .glb extension to save a binary glTF gile")]
        public string OutputPath { get; set; }
    }

    [Verb("skn2gltf", HelpText = "Converts an SKN (Simple Skin) file to a glTF with optional textures")]
    public class SimpleSkinToGltfOptions : IBaseSimpleSkinToGltfOptions
    {
        public string SimpleSkinPath { get; set; }
        public IEnumerable<string> MaterialTextures { get; set; }
        public string OutputPath { get; set; }
    }

    [Verb("skn2gltf-rigged", HelpText = "Converts an SKN (Simple Skin) file with a SKL (Skeleton) file to a glTF with optional textures and animations")]
    public class SkinnedModelToGltfOptions : IBaseSimpleSkinToGltfOptions
    {
        public string SimpleSkinPath { get; set; }
        public IEnumerable<string> MaterialTextures { get; set; }
        public string OutputPath { get; set; }

        [Option('s', "skeleton", Required = true, HelpText = "Path to the SKL (Skeleton) file")]
        public string SkeletonPath { get; set; }

        [Option("animations-folder", Group = "animations", HelpText = "Path to the folder which contains the ANM (Animation) files used for conversion")]
        public string AnimationsFolder { get; set; }

        [Option("animations", Group = "animations", HelpText = "Paths to the ANM (Animation) files used for conversion")]
        public IEnumerable<string> AnimationPaths { get; set; }
    }

    [Verb("dump-skn", HelpText = "Displays all data about the provided SKN (Simple Skin)")]
    public class DumpSimpleSkinInfoOptions
    {
        [Option('s', "skn", Required = true, HelpText = "Path to the SKN (Simple Skin) file")]
        public string SimpleSkinPath { get; set; }
    }

    [Verb("legacy2skn", HelpText = "Creates a Simple Skin (SKN) from legacy SCO + WGT file combination")]
    public class CreateSimpleSkinFromLegacyOptions
    {
        [Option('s', "sco", Required = true, HelpText = "Path to the SCO file")]
        public string StaticObjectPath { get; set; }

        [Option('w', "wgt", Required = true, HelpText = "Path to the WGT file")]
        public string WeightFilePath { get; set; }

        [Option('o', "output-path", Required = true, HelpText = "Path where to save the created SKN")]
        public string SimpleSkinPath { get; set; }
    }

    [Verb("mapgeo2gltf", HelpText = "Convertes a Map Geometry (mapgeo) to a glTF")]
    public class ConvertMapGeometryToGltfOptions : IBaseMapGeometryToGltfOptions
    {
        public string MapGeometryPath { get; set; }
        public string OutputPath { get; set; }
    }

    [Verb("gltf2skn", HelpText = "Converts a glTF file to a League model (SKN + SKL)")]
    public class ConvertGltfToSimpleSkinOptions
    {
        [Option('g', "gltf", Required = true, HelpText = "Path to a glTF file")]
        public string GltfPath { get; set; }

        [Option('m', "skn", Required = true, HelpText = "Where the created SKN should be saved to")]
        public string SimpleSkinPath { get; set; }

        [Option('s', "skl", Required = true, HelpText = "Where the created SKL should be saved to")]
        public string SkeletonPath { get; set; }
    }
}
