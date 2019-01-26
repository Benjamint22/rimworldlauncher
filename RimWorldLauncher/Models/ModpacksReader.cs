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
            var directory = new DirectoryInfo("modpacks");
            if (!directory.Exists)
            {
                directory.Create();
            }
            Directory = directory;
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
    }
}
