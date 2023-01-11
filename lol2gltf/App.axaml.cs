using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using lol2gltf.ViewModels;
using lol2gltf.Views;
using Microsoft.Extensions.Logging;
using Splat;
using System;

namespace lol2gltf
{
    public partial class App : Application
    {
        public static MainWindowViewModel MainWindow => Locator.Current.GetService<MainWindowViewModel>()!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            SplatRegistrations.Register<MainWindowViewModel>();
            SplatRegistrations.SetupIOC();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            GC.KeepAlive(typeof(DialogService));

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow { DataContext = MainWindow, };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
