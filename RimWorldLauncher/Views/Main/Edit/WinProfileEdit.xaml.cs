using RimWorldLauncher.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RimWorldLauncher.Views.Main.Edit
{
    /// <summary>
    /// Interaction logic for WinModEdit.xaml
    /// </summary>
    public partial class WinProfileEdit : Window
    {
        public Profile Profile { get; set; }

        public WinProfileEdit()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CbModpack.ItemsSource = App.Modpacks.List;
            if (Profile == null)
            {
                BtnSave.Content = "Create";
                Title = "Create profile";
                CbModpack.SelectedItem = App.Modpacks.List.FirstOrDefault((modpack) => modpack.Identifier == "vanilla");
            }
            else
            {
                TxtName.Text = Profile.DisplayName;
                CbModpack.SelectedItem = Profile.Modpack;
                Title = $"Editing {Profile.DisplayName}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                App.ShowError("\"Name\" cannot be empty.");
                return;
            }
            if (Profile == null)
            {
                Profile = new Profile(
                    TxtName.Text,
                    CbModpack.SelectedItem as Modpack,
                    TxtName.Text
                );
            }
            else
            {
                Profile.DisplayName = TxtName.Text;
                Profile.Modpack = CbModpack.SelectedItem as Modpack;
                Profile.Save();
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
