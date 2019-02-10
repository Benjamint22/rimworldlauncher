using System.Linq;
using System.Windows;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Mixins;

namespace RimWorldLauncher.Views.Main.Edit
{
    /// <summary>
    ///     Interaction logic for WinModEdit.xaml
    /// </summary>
    public partial class WinProfileEdit
    {
        public WinProfileEdit()
        {
            InitializeComponent();
        }

        public BoundProfile BoundProfile { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CbModpack.ItemsSource = App.Modpacks.ObservableModpacksList;
            if (BoundProfile == null)
            {
                BtnSave.Content = "Create";
                Title = "Create profile";
                CbModpack.SelectedItem = App.Modpacks.ObservableModpacksList.FirstOrDefault(modpack => modpack.Identifier == "vanilla");
            }
            else
            {
                TxtName.Text = BoundProfile.DisplayName;
                CbModpack.SelectedItem = BoundProfile.BoundModList;
                Title = $"Editing {BoundProfile.DisplayName}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                App.ShowError("\"Name\" cannot be empty.");
                return;
            }

            if (BoundProfile == null)
            {
                BoundProfile = new BoundProfile(
                    TxtName.Text,
                    CbModpack.SelectedItem as BoundModList,
                    TxtName.Text
                );
            }
            else
            {
                BoundProfile.DisplayName = TxtName.Text;
                BoundProfile.BoundModList = CbModpack.SelectedItem as BoundModList;
                BoundProfile.Save();
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}