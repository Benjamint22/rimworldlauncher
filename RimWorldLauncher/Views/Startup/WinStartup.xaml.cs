using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using RimWorldLauncher.Models;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace RimWorldLauncher.Views.Startup
{
    /// <summary>
    /// Interaction logic for WinStartup.xaml
    /// </summary>
    public partial class WinStartup : Window
    {
        private static FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

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
            folderBrowser.Description = $"The folder containing {Properties.Resources.LauncherName}";
            folderBrowser.SelectedPath = TxtGameFolder.Text;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtGameFolder.Text = folderBrowser.SelectedPath;
            }
        }

        private void BrowseDataFolder_Click(object sender, RoutedEventArgs e)
        {
            folderBrowser.Description = $"The folder containing the {Properties.Resources.SavesFolderName} folder.";
            folderBrowser.SelectedPath = TxtDataFolder.Text;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtDataFolder.Text = folderBrowser.SelectedPath;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (!App.Config.SetGameFolder(TxtGameFolder.Text))
            {
                App.ShowError($"The game folder is not valid.\nIt must be the folder containing {Properties.Resources.LauncherName}.");
                return;
            }
            if (!App.Config.SetDataFolder(TxtDataFolder.Text))
            {
                App.ShowError($"The data folder is not valid.\nIt must be the folder containing {Properties.Resources.SavesFolderName} folder.");
                return;
            }
            App.Config.Save();
            DialogResult = true;
        }
    }
}
