using System.Windows;
using System.Windows.Controls;

namespace lol2gltf.UI.MVVM.ModelViews
{
    /// <summary>
    /// Interaction logic for FileSelectionControl.xaml
    /// </summary>
    public partial class FileSelectionControl : UserControl
    {
        public FileSelectionControl()
        {
            InitializeComponent();
        }

        public bool EnableClearButton
        {
            get { return (bool)GetValue(EnableClearButtonProperty); }
            set { SetValue(EnableClearButtonProperty, value); }
        }

        public static readonly DependencyProperty EnableClearButtonProperty = DependencyProperty.Register(
            "EnableClearButton", 
            typeof(bool), 
            typeof(FileSelectionControl), 
            new PropertyMetadata(default(bool)));
    }
}
