using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Views.Main.Edit;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    ///     Interaction logic for PgMods.xaml
    /// </summary>
    public partial class WinModpacks
    {
        private BoundModList _currentBoundModList;
        private Point? _dragStartPoint;

        public WinModpacks()
        {
            InitializeComponent();
        }

        private void RefreshModpacksList()
        {
            var viewSource = new ListCollectionView(App.Modpacks.ObservableModpacksList);
            viewSource.Filter += ViewSource_Filter;
            LvModpacks.ItemsSource = viewSource;
        }

        private static bool ViewSource_Filter(object modpack)
        {
            return (modpack as BoundModList)?.Identifier != Properties.Resources.VanillaModpackName;
        }

        private void RefreshInstalledMods()
        {
            LvInstalledMods.ItemsSource = App.Mods.ModsList;
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
            if ((sender as ListView)?.SelectedItem is BoundModList selectedModpack)
            {
                LvActivatedMods.ItemsSource = _currentBoundModList = selectedModpack;
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
            var modpack = (sender as Button)?.DataContext as BoundModList;
            var editWindow = new WinModpackEdit
            {
                BoundModList = modpack
            };
            editWindow.ShowDialog();
        }

        private void BtnClone_OnClick(object sender, RoutedEventArgs e)
        {
            var modpackToClone = (sender as Button)?.DataContext as BoundModList;
            var editWindow = new WinModpackEdit();
            if (!(editWindow.ShowDialog() ?? false)) return;
            var newModpack = editWindow.BoundModList;
            newModpack.CopyTo((modpackToClone ?? throw new InvalidOperationException()).Select(mod => mod).ToArray(),
                0);
            App.Modpacks.LoadModpacks();
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button)?.DataContext as BoundModList;
            var profiles = App.Profiles.ObservableProfilesList.Where(profile => profile.BoundModList == modpack);
            var message =
                $"Are you sure you want to delete \"{modpack?.DisplayName}\"?\nMods are not going to be uninstalled.\nThis cannot be undone.";
            var arrProfiles = profiles as BoundProfile[] ?? profiles.ToArray();
            if (arrProfiles.Any())
                message += "\n\nThis modpack is used by the following profile(s):\n" +
                           string.Join("\n", arrProfiles.Select(profile => profile.DisplayName));
            if (MessageBox.Show(
                    message,
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) != MessageBoxResult.Yes) return;
            foreach (var profile in arrProfiles)
            {
                profile.BoundModList =
                    App.Modpacks.ObservableModpacksList.First(modlist => modlist.Identifier == Properties.Resources.VanillaModpackName);
            }
            modpack?.Delete();
            App.Modpacks.LoadModpacks();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinModpackEdit();
            if (editWindow.ShowDialog() ?? false) App.Modpacks.LoadModpacks();
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
                if (_currentBoundModList.Contains(mod)) _currentBoundModList.Remove(mod);
            }
        }

        private void LvActivatedMods_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(Properties.Resources.DragModpackActivate))
            {
                var mod = e.Data.GetData(Properties.Resources.DragModpackActivate) as ModInfo;
                if (_currentBoundModList.Contains(mod))
                {
                    App.ShowError($"This mod is already part of {_currentBoundModList.DisplayName}.");
                    return;
                }

                var targetItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
                //var index = targetItem != null ? LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) : _currentModpack.Count;
                int index;
                if (targetItem != null)
                    index = LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) +
                            (e.GetPosition(targetItem).Y / targetItem.ActualHeight > 0.5 ? 1 : 0);
                else
                    index = _currentBoundModList.Count;
                _currentBoundModList.Insert(index, mod);
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
                    index = _currentBoundModList.Count;
                var oldIndex = _currentBoundModList.IndexOf(mod);
                if (oldIndex < index) index--;
                _currentBoundModList.RemoveAt(oldIndex);
                _currentBoundModList.Insert(index, mod);
            }
        }

        private void LvActivatedMods_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is ListView list)) return;
            var selectedMods = list.SelectedItems;
            if (e.Key != Key.Delete) return;
            for (var i = selectedMods.Count - 1; i >= 0; i--)
                _currentBoundModList.Remove(selectedMods[i] as ModInfo);
        }
    }
}