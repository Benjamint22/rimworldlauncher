using System.Windows;
using RimWorldLauncher.Classes;

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    ///     Interaction logic for PgMod.xaml
    /// </summary>
    public partial class PgMod
    {
        public PgMod()
        {
            InitializeComponent();
        }

        public Visibility Displayed
        {
            get => Contents.Visibility;
            set => Contents.Visibility = value;
        }

        public void SetMod(ModInfo mod)
        {
            ImgPreview.Source = mod.Preview;
            LblName.Content = mod.DisplayName;
            LblAuthor.Content = mod.Author ?? "Anonymous";
            LblVersion.Content = mod.ModVersion ?? "Unknown";
            LblGameVersion.Content = mod.TargetGameVersion ?? "Unknown";
            LblUrl.Content = mod.Url ?? "None";
            LblDescription.Text = mod.Description;
        }
    }
}