using Fantome.Libraries.League.Helpers;
using lol2gltf.UI.MVVM;
using lol2gltf.UI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lol2gltf.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Uri _selectedCommandPageUri;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        private void SelectedPageSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedValue is Uri uri)
            {
                this._selectedCommandPageUri = uri;

                this.CommandFrame?.Navigate(uri);
            }
        }

        private void OnCommandFrameLoaded(object sender, RoutedEventArgs e)
        {
            this.CommandFrame?.Navigate(this._selectedCommandPageUri);
        }
    }
}
