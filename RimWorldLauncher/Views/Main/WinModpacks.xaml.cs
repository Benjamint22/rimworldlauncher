using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using RimWorldLauncher.Models;
using RimWorldLauncher.Views.Main.Edit;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    ///     Interaction logic for PgMods.xaml
    /// </summary>
    public partial class WinModpacks : Window
    {
        private Modpack _currentModpack;
        private Point? _dragStartPoint;

        public WinModpacks()
        {
            InitializeComponent();
        }

        private void RefreshModpacksList()
        {
            var viewSource = new ListCollectionView(App.Modpacks.List);
            viewSource.Filter += ViewSource_Filter;
            LvModpacks.ItemsSource = viewSource;
        }

        private static bool ViewSource_Filter(object modpack)
        {
            return (modpack as Modpack)?.Identifier != "vanilla";
        }

        private void RefreshInstalledMods()
        {
            LvInstalledMods.ItemsSource = App.Mods.Mods;
        }

        private void SelectMod(ModInfo mod)
        {
            if (mod == null) return;
            (FrMod.Content as PgMod)?.SetMod(mod);
        }

        private void PgModpacks_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshModpacksList();
            RefreshInstalledMods();
        }

        private void ModpacksList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView)?.SelectedItem is Modpack selectedModpack)
            {
                LvActivatedMods.ItemsSource = _currentModpack = selectedModpack;
                LvActivatedMods.IsEnabled = true;
            }
            else
            {
                LvActivatedMods.ItemsSource = null;
                LvActivatedMods.IsEnabled = false;
            }
        }

        private void LvInstalledMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMod((sender as ListView)?.SelectedItem as ModInfo);
        }

        private void LvActivatedMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMod((sender as ListView)?.SelectedItem as ModInfo);
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button)?.DataContext as Modpack;
            var editWindow = new WinModpackEdit
            {
                Modpack = modpack
            };
            editWindow.ShowDialog();
        }

        private void BtnClone_OnClick(object sender, RoutedEventArgs e)
        {
            var modpackToClone = (sender as Button)?.DataContext as Modpack;
            var editWindow = new WinModpackEdit();
            if (!(editWindow.ShowDialog() ?? false)) return;
            var newModpack = editWindow.Modpack;
            newModpack.CopyTo((modpackToClone ?? throw new InvalidOperationException()).Select(mod => mod).ToArray(),
                0);
            App.Modpacks.Refresh();
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button)?.DataContext as Modpack;
            var profiles = App.Profiles.List.Where(profile => profile.Modpack == modpack);
            var message =
                $"Are you sure you want to delete \"{modpack?.DisplayName}\"?\nMods are not going to be uninstalled.\nThis cannot be undone.";
            var arrProfiles = profiles as Profile[] ?? profiles.ToArray();
            if (arrProfiles.Any())
                message += "\n\nThis modpack is used by the following profile(s):\n" +
                           string.Join("\n", arrProfiles.Select(profile => profile.DisplayName));
            if (MessageBox.Show(
                    message,
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes) return;
            modpack?.Delete();
            App.Modpacks.Refresh();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinModpackEdit();
            if (editWindow.ShowDialog() ?? false) App.Modpacks.Refresh();
        }

        private void LvInstalledMods_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void LvActivatedMods_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void LvInstalledMods_MouseMove(object sender, MouseEventArgs e)
        {
            LvMods_MouseMove(sender, e, Properties.Resources.DragModpackActivate, DragDropEffects.Link);
        }

        private void LvActivatedMods_MouseMove(object sender, MouseEventArgs e)
        {
            LvMods_MouseMove(sender, e, Properties.Resources.DragModpackReorder, DragDropEffects.Move);
        }

        private void LvMods_MouseMove(object sender, MouseEventArgs e, string dataObjectFormat, DragDropEffects effect)
        {
            if (_dragStartPoint == null || e.LeftButton != MouseButtonState.Pressed) return;
            var diff = e.GetPosition(null) - _dragStartPoint ?? throw new InvalidOperationException();
            if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
                return;
            _dragStartPoint = null;
            var sourceItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
            var sourceView = sourceItem.FindAncestor<ListView>();
            if (sourceItem == null || sourceView != sender) return;
            var mod = sourceView.ItemContainerGenerator.ItemFromContainer(sourceItem) as ModInfo;
            var dragData = new DataObject(dataObjectFormat, mod);
            DragDrop.DoDragDrop(sourceItem, dragData, effect);
        }

        private void LvInstalledMods_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(Properties.Resources.DragModpackReorder)) e.Effects = DragDropEffects.None;
        }

        private void LvActivatedMods_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(Properties.Resources.DragModpackActivate) &&
                !e.Data.GetDataPresent(Properties.Resources.DragModpackReorder))
                e.Effects = DragDropEffects.None;
        }

        private void LvInstalledMods_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(Properties.Resources.DragModpackReorder))
            {
                var mod = e.Data.GetData(Properties.Resources.DragModpackReorder) as ModInfo;
                if (_currentModpack.Contains(mod)) _currentModpack.Remove(mod);
            }
        }

        private void LvActivatedMods_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(Properties.Resources.DragModpackActivate))
            {
                var mod = e.Data.GetData(Properties.Resources.DragModpackActivate) as ModInfo;
                if (_currentModpack.Contains(mod))
                {
                    App.ShowError($"This mod is already part of {_currentModpack.DisplayName}.");
                    return;
                }

                var targetItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
                //var index = targetItem != null ? LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) : _currentModpack.Count;
                int index;
                if (targetItem != null)
                    index = LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) +
                            (e.GetPosition(targetItem).Y / targetItem.ActualHeight > 0.5 ? 1 : 0);
                else
                    index = _currentModpack.Count;
                _currentModpack.Insert(index, mod);
            }
            else if (e.Data.GetDataPresent(Properties.Resources.DragModpackReorder))
            {
                var mod = e.Data.GetData(Properties.Resources.DragModpackReorder) as ModInfo;
                var targetItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
                int index;
                if (targetItem != null)
                    index = LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) +
                            (e.GetPosition(targetItem).Y / targetItem.ActualHeight > 0.5 ? 1 : 0);
                else
                    index = _currentModpack.Count;
                var oldIndex = _currentModpack.IndexOf(mod);
                if (oldIndex < index) index--;
                _currentModpack.RemoveAt(oldIndex);
                _currentModpack.Insert(index, mod);
            }
        }

        private void LvActivatedMods_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is ListView list)) return;
            var selectedMods = list.SelectedItems;
            if (e.Key != Key.Delete) return;
            for (var i = selectedMods.Count - 1; i >= 0; i--)
                _currentModpack.Remove(selectedMods[i] as ModInfo);
        }
    }
}