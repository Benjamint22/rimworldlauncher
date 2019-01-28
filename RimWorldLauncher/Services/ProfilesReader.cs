using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RimWorldLauncher.Models;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ProfilesReader
    {
        public ProfilesReader()
        {
            var dataFolder = App.Config.ReadDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault(dir => dir.Name == Resources.ProfilesFolderName)
                        ?? dataFolder.CreateSubdirectory(Resources.ProfilesFolderName);
            Refresh();
        }

        public ObservableCollection<Profile> List { get; set; }

        public DirectoryInfo Directory { get; }

        public bool IsSymlinked()
        {
            return App.Config.ReadDataFolder().GetDirectories()
                       .FirstOrDefault(directory => directory.Name == Resources.SavesFolderName)?.IsSymlink() ?? true;
        }

        public void Refresh()
        {
            if (List == null)
                List = new ObservableCollection<Profile>();
            else
                List.Clear();
            foreach (var directory in Directory.GetDirectories()) List.Add(new Profile(directory));
        }
    }
}