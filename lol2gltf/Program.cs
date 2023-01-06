using CommandLine;
using LeagueToolkit.Core.Mesh;
using LeagueToolkit.IO.AnimationFile;
using LeagueToolkit.IO.MapGeometryFile;
using LeagueToolkit.IO.SkeletonFile;
using lol2gltf.Core.ConversionOptions;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using LeagueConverter = lol2gltf.Core.Converter;
using LeagueAnimation = LeagueToolkit.IO.AnimationFile.Animation;
using LeagueToolkit.Core.Primitives;
using LeagueToolkit.IO.SimpleSkinFile;

namespace lol2gltf
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                SimpleSkinToGltfOptions,
                SkinnedModelToGltfOptions,
                DumpSimpleSkinInfoOptions,
                ConvertMapGeometryToGltfOptions>(args)
                .MapResult(
                    (SimpleSkinToGltfOptions opts) => ConvertSimpleSkin(opts),
                    (SkinnedModelToGltfOptions opts) => ConvertSkinnedModel(opts),
                    (DumpSimpleSkinInfoOptions opts) => DumpSimpleSkinInfo(opts),
                    (ConvertMapGeometryToGltfOptions opts) => ConvertMapGeometryToGltf(opts),
                    errors => 1
                );
        }

        // ------------- COMMANDS ------------- \\

        private static int ConvertSimpleSkin(SimpleSkinToGltfOptions opts)
        {
            try
            {
                SimpleSkinToGltf simpleSkinToGltf = new()
                {
                    OutputPath = opts.OutputPath,
                    SimpleSkinPath = opts.SimpleSkinPath,
                    MaterialTextures = CreateMaterialTextureMap(opts.MaterialTextures)
                };

                LeagueConverter.ConvertSimpleSkin(simpleSkinToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Simple Skin to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertSkinnedModel(SkinnedModelToGltfOptions opts)
        {
            try
            {
                Skeleton skeleton = ReadSkeleton(opts.SkeletonPath);

                List<(string name, LeagueAnimation animation)> animations = null;
                if (!string.IsNullOrEmpty(opts.AnimationsFolder))
                {
                    string[] animationFiles = Directory.GetFiles(opts.AnimationsFolder, "*.anm");
                    animations = ReadAnimations(animationFiles);
                }
                else
                {
                    animations = ReadAnimations(opts.AnimationPaths);
                }

                // Check animation compatibility
                foreach (var animation in animations)
                {
                    if (!animation.Item2.IsCompatibleWithSkeleton(skeleton))
                    {
                        Console.WriteLine("Warning: Found an animation that's potentially not compatible with the provided skeleton - " + animation.Item1);
                    }
                }

                SkinnedModelToGltf skinnedModelToGltf = new()
                {
                    OutputPath = opts.OutputPath,
                    Animations = animations,
                    MaterialTextures = CreateMaterialTextureMap(opts.MaterialTextures),
                    SimpleSkinPath = opts.SimpleSkinPath,
                    SkeletonPath = opts.SkeletonPath
                };

                LeagueConverter.ConvertSkinnedModel(skinnedModelToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Skinned Model to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertMapGeometryToGltf(ConvertMapGeometryToGltfOptions opts)
        {
            try
            {
                ConvertMapGeometryToGltf convertMapGeometryToGltf = new ConvertMapGeometryToGltf()
                {
                    MapGeometryPath = opts.MapGeometryPath,
                    OutputPath = opts.OutputPath
                };

                LeagueConverter.ConvertMapGeometryToGltf(convertMapGeometryToGltf);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Map Geometry to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertGltfToSimpleSkin(ConvertGltfToSimpleSkinOptions opts)
        {
            ModelRoot gltf = ReadGltf(opts.GltfPath);
            (SkinnedMesh simpleSkin, Skeleton skeleton) = gltf.ToRiggedMesh();

            simpleSkin.WriteSimpleSkin(opts.SimpleSkinPath);
            skeleton.Write(opts.SkeletonPath);

            return 1;
        }

        // ------------- BACKING FUNCTIONS ------------- \\

        private static SkinnedMesh ReadSimpleSkin(string location)
        {
            try
            {
                return SkinnedMesh.ReadFromSimpleSkin(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read specified SKN file", exception);
            }
        }
        private static Skeleton ReadSkeleton(string location)
        {
            try
            {
                return new Skeleton(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read specified SKL file", exception);
            }
        }
        private static List<(string name, LeagueAnimation animation)> ReadAnimations(IEnumerable<string> animationPaths)
        {
            var animations = new List<(string name, LeagueAnimation animation)>();

            foreach (string animationPath in animationPaths)
            {
                string animationName = Path.GetFileNameWithoutExtension(animationPath);
                LeagueAnimation animation = new(animationPath);

                animations.Add((animationName, animation));
            }

            return animations;
        }
        private static MapGeometry ReadMapGeometry(string location)
        {
            try
            {
                return new MapGeometry(location);
            }
            catch (Exception exception)
            {
                throw new Exception("Error: Failed to read map geometry file", exception);
            }
        }
        private static ModelRoot ReadGltf(string location)
        {
            try
            {
                return ModelRoot.Load(Path.GetFullPath(location));
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to load glTF file", exception);
            }
        }

        private static Dictionary<string, ReadOnlyMemory<byte>> CreateMaterialTextureMap(IEnumerable<string> materialTextures)
        {
            var materialTextureMap = new Dictionary<string, ReadOnlyMemory<byte>>();

            foreach (string materialTexture in materialTextures)
            {
                if (!materialTexture.Contains(':'))
                {
                    throw new Exception("Material Texture does not contain a separator (:) - " + materialTexture);
                }

                string[] materialTextureSplit = materialTexture.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                string material = materialTextureSplit[0];
                string texturePath;
                if (materialTextureSplit.Length == 2) // Material : Relative path
                {
                    texturePath = materialTextureSplit[1];
                }
                else if (materialTextureSplit.Length == 3) // Material : Absolute path C:/.......
                {
                    texturePath = materialTextureSplit[1] + ':' + materialTextureSplit[2];
                }
                else
                {
                    throw new Exception("Invalid format for material texture: " + materialTexture);
                }

                ReadOnlyMemory<byte> textureImage = File.ReadAllBytes(texturePath);

                if (materialTextureMap.ContainsKey(material))
                {
                    throw new Exception($"Material <{material}> has already been added");
                }

                materialTextureMap.Add(material, textureImage);
            }

            return materialTextureMap;
        }

        private static int DumpSimpleSkinInfo(DumpSimpleSkinInfoOptions opts)
        {
            SkinnedMesh simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            if (simpleSkin != null)
            {
                DumpSimpleSkinInfo(simpleSkin);
            }

            return 1;
        }
        private static void DumpSimpleSkinInfo(SkinnedMesh simpleSkin)
        {
            Console.WriteLine("----------SIMPLE SKIN INFO----------");
            Console.WriteLine("Bounding Box:");
            Console.WriteLine("\t Min: " + simpleSkin.AABB.Min.ToString());
            Console.WriteLine("\t Max: " + simpleSkin.AABB.Max.ToString());

            Console.WriteLine("Submesh Count: " + simpleSkin.Ranges.Count);

            foreach (SkinnedMeshRange range in simpleSkin.Ranges)
            {
                Console.WriteLine("--- SUBMESH ---");
                Console.WriteLine("Material: " + range.Material);
                Console.WriteLine("Vertex Count: " + range.VertexCount);
                Console.WriteLine("Index Count: " + range.IndexCount);
                Console.WriteLine("Face Count: " + range.IndexCount/ 3);
                Console.WriteLine();
            }
        }
    }
}
