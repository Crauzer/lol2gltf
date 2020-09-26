using Fantome.Libraries.League.IO.AnimationFile;
using System.IO;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class AnimationViewModel
    {
        public string Name { get; set; }
        public float FPS => this.Animation.FPS;

        public Animation Animation { get; private set; }

        public AnimationViewModel(string animationFilePath)
        {
            this.Name = Path.GetFileNameWithoutExtension(animationFilePath);
            this.Animation = new Animation(animationFilePath);
        }
    }
}
