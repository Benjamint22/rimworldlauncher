using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RimWorldLauncher.Models
{
    public class ModpacksReader
    {
        public ObservableCollection<Modpack> List { get; set; }

        public DirectoryInfo Directory { get; }

        public ModpacksReader()
        {
            var dataFolder = App.Config.ReadDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault((dir) => dir.Name == Properties.Resources.ModpacksFolderName)
                ?? dataFolder.CreateSubdirectory(Properties.Resources.ModpacksFolderName);
            Refresh();
        }

        public void Refresh()
        {
            if (List == null)
            {
                List = new ObservableCollection<Modpack>();
            }
            else
            {
                List.Clear();
            }
            foreach (var file in Directory.GetFiles())
            {
                List.Add(new Modpack(file));
            }
        }

        public void AddVanillaModpack()
        {
            if (!List.Any((modpack) => modpack.Identifier == "vanilla"))
            {
                new Modpack("Vanilla (default)", "vanilla");
                Refresh();
            }
        }
    }
}
