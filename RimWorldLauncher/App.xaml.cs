using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RimWorldLauncher.Models;
using RimWorldLauncher.Views.Main;
using RimWorldLauncher.Views.Startup;

namespace RimWorldLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static LauncherConfig Config { get; private set; }
        public static InstalledMods Mods { get; private set; }
        public static ModpacksReader Modpacks { get; private set; }
        public static ProfilesReader Profiles { get; private set; }
        public static ActiveModsConfigReader ActiveModsConfig { get; private set; }
        public static App Instance { get; private set; }

        public static void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public App()
        {
            Instance = this;
            Config = new LauncherConfig();
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        public void SwitchMainWindow(Window newWindow, Window parentWindow = null)
        {
            if (MainWindow != null)
            {
                MainWindow.Closed -= MainWindow_Closed;
            }
            MainWindow = newWindow;
            if (parentWindow == null)
            {
                MainWindow.Closed += MainWindow_Closed;
            }
            else
            {
                MainWindow.Closed += (sender, e) => SwitchMainWindow(parentWindow);
            }
            MainWindow.Show();
        }

        private void OpenMainWindow()
        {
            Mods = new InstalledMods();
            Modpacks = new ModpacksReader();
            ActiveModsConfig = new ActiveModsConfigReader();
            Profiles = new ProfilesReader();
            if (!Profiles.IsSymlinked())
            {
                if (
                    MessageBox.Show(
                        "In order for the launcher to work, saves must be moved to a new profile.\nWould you like your saves to be moved to a new profile?",
                        "Move saves to new profile?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    ) == MessageBoxResult.Yes
                )
                {
                    var oldModpack = new Modpack("Old modpack", "old");
                    oldModpack.CopyTo(ActiveModsConfig.GetActiveMods().ToArray(), 0);
                    Modpacks.Refresh();
                    var profile = new Profile("Old profile", oldModpack, "old");
                    profile.SavesFolder?.Delete();
                    App.Config.ReadDataFolder().GetDirectories().First((directory) => directory.Name == RimWorldLauncher.Properties.Resources.SavesFolderName)
                        .MoveTo(Path.Combine(profile.ProfileFolder.FullName, RimWorldLauncher.Properties.Resources.SavesFolderName));
                }
                else
                {
                    Current.Shutdown();
                    return;
                }
            }
            SwitchMainWindow(new WinMain());
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (Config.ReadGameFolder() != null && Config.ReadDataFolder() != null)
            {
                OpenMainWindow();
            }
            else
            {
                MainWindow = new WinStartup();
                if (MainWindow.ShowDialog() ?? false)
                {
                    OpenMainWindow();
                }
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Shutdown();
        }
    }
}
