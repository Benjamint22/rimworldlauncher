using System.Windows;
using RimWorldLauncher.Models;

namespace RimWorldLauncher.Views.Main.Edit
{
    /// <summary>
    ///     Interaction logic for WinModEdit.xaml
    /// </summary>
    public partial class WinModpackEdit : Window
    {
        public WinModpackEdit()
        {
            InitializeComponent();
        }

        public Modpack Modpack { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Modpack == null)
            {
                BtnSave.Content = "Create";
                Title = "Create modpack";
            }
            else
            {
                TxtName.Text = Modpack.DisplayName;
                Title = $"Editing {Modpack.DisplayName}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text))
            {
                App.ShowError("\"Name\" cannot be empty.");
                return;
            }

            if (Modpack == null)
            {
                Modpack = new Modpack(
                    TxtName.Text,
                    TxtName.Text
                );
            }
            else
            {
                Modpack.DisplayName = TxtName.Text;
                Modpack.Save();
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}