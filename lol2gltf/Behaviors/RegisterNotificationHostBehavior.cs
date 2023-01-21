using Avalonia.Controls;
using lol2gltf.Helpers;
using System.Reactive.Disposables;

namespace lol2gltf.Behaviors
{
    public class RegisterNotificationHostBehavior : DisposingBehavior<Window>
    {
        public RegisterNotificationHostBehavior() { }

        protected override void OnAttached(CompositeDisposable disposables)
        {
            if (this.AssociatedObject is null)
                return;

            NotificationHelper.SetNotificationManager(this.AssociatedObject);
        }
    }
}
