using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RimWorldLauncher.Models
{
    public class ProfilesReader
    {
        public ObservableCollection<Profile> List { get; set; }

        public DirectoryInfo Directory { get; }

        public bool IsSymlinked()
        {
            return App.Config.ReadDataFolder().GetDirectories().FirstOrDefault((directory) => directory.Name == Properties.Resources.SavesFolderName)?.IsSymlink() ?? true;
        }

        public ProfilesReader()
        {
            var dataFolder = App.Config.ReadDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault((dir) => dir.Name == Properties.Resources.ProfilesFolderName)
                ?? dataFolder.CreateSubdirectory(Properties.Resources.ProfilesFolderName);
            Refresh();
        }

        public void Refresh()
        {
            if (List == null)
            {
                List = new ObservableCollection<Profile>();
            }
            else
            {
                List.Clear();
            }
            foreach (var directory in Directory.GetDirectories())
            {
                List.Add(new Profile(directory));
            }
        }
    }
}
