using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using RimWorldLauncher.Views.Main.ModEdit;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    /// Interaction logic for PgMods.xaml
    /// </summary>
    public partial class PgModpacks : Page
    {
        private PgMod ModPage { get; set; }

        public PgModpacks()
        {
            InitializeComponent();
        }

        private void RefreshModpacksList()
        {
            LvModpacks.ItemsSource = App.Modpacks.List;
        }

        private void RefreshInstalledMods()
        {
            LvInstalledMods.ItemsSource = App.Mods.Mods;
        }

        private void PgModpacks_OnLoaded(object sender, RoutedEventArgs e)
        {
            ModPage = FrMod.Content as PgMod;
            RefreshModpacksList();
            RefreshInstalledMods();
        }

        private void ModpacksList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedModpack = LvModpacks.SelectedItem as Modpack;
            if (selectedModpack == null)
            {
                
            }
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button).DataContext as Modpack;
            var editWindow = new WinModpackEdit();
            editWindow.Modpack = modpack;
            if (editWindow.ShowDialog() ?? false)
            {
                App.Modpacks.Refresh();
                RefreshModpacksList();
            }
        }

        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button).DataContext as Modpack;
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button).DataContext as Modpack;
            modpack.Delete();
            App.Modpacks.Refresh();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
