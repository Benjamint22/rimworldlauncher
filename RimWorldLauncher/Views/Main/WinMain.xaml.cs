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
using RimWorldLauncher.Models;
using RimWorldLauncher.Views.Main.Edit;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WinMain : Window
    {
        private Profile _selectedProfile = null;

        public WinMain()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LvProfiles.ItemsSource = App.Profiles.List;
        }

        private void LvProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProfile = (sender as ListView).SelectedItem as Profile;
            BtnPlay.IsEnabled = (_selectedProfile != null);
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinProfileEdit();
            if (editWindow.ShowDialog() ?? false)
            {
                App.Profiles.Refresh();
            }
        }

        private void BtnModpacks_OnClick(object sender, RoutedEventArgs e)
        {
            //App.Instance.SwitchMainWindow(new WinModpacks(), this);
            //Hide();
            (new WinModpacks()).ShowDialog();
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedProfile != null)
            {
                _selectedProfile.StartGame();
            }
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button).DataContext as Profile;
            var editWindow = new WinProfileEdit
            {
                Profile = profile
            };
            editWindow.ShowDialog();
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button).DataContext as Profile;
            if (
                MessageBox.Show(
                    $"Are you sure you want to delete \"{profile.DisplayName}\" and all its saves?\nThis cannot be undone.",
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) == MessageBoxResult.Yes
            )
            {
                profile.Delete();
                App.Profiles.Refresh();
            }
        }
    }
}
