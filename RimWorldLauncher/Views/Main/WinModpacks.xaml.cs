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
using RimWorldLauncher;
using RimWorldLauncher.Models;
using RimWorldLauncher.Views.Main.Edit;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    /// Interaction logic for PgMods.xaml
    /// </summary>
    public partial class WinModpacks : Window
    {
        private Point? _dragStartPoint;
        private Modpack _currentModpack;
        private ModInfo _currentMod;

        public WinModpacks()
        {
            InitializeComponent();
        }

        private void RefreshModpacksList()
        {
            LvModpacks.ItemsSource = App.Modpacks.List;
            LvModpacks.Items.Filter = (mod) => (mod as Modpack).Identifier != "vanilla";
        }

        private void RefreshInstalledMods()
        {
            LvInstalledMods.ItemsSource = App.Mods.Mods;
        }

        private void SelectMod(ModInfo mod)
        {
            if (mod != null)
            {
                _currentMod = mod;
                (FrMod.Content as PgMod).SetMod(mod);
            }
        }

        private void PgModpacks_OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshModpacksList();
            RefreshInstalledMods();
        }

        private void ModpacksList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedModpack = (sender as ListView).SelectedItem as Modpack;
            if (selectedModpack != null)
            {
                LvActivatedMods.ItemsSource = _currentModpack = selectedModpack;
            }
        }

        private void LvInstalledMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMod((sender as ListView).SelectedItem as ModInfo);
        }

        private void LvActivatedMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMod((sender as ListView).SelectedItem as ModInfo);
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button).DataContext as Modpack;
            var editWindow = new WinModpackEdit
            {
                Modpack = modpack
            };
            editWindow.ShowDialog();
        }

        private void BtnClone_OnClick(object sender, RoutedEventArgs e)
        {
            var modpackToClone = (sender as Button).DataContext as Modpack;
            var editWindow = new WinModpackEdit();
            if (editWindow.ShowDialog() ?? false)
            {
                var newModpack = editWindow.Modpack;
                newModpack.CopyTo(modpackToClone.Select(mod => mod).ToArray(), 0);
                App.Modpacks.List.Add(editWindow.Modpack);
                App.Modpacks.Refresh();
            }
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var modpack = (sender as Button).DataContext as Modpack;
            if (
                MessageBox.Show(
                    $"Are you sure you want to delete \"{modpack.DisplayName}\"?\nMods are not going to be uninstalled.\nThis cannot be undone.",
                    "Delete profile?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                ) == MessageBoxResult.Yes
            )
            {
                modpack.Delete();
                App.Modpacks.Refresh();
            }
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new WinModpackEdit();
            if (editWindow.ShowDialog() ?? false)
            {
                App.Modpacks.Refresh();
            }
        }

        private void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            LvMods_MouseMove(sender, e, "RimworldMod_Activate", DragDropEffects.Link);
        }

        private void LvActivatedMods_MouseMove(object sender, MouseEventArgs e)
        {
            LvMods_MouseMove(sender, e, "RimworldMod_Reorder", DragDropEffects.Move);
        }

        private void LvMods_MouseMove(object sender, MouseEventArgs e, string dataObjectFormat, DragDropEffects effect)
        {
            if (_dragStartPoint == null || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var diff = e.GetPosition(null) - _dragStartPoint ?? throw new InvalidOperationException();
            if (Math.Abs(diff.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) <= SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            _dragStartPoint = null;
            var sourceItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
            var sourceView = sourceItem.FindAncestor<ListView>();
            if (sourceItem == null || sourceView != sender)
            {
                return;
            }
            var mod = sourceView.ItemContainerGenerator.ItemFromContainer(sourceItem) as ModInfo;
            var dragData = new DataObject(dataObjectFormat, mod);
            DragDrop.DoDragDrop(sourceItem, dragData, effect);
        }

        private void LvInstalledMods_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("RimworldMod_Reorder"))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void LvActivatedMods_DragEnter(object sender, DragEventArgs e)
        {
            if ((!e.Data.GetDataPresent("RimworldMod_Activate") &&
                !e.Data.GetDataPresent("RimworldMod_Reorder")))
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void LvInstalledMods_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("RimworldMod_Reorder"))
            {
                var mod = e.Data.GetData("RimworldMod_Reorder") as ModInfo;
                if (_currentModpack.Contains(mod))
                {
                    _currentModpack.Remove(mod);
                }
            }
        }

        private void LvActivatedMods_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("RimworldMod_Activate"))
            {
                var mod = e.Data.GetData("RimworldMod_Activate") as ModInfo;
                if (_currentModpack.Contains(mod))
                {
                    App.ShowError($"This mod is already part of {_currentModpack.DisplayName}.");
                    return;
                }
                var targetItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
                //var index = targetItem != null ? LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) : _currentModpack.Count;
                int index;
                if (targetItem != null)
                {
                    index = LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) + 
                        (e.GetPosition(targetItem).Y / targetItem.ActualHeight > 0.5 ? 1 : 0);
                }
                else
                {
                    index = _currentModpack.Count;
                }
                _currentModpack.Insert(index, mod);
            }
            else if (e.Data.GetDataPresent("RimworldMod_Reorder"))
            {
                var mod = e.Data.GetData("RimworldMod_Reorder") as ModInfo;
                var targetItem = (e.OriginalSource as DependencyObject).FindAncestor<ListViewItem>();
                int index;
                if (targetItem != null)
                {
                    index = LvActivatedMods.ItemContainerGenerator.IndexFromContainer(targetItem) +
                        (e.GetPosition(targetItem).Y / targetItem.ActualHeight > 0.5 ? 1 : 0);
                }
                else
                {
                    index = _currentModpack.Count;
                }
                int oldIndex = _currentModpack.IndexOf(mod);
                if (oldIndex < index)
                {
                    index--;
                }
                _currentModpack.RemoveAt(oldIndex);
                _currentModpack.Insert(index, mod);
            }
        }

        private void LvActivatedMods_KeyDown(object sender, KeyEventArgs e)
        {
            var list = sender as ListView;
            var selectedMods = list.SelectedItems;
            if (selectedMods != null && e.Key == Key.Delete)
            {
                for (int i = selectedMods.Count - 1; i >= 0; i--)
                {
                    _currentModpack.Remove(selectedMods[i] as ModInfo);
                }
            }
        }
    }
}
