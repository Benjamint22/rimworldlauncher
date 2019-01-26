using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        public static LauncherConfig Config { get; set; }
        public static InstalledMods Mods { get; set; }
        public static ModpacksReader Modpacks { get; set; }
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
