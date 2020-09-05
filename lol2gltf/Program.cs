using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CommandLine;
using Fantome.Libraries.League.Helpers.Structures;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SimpleSkinFile;
using Fantome.Libraries.League.IO.SkeletonFile;
using Fantome.Libraries.League.IO.StaticObjectFile;
using Fantome.Libraries.League.IO.WGT;
using ImageMagick;
using SharpGLTF.Schema2;
using LeagueAnimation = Fantome.Libraries.League.IO.AnimationFile.Animation;

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
                CreateSimpleSkinFromLegacyOptions,
                ConvertMapGeometryToGltfOptions>(args)
                .MapResult(
                    (SimpleSkinToGltfOptions opts) => ConvertSimpleSkin(opts),
                    (SkinnedModelToGltfOptions opts) => ConvertSkinnedModel(opts),
                    (DumpSimpleSkinInfoOptions opts) => DumpSimpleSkinInfo(opts),
                    (CreateSimpleSkinFromLegacyOptions opts) => CreateSimpleSkinFromLegacy(opts),
                    (ConvertMapGeometryToGltfOptions opts) => ConvertMapGeometryToGltf(opts),
                    errors => 1
                );
        }

        // ------------- COMMANDS ------------- \\

        private static int ConvertSimpleSkin(SimpleSkinToGltfOptions opts)
        {
            try
            {
                SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
                var materialTextureMap = CreateMaterialTextureMap(opts.TextureMaterialNames, opts.MaterialTexturePaths);
                var gltf = simpleSkin.ToGltf(materialTextureMap);

                gltf.Save(opts.OutputPath);
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
                SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
                Skeleton skeleton = ReadSkeleton(opts.SkeletonPath);
                var materialTextureMap = CreateMaterialTextureMap(opts.TextureMaterialNames, opts.MaterialTexturePaths);

                List<(string, LeagueAnimation)> animations = null;
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
                foreach(var animation in animations)
                {
                    if(!animation.Item2.IsCompatibleWithSkeleton(skeleton))
                    {
                        Console.WriteLine("Warning: Found an animation that's potentially not compatible with the provided skeleton - " + animation.Item1);
                    }
                }

                var gltf = simpleSkin.ToGltf(skeleton, materialTextureMap, animations);

                gltf.Save(opts.OutputPath);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to convert Simple Skin to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int CreateSimpleSkinFromLegacy(CreateSimpleSkinFromLegacyOptions opts)
        {
            try
            {
                StaticObject staticObject = StaticObject.ReadSCO(opts.StaticObjectPath);
                WGTFile weightFile = new WGTFile(opts.WeightFilePath);
                SimpleSkin simpleSkin = new SimpleSkin(staticObject, weightFile);

                simpleSkin.Write(opts.SimpleSkinPath);
            }
            catch(Exception exception)
            {
                Console.WriteLine("Failed to convert Simple Skin to glTF");
                Console.WriteLine(exception);
            }

            return 1;
        }

        private static int ConvertMapGeometryToGltf(ConvertMapGeometryToGltfOptions opts)
        {
            MapGeometry mapGeometry = ReadMapGeometry(opts.MapGeometryPath);
            ModelRoot gltf = mapGeometry.ToGLTF();

            gltf.Save(opts.OutputPath);

            return 1;
        }

        // ------------- BACKING FUNCTIONS ------------- \\

        private static SimpleSkin ReadSimpleSkin(string location)
        {
            try
            {
                return new SimpleSkin(location);
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
        private static List<(string, LeagueAnimation)> ReadAnimations(IEnumerable<string> animationPaths)
        {
            var animations = new List<(string, LeagueAnimation)>();

            foreach (string animationPath in animationPaths)
            {
                string animationName = Path.GetFileNameWithoutExtension(animationPath);
                LeagueAnimation animation = new LeagueAnimation(animationPath);

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
            catch(Exception exception)
            {
                throw new Exception("Error: Failed to read map geometry file", exception);
            }
        }

        private static Dictionary<string, MagickImage> CreateMaterialTextureMap(IEnumerable<string> materials, IEnumerable<string> textures)
        {
            var materialTextureMap = new Dictionary<string, MagickImage>();

            int materialCount = materials.Count();
            int texturesCount = textures.Count();
            if (materialCount != texturesCount)
            {
                Console.WriteLine("Warning: Detected mismatch of material and texture counts");
            }

            int i = 0;
            foreach (string material in materials)
            {
                if (i < texturesCount)
                {
                    MagickImage textureImage = null;
                    string texture = textures.ElementAt(i);

                    try { textureImage = new MagickImage(texture); }
                    catch (Exception exception)
                    {
                        throw new Exception("Error: Failed to create an Image object for texture: " + texture, exception);
                    }

                    materialTextureMap.Add(material, textureImage);
                }

                i++;
            }

            return materialTextureMap;
        }

        private static int DumpSimpleSkinInfo(DumpSimpleSkinInfoOptions opts)
        {
            SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            if (simpleSkin != null)
            {
                DumpSimpleSkinInfo(simpleSkin);
            }

            return 1;
        }
        private static void DumpSimpleSkinInfo(SimpleSkin simpleSkin)
        {
            Console.WriteLine("----------SIMPLE SKIN INFO----------");

            R3DBox boundingBox = simpleSkin.GetBoundingBox();
            Console.WriteLine("Bounding Box:");
            Console.WriteLine("\t Min: " + boundingBox.Min.ToString());
            Console.WriteLine("\t Max: " + boundingBox.Max.ToString());

            Console.WriteLine("Submesh Count: " + simpleSkin.Submeshes.Count);

            foreach (SimpleSkinSubmesh submesh in simpleSkin.Submeshes)
            {
                Console.WriteLine("--- SUBMESH ---");
                Console.WriteLine("Material: " + submesh.Name);
                Console.WriteLine("Vertex Count: " + submesh.Vertices.Count);
                Console.WriteLine("Index Count: " + submesh.Indices.Count);
                Console.WriteLine("Face Count: " + submesh.Indices.Count / 3);
                Console.WriteLine();
            }
        }
    }
}
