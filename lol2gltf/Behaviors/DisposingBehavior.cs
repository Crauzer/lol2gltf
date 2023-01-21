using Avalonia;
using Avalonia.Xaml.Interactivity;
using System.Reactive.Disposables;

namespace lol2gltf.Behaviors
{
    public abstract class DisposingBehavior<T> : Behavior<T> where T : class, IAvaloniaObject
    {
        private CompositeDisposable _disposables;

        protected override void OnAttached()
        {
            base.OnAttached();

            this._disposables?.Dispose();
            this._disposables = new CompositeDisposable();

            OnAttached(this._disposables);
        }

        protected abstract void OnAttached(CompositeDisposable disposables);

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this._disposables?.Dispose();
        }
    }
}
