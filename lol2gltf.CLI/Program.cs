using BCnEncoder.Shared;
using CommandLine;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.PropertyBin;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta;
using LeagueToolkit.Toolkit;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using LeagueTexture = LeagueToolkit.Core.Renderer.Texture;

namespace lol2gltf.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine.Parser.Default
                .ParseArguments<SkinnedMeshToGltfOptions, MapGeometryToGltfOptions>(args)
                .MapResult(
                    (SkinnedMeshToGltfOptions opts) => ConvertSkinnedMeshToGltf(opts),
                    (MapGeometryToGltfOptions opts) => ConvertMapGeometryToGltf(opts),
                    HandleErrors
                );
        }

        private static int HandleErrors(IEnumerable<Error> errors)
        {
            return -1;
        }

        private static int ConvertSkinnedMeshToGltf(SkinnedMeshToGltfOptions options)
        {
            if (options.MaterialNames.Count() != options.TexturePaths.Count())
                throw new InvalidOperationException("Material name count and Animation path count must be equal");

            // Convert textures to png
            IEnumerable<(string, Stream)> textures = options.MaterialNames
                .Zip(options.TexturePaths)
                .Select(x =>
                {
                    using FileStream textureFileStream = File.OpenRead(x.Second);
                    LeagueTexture texture = LeagueTexture.Load(textureFileStream);

                    ReadOnlyMemory2D<ColorRgba32> mipMap = texture.Mips[0];
                    using ImageSharpImage image = mipMap.ToImage();

                    MemoryStream imageStream = new();
                    image.SaveAsPng(imageStream);
                    imageStream.Seek(0, SeekOrigin.Begin);

                    return (x.First, (Stream)imageStream);
                });

            IEnumerable<(string, IAnimationAsset)> animations = !string.IsNullOrEmpty(options.AnimationsPath)
                ? LoadAnimations(options.AnimationsPath)
                : Enumerable.Empty<(string, IAnimationAsset)>();

            using FileStream skeletonStream = File.OpenRead(options.SkeletonPath);

            SkinnedMesh simpleSkin = SkinnedMesh.ReadFromSimpleSkin(options.SimpleSkinPath);
            RigResource skeleton = new(skeletonStream);

            simpleSkin.ToGltf(skeleton, textures, animations).Save(options.GltfPath);

            return 1;
        }

        private static int ConvertMapGeometryToGltf(MapGeometryToGltfOptions options)
        {
            MapGeometryGltfConversionContext conversionContext =
                new(
                    MetaEnvironment.Create(
                        Assembly.Load("LeagueToolkit.Meta.Classes").GetExportedTypes().Where(x => x.IsClass)
                    ),
                    new()
                    {
                        FlipAcrossX = options.FlipAcrossX,
                        GameDataPath = options.GameDataPath,
                        LayerGroupingPolicy = options.LayerGroupingPolicy,
                        TextureQuality = options.TextureQuality
                    }
                );

            using MapGeometry mapGeometry = new(options.MapGeometryPath);
            BinTree materialsBin = new(options.MaterialsBinPath);

            mapGeometry.ToGltf(materialsBin, conversionContext).Save(options.GltfPath);

            return 1;
        }

        private static IEnumerable<(string, IAnimationAsset)> LoadAnimations(string path) =>
            Directory
                .EnumerateFiles(path, "*.anm")
                .Select(animationPath =>
                {
                    using FileStream stream = File.OpenRead(animationPath);
                    return (Path.GetFileNameWithoutExtension(animationPath), AnimationAsset.Load(stream));
                });
    }
}
