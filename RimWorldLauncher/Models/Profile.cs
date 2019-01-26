using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public class Profile : MixinXmlConfig, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }
        public Modpack Modpack
        {
            get
            {
                var identifier = XmlRoot.Element("config").Element("modpack").Value;
                return App.Modpacks.List.FirstOrDefault((modpack) => modpack.Identifier == identifier);
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
            get
            {
                return XmlRoot.Element("config").Element("displayName").Value;
            }
            set
            {
                XmlRoot.Element("config").Element("displayName").Value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
                this.Save();
            }
        }
        public DirectoryInfo ProfileFolder { get; set; }
        public DirectoryInfo SavesFolder => ProfileFolder.GetDirectories().First((directory) => directory.Name == Properties.Resources.SavesFolderName);

        public Profile(string displayName, Modpack modpack, string identifier = null)
        {
            identifier = (identifier ?? displayName).Sanitize();
            ProfileFolder = App.Profiles.Directory.CreateSubdirectory(identifier);
            Source = new FileInfo(Path.Combine(ProfileFolder.FullName, Properties.Resources.ProfileConfigName));
            XmlRoot = new XDocument(
                new XElement("config",
                    new XElement("displayName", displayName),
                    new XElement("modpack", modpack.Identifier)
                )
            );
            ProfileFolder.CreateSubdirectory(Properties.Resources.SavesFolderName);
            this.Save();
        }

        public Profile(DirectoryInfo profileFolder)
        {
            ProfileFolder = profileFolder;
            Source = profileFolder.GetFiles().First((file) => file.Name == Properties.Resources.ProfileConfigName);
            this.Load();
        }

        public void StartGame()
        {
            var dataFolder = App.Config.ReadDataFolder();
            App.ActiveModsConfig.SetActiveMods(Modpack);
            dataFolder.CreateJunction(Properties.Resources.SavesFolderName, SavesFolder, true);
            Process.Start(Path.Combine(App.Config.ReadGameFolder().FullName, Properties.Resources.LauncherName));
            App.Current.Shutdown();
        }

        public void Delete()
        {
            ProfileFolder.Delete(true);
        }
    }
}
