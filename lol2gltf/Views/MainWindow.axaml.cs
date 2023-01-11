using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Styling;
using System.Runtime.InteropServices;
using System;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Controls;

namespace lol2gltf.Views
{
    public partial class MainWindow : CoreWindow
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
