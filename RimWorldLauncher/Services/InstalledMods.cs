using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorldLauncher.Models;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class InstalledMods
    {
        public InstalledMods()
        {
            var gameFolder = App.Config.ReadGameFolder();
            ModsDirectory = gameFolder.EnumerateDirectories()
                                .FirstOrDefault(directory => directory.Name == Resources.InstalledModsFolderName) ??
                            gameFolder.CreateSubdirectory(Resources.InstalledModsFolderName);
            RefreshMods();
        }

        public List<ModInfo> Mods { get; set; }

        private DirectoryInfo ModsDirectory { get; }

        public void RefreshMods()
        {
            Mods = ModsDirectory.GetDirectories().Where(modDirectory => modDirectory.Name != "Core")
                .Select(modDirectory => new ModInfo(modDirectory)).ToList();
        }
    }
}