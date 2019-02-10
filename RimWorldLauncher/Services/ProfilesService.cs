using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ProfilesService
    {
        public ProfilesService()
        {
            var dataFolder = App.Config.FetchDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault(dir => dir.Name == Resources.ProfilesFolderName)
                        ?? dataFolder.CreateSubdirectory(Resources.ProfilesFolderName);
            LoadProfiles();
        }

        public ObservableCollection<BoundProfile> ObservableProfilesList { get; set; }

        public DirectoryInfo Directory { get; }

        public static bool IsSavesFolderSymlinked()
        {
            return App.Config.FetchDataFolder().GetDirectories()
                       .FirstOrDefault(directory => directory.Name == Resources.SavesFolderName)?.IsSymlink() ?? true;
        }

        public void LoadProfiles()
        {
            if (ObservableProfilesList == null)
                ObservableProfilesList = new ObservableCollection<BoundProfile>();
            else
                ObservableProfilesList.Clear();
            foreach (var directory in Directory.GetDirectories()) ObservableProfilesList.Add(new BoundProfile(directory));
        }
    }
}