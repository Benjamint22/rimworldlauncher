using RimWorldLauncher.Models;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace RimWorldLauncher.Views.Main
{
    /// <summary>
    /// Interaction logic for PgMod.xaml
    /// </summary>
    public partial class PgMod : Page
    {
        public Visibility Displayed
        {
            get => Contents.Visibility;
            set => Contents.Visibility = value;
        }

        public PgMod()
        {
            InitializeComponent();
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
