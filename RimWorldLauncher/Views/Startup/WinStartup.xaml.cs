using System.Windows;
using System.Windows.Forms;
using RimWorldLauncher.Models;

namespace RimWorldLauncher.Views.Startup
{
    /// <summary>
    ///     Interaction logic for WinStartup.xaml
    /// </summary>
    public partial class WinStartup : Window
    {
        private static readonly FolderBrowserDialog FolderBrowser = new FolderBrowserDialog();

        public WinStartup()
        {
            InitializeComponent();
        }

        private void WinStartup_OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtGameFolder.Text = App.Config.ReadGameFolder()?.FullName ?? "";
            TxtDataFolder.Text = App.Config.ReadDataFolder()?.FullName ?? "";
        }

        private void BrowseGameFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser.Description = $"The folder containing {Properties.Resources.LauncherName}";
            FolderBrowser.SelectedPath = TxtGameFolder.Text;
            if (FolderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TxtGameFolder.Text = FolderBrowser.SelectedPath;
        }

        private void BrowseDataFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowser.Description = $"The folder containing the {Properties.Resources.SavesFolderName} folder.";
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
            if (!App.Config.SetGameFolder(TxtGameFolder.Text))
            {
                App.ShowError(
                    $"The game folder is not valid.\nIt must be the folder containing {Properties.Resources.LauncherName}.");
                return;
            }

            if (!App.Config.SetDataFolder(TxtDataFolder.Text))
            {
                App.ShowError(
                    $"The data folder is not valid.\nIt must be the folder containing {Properties.Resources.SavesFolderName} folder.");
                return;
            }

            App.Config.Save();
            DialogResult = true;
        }
    }
}