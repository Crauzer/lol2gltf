using Fantome.Libraries.League.IO.AnimationFile;
using ImageMagick;
using System.Collections.Generic;

namespace lol2gltf.Core.ConversionOptions
{
    public class SkinnedModelToGltf : IBaseSimpleSkinToGltf
    {
        public string SimpleSkinPath { get; set; }
        public Dictionary<string, MagickImage> MaterialTextures { get; set; }
        public string OutputPath { get; set; }
        public string SkeletonPath { get; set; }
        public List<(string, Animation)> Animations { get; set; }
    }
}
