using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RimWorldLauncher.Models;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ModpacksReader
    {
        public ModpacksReader()
        {
            var dataFolder = App.Config.ReadDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault(dir => dir.Name == Resources.ModpacksFolderName)
                        ?? dataFolder.CreateSubdirectory(Resources.ModpacksFolderName);
            Refresh();
        }

        public ObservableCollection<Modpack> List { get; set; }

        public DirectoryInfo Directory { get; }

        public void Refresh()
        {
            if (List == null)
                List = new ObservableCollection<Modpack>();
            else
                List.Clear();
            foreach (var file in Directory.GetFiles()) List.Add(new Modpack(file));
        }

        public void AddVanillaModpack()
        {
            if (List.Any(modpack => modpack.Identifier == "vanilla")) return;
            // ReSharper disable once ObjectCreationAsStatement
            new Modpack("Vanilla (default)", "vanilla");
            Refresh();
        }
    }
}