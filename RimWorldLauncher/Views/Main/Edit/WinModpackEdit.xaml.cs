using System.Windows;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Mixins;

namespace RimWorldLauncher.Views.Main.Edit
{
    /// <summary>
    ///     Interaction logic for WinModEdit.xaml
    /// </summary>
    public partial class WinModpackEdit
    {
        public WinModpackEdit()
        {
            InitializeComponent();
        }

        public BoundModList BoundModList { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (BoundModList == null)
            {
                BtnSave.Content = "Create";
                Title = "Create modpack";
            }
            else
            {
                TxtName.Text = BoundModList.DisplayName;
                Title = $"Editing {BoundModList.DisplayName}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                App.ShowError("\"Name\" cannot be empty.");
                return;
            }

            if (BoundModList == null)
            {
                BoundModList = new BoundModList(
                    TxtName.Text,
                    TxtName.Text
                );
            }
            else
            {
                BoundModList.DisplayName = TxtName.Text;
                BoundModList.Save();
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}