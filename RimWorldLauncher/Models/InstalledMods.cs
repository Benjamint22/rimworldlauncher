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
            var modsDirectory = gameFolder.EnumerateDirectories().FirstOrDefault((directory) => directory.Name == Properties.Resources.InstalledModsFolderName);
            if (modsDirectory != null)
            {
                ModsDirectory = modsDirectory;
            }
            else
            {
                var legacyModsDirectory = gameFolder.EnumerateDirectories().FirstOrDefault((directory) => directory.Name == Properties.Resources.LegacyModsFolderName);
                if (legacyModsDirectory != null)
                {
                    legacyModsDirectory.MoveTo(Path.Combine(gameFolder.FullName, Properties.Resources.InstalledModsFolderName));
                    ModsDirectory = legacyModsDirectory;
                    gameFolder.CreateSubdirectory(Properties.Resources.LegacyModsFolderName);
                }
                else
                {
                    ModsDirectory = gameFolder.CreateSubdirectory(Properties.Resources.InstalledModsFolderName);
                    gameFolder.CreateSubdirectory(Properties.Resources.LegacyModsFolderName);
                }
            }
            RefreshMods();
        }

        public void RefreshMods()
        {
            Mods = ModsDirectory.GetDirectories().Select((modDirectory) => new ModInfo(modDirectory)).ToList();
        }
    }
}