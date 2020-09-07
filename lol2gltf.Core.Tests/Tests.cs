using Fantome.Libraries.League.IO.AnimationFile;
using ImageMagick;
using lol2gltf.Core;
using lol2gltf.Core.ConversionOptions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace lol2gltf.Core.Tests
{
    public class Tests
    {
        private const string TESTFILES_SIMPLE_SKIN_DIR = "testfiles/simple-skin";

        [SetUp]
        public void Setup()
        {

        }

        [TestCase("aatrox")]
        [TestCase("ezreal_skin21")]
        public void TestConvertSimpleSkinToGltf(string modelName)
        {
            string simpleSkinDirectoryPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName);
            var materialTextureMap = CreateMaterialTextureMap(modelName);

            SimpleSkinToGltf simpleSkinToGltf = new SimpleSkinToGltf()
            {
                OutputPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName, modelName + ".glb"),
                MaterialTextures = materialTextureMap,
                SimpleSkinPath = Path.Join(simpleSkinDirectoryPath, modelName + ".skn")
            };

            Assert.DoesNotThrow(() => Converter.ConvertSimpleSkin(simpleSkinToGltf), "Failed to convert simple skin to gltf: {0}", modelName);

            Assert.Pass("Successfully converted model <{0}> to glTF", modelName);
        }

        [TestCase("aatrox")]
        [TestCase("ezreal_skin21")]
        public void TestConvertSkinnedModelToGltf(string modelName)
        {
            string modelDirectoryPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName);
            var materialTextureMap = CreateMaterialTextureMap(modelName);

            SkinnedModelToGltf skinnedModelToGltf = new SkinnedModelToGltf()
            {
                OutputPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName, modelName + ".glb"),
                MaterialTextures = materialTextureMap,
                SimpleSkinPath = Path.Join(modelDirectoryPath, modelName + ".skn"),
                Animations = CreateAnimations(modelName),
                SkeletonPath = Path.Join(modelDirectoryPath, modelName + ".skl")
            };

            Assert.DoesNotThrow(() => Converter.ConvertSkinnedModel(skinnedModelToGltf), "Failed to convert skinned model to gltf: {0}", modelName);

            Assert.Pass("Successfully converted skinned model <{0}> to glTF", modelName);
        }

        private Dictionary<string, MagickImage> CreateMaterialTextureMap(string modelName)
        {
            string simpleSkinDirectoryPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName);
            string materialTexturePathMapPath = Path.Join(simpleSkinDirectoryPath, modelName + ".materialmap.json");
            var materialTexturePathMap = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(materialTexturePathMapPath));

            // Create material texture map
            var materialTextureMap = new Dictionary<string, MagickImage>();
            foreach (var materialTexturePath in materialTexturePathMap)
            {
                MagickImage texture = null;
                string texturePath = Path.Join(simpleSkinDirectoryPath, materialTexturePath.Value);

                Assert.DoesNotThrow(() => texture = new MagickImage(texturePath), "Failed to load texture {0}", materialTexturePath.Value);

                materialTextureMap.Add(materialTexturePath.Key, texture);
            }

            return materialTextureMap;
        }
        private List<(string, Animation)> CreateAnimations(string modelName)
        {
            string modelDirectoryPath = Path.Join(TESTFILES_SIMPLE_SKIN_DIR, modelName);
            string animationsDirectoryPath = Path.Join(modelDirectoryPath, "animations");

            List<(string, Animation)> animations = new List<(string, Animation)>();
            foreach(string animationFile in Directory.EnumerateFiles(animationsDirectoryPath, ".anm"))
            {
                animations.Add((Path.GetFileNameWithoutExtension(animationFile), new Animation(animationFile)));
            }

            return animations;
        }
    }
}