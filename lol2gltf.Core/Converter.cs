using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SimpleSkinFile;
using Fantome.Libraries.League.IO.SkeletonFile;
using Fantome.Libraries.League.IO.StaticObjectFile;
using Fantome.Libraries.League.IO.WGT;
using ImageMagick;
using lol2gltf.Core.ConversionOptions;
using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.IO;
using LeagueAnimation = Fantome.Libraries.League.IO.AnimationFile.Animation;

namespace lol2gltf.Core
{
    public static class Converter
    {
        // ------------- COMMANDS ------------- \\

        public static void ConvertSimpleSkin(SimpleSkinToGltf opts)
        {
            SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            var gltf = simpleSkin.ToGltf(opts.MaterialTextures);

            gltf.Save(opts.OutputPath);
        }

        public static void ConvertSkinnedModel(SkinnedModelToGltf opts)
        {
            SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            Skeleton skeleton = ReadSkeleton(opts.SkeletonPath);

            var gltf = simpleSkin.ToGltf(skeleton, opts.MaterialTextures, opts.Animations);

            gltf.Save(opts.OutputPath);
        }

        public static void CreateSimpleSkinFromLegacy(CreateSimpleSkinFromLegacy opts)
        {
            StaticObject staticObject = StaticObject.ReadSCO(opts.StaticObjectPath);
            WGTFile weightFile = new WGTFile(opts.WeightFilePath);
            SimpleSkin simpleSkin = new SimpleSkin(staticObject, weightFile);

            simpleSkin.Write(opts.SimpleSkinPath);
        }

        public static void ConvertMapGeometryToGltf(ConvertMapGeometryToGltf opts)
        {
            MapGeometry mapGeometry = ReadMapGeometry(opts.MapGeometryPath);
            ModelRoot gltf = mapGeometry.ToGLTF();

            gltf.Save(opts.OutputPath);
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
    }
}
