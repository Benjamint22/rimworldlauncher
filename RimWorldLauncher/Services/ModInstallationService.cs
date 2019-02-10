using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ModInstallationService
    {
        public ModInstallationService()
        {
            var gameFolder = App.Config.FetchGameFolder();
            ModsDirectory = gameFolder.EnumerateDirectories()
                                .FirstOrDefault(directory => directory.Name == Resources.InstalledModsFolderName) ??
                            gameFolder.CreateSubdirectory(Resources.InstalledModsFolderName);
            LoadMods();
        }

        public List<ModInfo> ModsList { get; set; }

        private DirectoryInfo ModsDirectory { get; }

        public void LoadMods()
        {
            ModsList = ModsDirectory.GetDirectories().Where(modDirectory => modDirectory.Name != "Core")
                .Select(modDirectory => new ModInfo(modDirectory)).ToList();
        }
    }
}