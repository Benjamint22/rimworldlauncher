using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Classes
{
    public class BoundProfile : IMixinXmlConfig, INotifyPropertyChanged
    {
        public BoundProfile(string displayName, BoundModList boundModList, string identifier = null)
        {
            identifier = (identifier ?? displayName).Sanitize();
            ProfileFolder = App.Profiles.Directory.CreateSubdirectory(identifier);
            Source = new FileInfo(Path.Combine(ProfileFolder.FullName, Resources.ProfileConfigName));
            XmlRoot = new XDocument(
                new XElement("config",
                    new XElement("displayName", displayName),
                    new XElement("modpack", boundModList.Identifier)
                )
            );
            ProfileFolder.CreateSubdirectory(Resources.SavesFolderName);
            this.Save();
        }

        public BoundProfile(DirectoryInfo profileFolder)
        {
            ProfileFolder = profileFolder;
            Source = profileFolder.GetFiles().First(file => file.Name == Resources.ProfileConfigName);
            this.Load();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DirectoryInfo ProfileFolder { get; set; }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public DirectoryInfo SavesFolder => ProfileFolder.GetDirectories()
            .First(directory => directory.Name == Resources.SavesFolderName);

        public BoundModList BoundModList
        {
            get
            {
                var identifier = XmlRoot.Element("config").Element("modpack").Value;
                return App.Modpacks.ObservableModpacksList.FirstOrDefault(modpack => modpack.Identifier == identifier);
            }
            set
            {
                XmlRoot.Element("config").Element("modpack").Value = value.Identifier;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BoundModList"));
                this.Save();
            }
        }

        public string DisplayName
        {
            get => XmlRoot.Element("config").Element("displayName").Value;
            set
            {
                XmlRoot.Element("config").Element("displayName").Value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
                this.Save();
            }
        }

        public void Delete()
        {
            ProfileFolder.Delete(true);
        }

        public void StartGame()
        {
            var dataFolder = App.Config.FetchDataFolder();
            App.ActiveModsConfig.UpdateActiveMods(BoundModList);
            dataFolder.CreateJunction(Resources.SavesFolderName, SavesFolder, true);
            Process.Start(Path.Combine(App.Config.FetchGameFolder().FullName, Resources.LauncherName));
            Application.Current.Shutdown();
        }
    }
}