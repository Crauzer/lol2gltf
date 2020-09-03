using System;
using System.Collections.Generic;
using CommandLine;

namespace lol2gltf
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SimpleSkinOptions, SkinnedModelOptions>(args)
                .MapResult(
                    (SimpleSkinOptions opts) => ConvertSimpleSkin(opts),
                    (SkinnedModelOptions opts) => ConvertSkinnedModel(opts),
                    errors => HandleErrors(errors)
                );
        }

        private static int ConvertSimpleSkin(SimpleSkinOptions opts)
        {

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
    }
}
