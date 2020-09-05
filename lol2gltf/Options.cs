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
        [Option("simple-skin", Default = null, Required = true, HelpText = "Path to the SKN (Simple Skin) file")]
        public string SimpleSkinPath { get; set; }

        [Option("material-names", Default = null, Required = false,
            HelpText =
            "Names of the materials you want to assign a texture to using the texture-paths argument\n" +
            "Textures will be applies to materials in sequential order specified by this argument")]
        public IEnumerable<string> TextureMaterialNames { get; set; }

        [Option("texture-paths", Default = null, Required = false, HelpText = "Paths to the textures used by the material-names argument")]
        public IEnumerable<string> MaterialTexturePaths { get; set; }

        [Option('o', "output-path", Default = null, Required = true,
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

    [Verb("convert-simple-skin", HelpText = "Converts an SKN (Simple Skin) file to a glTF with optional textures")]
    public class SimpleSkinToGltfOptions : IBaseSimpleSkinToGltfOptions
    {
        public string SimpleSkinPath { get; set; }
        public IEnumerable<string> TextureMaterialNames { get; set; }
        public IEnumerable<string> MaterialTexturePaths { get; set; }
        public string OutputPath { get; set; }
    }

    [Verb("convert-skinned-model", HelpText = "Converts a SKN (Simple Skin) file with a SKL (Skeleton) file to a glTF with optional textures and animations")]
    public class SkinnedModelToGltfOptions : IBaseSimpleSkinToGltfOptions
    {
        public string SimpleSkinPath { get; set; }
        public IEnumerable<string> TextureMaterialNames { get; set; }
        public IEnumerable<string> MaterialTexturePaths { get; set; }
        public string OutputPath { get; set; }

        [Option("skeleton", Default = null, Required = true, HelpText = "Path to the SKL (Skeleton) file")]
        public string SkeletonPath { get; set; }

        [Option("animations-folder", Group = "animations", Default = null, HelpText = "Path to the folder which contains the ANM (Animation) files used for conversion")]
        public string AnimationsFolder { get; set; }

        [Option("animations", Group = "animations", Default = null, HelpText = "Paths to the ANM (Animation) files used for conversion")]
        public IEnumerable<string> AnimationPaths { get; set; }
    }

    [Verb("dump-simple-skin", HelpText = "Displays all data about the provided SKN (Simple Skin)")]
    public class DumpSimpleSkinInfoOptions
    {
        [Option('s', "simple-skin", Default = null, Required = true, HelpText = "Path to the SKN (Simple Skin) file")]
        public string SimpleSkinPath { get; set; }
    }

    [Verb("create-skn-from-legacy", HelpText = "Creates a Simple Skin (SKN) from legacy SCO + WGT file combination")]
    public class CreateSimpleSkinFromLegacyOptions
    {
        [Option("sco", Required = true, HelpText = "Path to the SCO file")]
        public string StaticObjectPath { get; set; }

        [Option("wgt", Required = true, HelpText = "Path to the WGT file")]
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
