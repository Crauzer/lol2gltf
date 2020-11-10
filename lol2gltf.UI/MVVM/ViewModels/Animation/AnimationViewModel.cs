using Fantome.Libraries.League.IO.AnimationFile;
using System.IO;

namespace lol2gltf.UI.MVVM.ViewModels
{
    public class AnimationViewModel : PropertyNotifier
    {
        private bool _need = true;
        public bool Need
        {
            get => this._need;
            set
            {
                this._need = value;
                NotifyPropertyChanged();
            }
        }
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
