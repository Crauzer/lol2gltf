using System;
using System.Windows;
using System.Windows.Controls;

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
