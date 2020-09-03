using System;
using System.Collections.Generic;
using System.Drawing;
using CommandLine;
using Fantome.Libraries.League.Helpers.Structures;
using Fantome.Libraries.League.IO.SimpleSkin;
using ImageMagick;

namespace lol2gltf
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<
                SimpleSkinOptions,
                SkinnedModelOptions,
                DumpSimpleSkinInfoOptions>(args)
                .MapResult(
                    (SimpleSkinOptions opts) => ConvertSimpleSkin(opts),
                    (SkinnedModelOptions opts) => ConvertSkinnedModel(opts),
                    (DumpSimpleSkinInfoOptions opts) => DumpSimpleSkinInfo(opts),
                    errors => HandleErrors(errors)
                );
        }

        private static int ConvertSimpleSkin(SimpleSkinOptions opts)
        {
            SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            if (simpleSkin is not null)
            {

            }

            return 1;
        }

        private static int ConvertSkinnedModel(SkinnedModelOptions opts)
        {

            return 1;
        }

        private static int HandleErrors(IEnumerable<Error> errors)
        {

            return 1;
        }

        private static SimpleSkin ReadSimpleSkin(string location)
        {
            try
            {
                return new SimpleSkin(location);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to read specified SKN file\n" + exception);
                return null;
            }
        }

        private static Dictionary<string, MagickImage> CreateMaterialTextureMap(IEnumerable<string> materials, IEnumerable<string> textures)
        {
            Dictionary<string, MagickImage> materialTextureMap = new();



            return materialTextureMap;
        }

        private static int DumpSimpleSkinInfo(DumpSimpleSkinInfoOptions opts)
        {
            SimpleSkin simpleSkin = ReadSimpleSkin(opts.SimpleSkinPath);
            if (simpleSkin is not null)
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

            foreach(SimpleSkinSubmesh submesh in simpleSkin.Submeshes)
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
