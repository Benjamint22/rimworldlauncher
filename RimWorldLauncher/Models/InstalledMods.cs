using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RimWorldLauncher.Models
{
    public class InstalledMods
    {
        public List<ModInfo> Mods { get; set; }

        private DirectoryInfo ModsDirectory { get; set; }

        public InstalledMods()
        {
            var gameFolder = App.Config.ReadGameFolder();
            ModsDirectory = gameFolder.EnumerateDirectories().FirstOrDefault((directory) => directory.Name == Properties.Resources.InstalledModsFolderName) ?? 
                gameFolder.CreateSubdirectory(Properties.Resources.InstalledModsFolderName);
            RefreshMods();
        }

        public void RefreshMods()
        {
            Mods = ModsDirectory.GetDirectories().Where((modDirectory) => modDirectory.Name != "Core").Select((modDirectory) => new ModInfo(modDirectory)).ToList();
        }
    }
}