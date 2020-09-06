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

        [Option("material-names", Required = false,
            HelpText =
            "Names of the materials you want to assign a texture to using the texture-paths argument\n" +
            "Textures will be applies to materials in sequential order specified by this argument")]
        public IEnumerable<string> TextureMaterialNames { get; set; }

        [Option("texture-paths", Required = false, HelpText = "Paths to the textures used by the material-names argument")]
        public IEnumerable<string> MaterialTexturePaths { get; set; }

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
        public IEnumerable<string> TextureMaterialNames { get; set; }
        public IEnumerable<string> MaterialTexturePaths { get; set; }
        public string OutputPath { get; set; }
    }

    [Verb("skn2gltf-rigged", HelpText = "Converts an SKN (Simple Skin) file with a SKL (Skeleton) file to a glTF with optional textures and animations")]
    public class SkinnedModelToGltfOptions : IBaseSimpleSkinToGltfOptions
    {
        public string SimpleSkinPath { get; set; }
        public IEnumerable<string> TextureMaterialNames { get; set; }
        public IEnumerable<string> MaterialTexturePaths { get; set; }
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

    [Verb("convert-map-geometry", HelpText = "Convertes a Map Geometry (mapgeo) to a glTF")]
    public class ConvertMapGeometryToGltfOptions : IBaseMapGeometryToGltfOptions
    {
        public string MapGeometryPath { get; set; }
        public string OutputPath { get; set; }
    }
}
