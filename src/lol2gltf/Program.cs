﻿using BCnEncoder.Shared;
using CommandLine;
using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Animation;
using LeagueToolkit.Core.Environment;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SimpleSkinFile;
using LeagueToolkit.Meta;
using LeagueToolkit.Toolkit;
using SharpGLTF.Schema2;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using LeagueTexture = LeagueToolkit.Core.Renderer.Texture;
using System.Diagnostics.CodeAnalysis;
using LeagueToolkit.Toolkit.Gltf;

namespace lol2gltf;

class Program
{
    static void Main(string[] args)
    {
        CommandLine.Parser.Default
            .ParseArguments<SkinnedMeshToGltfOptions, MapGeometryToGltfOptions, GltfToSkinnedMeshOptions, GltfToStaticMeshOptions>(args)
            .MapResult(
                (SkinnedMeshToGltfOptions opts) => ConvertSkinnedMeshToGltf(opts),
                (MapGeometryToGltfOptions opts) => ConvertMapGeometryToGltf(opts),
                (GltfToSkinnedMeshOptions opts) => ConvertGltfToSkinnedMesh(opts),
                (GltfToStaticMeshOptions opts) => ConvertGltfToStaticMesh(opts),
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

    private static int ConvertGltfToSkinnedMesh(GltfToSkinnedMeshOptions options)
    {
        string skeletonPath = string.IsNullOrEmpty(options.SkeletonPath) switch
        {
            true => Path.ChangeExtension(options.SimpleSkinPath, "skl"),
            false => options.SkeletonPath
        };

        ModelRoot gltf = ModelRoot.Load(options.GltfPath);

        var (simpleSkin, rig) = gltf.ToRiggedMesh();

        using FileStream simpleSkinStream = File.Create(options.SimpleSkinPath);
        simpleSkin.WriteSimpleSkin(simpleSkinStream);

        using FileStream rigStream = File.Create(skeletonPath);
        rig.Write(rigStream);

        return 1;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetExportedTypes()")]
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

        using FileStream environmentAssetStream = File.OpenRead(options.MapGeometryPath);
        using FileStream materialsBinStream = File.OpenRead(options.MaterialsBinPath);

        using EnvironmentAsset mapGeometry = new(environmentAssetStream);
        BinTree materialsBin = new(materialsBinStream);

        mapGeometry.ToGltf(materialsBin, conversionContext).Save(options.GltfPath);

        return 1;
    }

    private static int ConvertGltfToStaticMesh(GltfToStaticMeshOptions options)
    {
        ModelRoot gltf = ModelRoot.Load(options.GltfPath);

        // Convert GLTF to StaticMesh
        StaticMesh staticMesh = gltf.ToStaticMesh();

        // Determine if we should save as SCB or SCO based on file extension
        string extension = Path.GetExtension(options.OutputPath).ToLowerInvariant();
        using FileStream outputStream = File.Create(options.OutputPath);

        if (extension == ".scb")
        {
            staticMesh.WriteBinary(outputStream);
        }
        else if (extension == ".sco")
        {
            staticMesh.WriteAscii(outputStream);
        }
        else
        {
            throw new InvalidOperationException("Output file must have .scb or .sco extension");
        }

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
