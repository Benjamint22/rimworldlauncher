using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Views.Main.Edit;
using RimWorldLauncher.Views.Startup;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WinMain
    {
        private BoundProfile _selectedBoundProfile;

        public WinMain()
        {
            InitializeComponent();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinProfileEdit();
            if (editWindow.ShowDialog() ?? false) App.Profiles.LoadProfiles();
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button)?.DataContext as BoundProfile;
            Debug.Assert(profile != null, nameof(profile) + " != null");
            if (MessageBox.Show(
                    $"Are you sure you want to delete \"{profile.DisplayName}\" and all its saves?\nThis cannot be undone.",
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes) return;
            profile.Delete();
            App.Profiles.LoadProfiles();
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button)?.DataContext as BoundProfile;
            var editWindow = new WinProfileEdit
            {
                BoundProfile = profile
            };
            editWindow.ShowDialog();
        }

        private void BtnModpacks_OnClick(object sender, RoutedEventArgs e)
        {
            new WinModpacks().ShowDialog();
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            _selectedBoundProfile?.StartGame();
        }

        private void LvProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedBoundProfile = (sender as ListView)?.SelectedItem as BoundProfile;
            BtnPlay.IsEnabled = _selectedBoundProfile != null;
        }

        private void SettingsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new WinSettings().ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LvProfiles.ItemsSource = App.Profiles.ObservableProfilesList;
        }
    }
}