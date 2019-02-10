using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RimWorldLauncher.Models;
using RimWorldLauncher.Views.Main.Edit;
using RimWorldLauncher.Views.Startup;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WinMain
    {
        private Profile _selectedProfile;

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
            _selectedProfile = (sender as ListView)?.SelectedItem as Profile;
            BtnPlay.IsEnabled = _selectedProfile != null;
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinProfileEdit();
            if (editWindow.ShowDialog() ?? false) App.Profiles.Refresh();
        }

        private void BtnModpacks_OnClick(object sender, RoutedEventArgs e)
        {
            new WinModpacks().ShowDialog();
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            _selectedProfile?.StartGame();
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button)?.DataContext as Profile;
            var editWindow = new WinProfileEdit
            {
                Profile = profile
            };
            editWindow.ShowDialog();
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button)?.DataContext as Profile;
            Debug.Assert(profile != null, nameof(profile) + " != null");
            if (MessageBox.Show(
                    $"Are you sure you want to delete \"{profile.DisplayName}\" and all its saves?\nThis cannot be undone.",
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes) return;
            profile.Delete();
            App.Profiles.Refresh();
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            (new WinSettings()).ShowDialog();
        }
    }
}