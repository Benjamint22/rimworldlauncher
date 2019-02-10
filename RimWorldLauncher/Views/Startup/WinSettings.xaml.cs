using System.Windows;
using System.Windows.Forms;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Services;

namespace RimWorldLauncher.Views.Startup
{
    /// <summary>
    ///     Interaction logic for WinSettings.xaml
    /// </summary>
    public partial class WinSettings
    {
        private static readonly FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();

        public WinSettings()
        {
            InitializeComponent();
        }

        private void WinSettings_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtGameFolder.Text = App.Config.FetchGameFolder()?.FullName ?? "";
            TxtDataFolder.Text = App.Config.FetchDataFolder()?.FullName ?? "";
        }

        private void BrowseGameFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser.Description = $@"The folder containing {Properties.Resources.LauncherName}";
            FolderBrowser.SelectedPath = TxtGameFolder.Text;
            if (FolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TxtGameFolder.Text = FolderBrowser.SelectedPath;
        }

        private void BrowseDataFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser.Description = $@"The folder containing the {Properties.Resources.SavesFolderName} folder.";
            FolderBrowser.SelectedPath = TxtDataFolder.Text;
            if (FolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TxtDataFolder.Text = FolderBrowser.SelectedPath;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var gameDirectory = new GameDirectory(TxtGameFolder.Text);
                App.Config.UpdateGameFolder(gameDirectory);
            }
            catch (InvalidConfigDirectoryException)
            {
                App.ShowError(
                    $"The game folder is not valid.\nIt must be the folder containing {Properties.Resources.LauncherName}.");
                return;
            }

            try
            {
                var dataDirectory = new DataDirectory(TxtDataFolder.Text);
                App.Config.UpdateDataFolder(dataDirectory);
            }
            catch (InvalidConfigDirectoryException)
            {
                App.ShowError(
                    $"The data folder is not valid.\nIt must be the folder containing {Properties.Resources.SavesFolderName} folder.");
                return;
            }
            DialogResult = true;
        }

        private void BtnResetGameFolder_OnClick(object sender, RoutedEventArgs e)
        {
            TxtGameFolder.Text = App.Config.FetchGameFolder().FullName;
        }

        private void BtnResetDataFolder_OnClick(object sender, RoutedEventArgs e)
        {
            TxtDataFolder.Text = App.Config.FetchDataFolder().FullName;
        }
    }
}