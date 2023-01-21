using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Avalonia.Xaml.Interactivity;
using WalletWasabi.Fluent.Behaviors;
using lol2gltf.Helpers;

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
