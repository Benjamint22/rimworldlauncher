using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Models
{
    public class Profile : IMixinXmlConfig, INotifyPropertyChanged
    {
        public Profile(string displayName, Modpack modpack, string identifier = null)
        {
            identifier = (identifier ?? displayName).Sanitize();
            ProfileFolder = App.Profiles.Directory.CreateSubdirectory(identifier);
            Source = new FileInfo(Path.Combine(ProfileFolder.FullName, Resources.ProfileConfigName));
            XmlRoot = new XDocument(
                new XElement("config",
                    new XElement("displayName", displayName),
                    new XElement("modpack", modpack.Identifier)
                )
            );
            ProfileFolder.CreateSubdirectory(Resources.SavesFolderName);
            this.Save();
        }

        public Profile(DirectoryInfo profileFolder)
        {
            ProfileFolder = profileFolder;
            Source = profileFolder.GetFiles().First(file => file.Name == Resources.ProfileConfigName);
            this.Load();
        }

        public Modpack Modpack
        {
            get
            {
                var identifier = XmlRoot.Element("config").Element("modpack").Value;
                return App.Modpacks.List.FirstOrDefault(modpack => modpack.Identifier == identifier);
            }
            set
            {
                XmlRoot.Element("config").Element("modpack").Value = value.Identifier;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Modpack"));
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

        public DirectoryInfo ProfileFolder { get; set; }

        public DirectoryInfo SavesFolder => ProfileFolder.GetDirectories()
            .First(directory => directory.Name == Resources.SavesFolderName);

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void StartGame()
        {
            var dataFolder = App.Config.ReadDataFolder();
            App.ActiveModsConfig.SetActiveMods(Modpack);
            dataFolder.CreateJunction(Resources.SavesFolderName, SavesFolder, true);
            Process.Start(Path.Combine(App.Config.ReadGameFolder().FullName, Resources.LauncherName));
            Application.Current.Shutdown();
        }

        public void Delete()
        {
            ProfileFolder.Delete(true);
        }
    }
}