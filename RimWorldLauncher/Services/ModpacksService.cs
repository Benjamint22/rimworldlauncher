using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ModpacksService
    {
        public ModpacksService()
        {
            var dataFolder = App.Config.FetchDataFolder();
            Directory = dataFolder.GetDirectories().FirstOrDefault(dir => dir.Name == Resources.ModpacksFolderName)
                        ?? dataFolder.CreateSubdirectory(Resources.ModpacksFolderName);
            LoadModpacks();
        }

        public DirectoryInfo Directory { get; }

        public ObservableCollection<BoundModList> ObservableModpacksList { get; set; }

        public void AddVanillaModpack()
        {
            if (ObservableModpacksList.Any(modpack => modpack.Identifier == Resources.VanillaModpackName)) return;
            // ReSharper disable once ObjectCreationAsStatement
            new BoundModList("Vanilla (default)", Resources.VanillaModpackName);
            LoadModpacks();
        }

        public void LoadModpacks()
        {
            if (ObservableModpacksList == null)
                ObservableModpacksList = new ObservableCollection<BoundModList>();
            else
                ObservableModpacksList.Clear();
            foreach (var file in Directory.GetFiles()) ObservableModpacksList.Add(new BoundModList(file));
        }
    }
}