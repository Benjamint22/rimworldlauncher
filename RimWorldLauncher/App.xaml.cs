using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Services;
using RimWorldLauncher.Views.Main;
using RimWorldLauncher.Views.Startup;

namespace RimWorldLauncher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            Instance = this;
            Config = new ConfigurationService();
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        public static ModActivationService ActiveModsConfig { get; private set; }

        public static ConfigurationService Config { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public static App Instance { get; private set; }
        public static ModpacksService Modpacks { get; private set; }
        public static ModInstallationService Mods { get; private set; }
        public static ProfilesService Profiles { get; private set; }

        public static void ShowError(string message, string caption = "Error")
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void SwitchMainWindow(Window newWindow, Window parentWindow = null)
        {
            if (MainWindow != null) MainWindow.Closed -= MainWindow_Closed;
            Debug.Assert(newWindow != null, nameof(newWindow) + " != null");
            MainWindow = newWindow;
            if (parentWindow == null)
                MainWindow.Closed += MainWindow_Closed;
            else
                MainWindow.Closed += (sender, e) => SwitchMainWindow(parentWindow);
            MainWindow.Show();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (Config.FetchGameFolder() != null && Config.FetchDataFolder() != null)
            {
                OpenMainWindow();
            }
            else
            {
                MainWindow = new WinSettings();
                if (MainWindow.ShowDialog() ?? false) OpenMainWindow();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void OpenMainWindow()
        {
            Mods = new ModInstallationService();
            Modpacks = new ModpacksService();
            Modpacks.AddVanillaModpack();
            ActiveModsConfig = new ModActivationService();
            Profiles = new ProfilesService();
            if (!ProfilesService.IsSavesFolderSymlinked())
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
                    var oldModpack = new BoundModList("Old modpack", "old");
                    oldModpack.CopyTo(ActiveModsConfig.FetchActiveMods().ToArray(), 0);
                    Modpacks.LoadModpacks();
                    var profile = new BoundProfile("Old profile", oldModpack, "old");
                    profile.SavesFolder?.Delete();
                    Config.FetchDataFolder().GetDirectories().First(directory =>
                            directory.Name == RimWorldLauncher.Properties.Resources.SavesFolderName)
                        .MoveTo(Path.Combine(profile.ProfileFolder.FullName,
                            RimWorldLauncher.Properties.Resources.SavesFolderName));
                    Profiles.LoadProfiles();
                }
                else
                {
                    Current.Shutdown();
                    return;
                }
            }

            SwitchMainWindow(new WinMain());
        }
    }
}