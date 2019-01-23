using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public class Modpack : XmlConfig
    {
        public static string FilterIdentifier(string unfilteredIdentifier)
        {
            return Regex.Replace(Regex.Replace(unfilteredIdentifier.ToLower(), @"[\s\-]", "_"), @"[^a-zA-Z0-9_\.]", "");
        }

        public string DisplayName
        {
            get => XmlRoot.Element("modpack").Element("displayName").Value;
            set
            {
                XmlRoot.Element("modpack").Element("displayName").Value = value;
                Save();
            }
        }

        public string Identifier
        {
            get => Source.Name.EndsWith(".xml") ? Source.Name.Remove(Source.Name.Length - 4) : Source.Name;
            set
            {
                var desiredName = $"{FilterIdentifier(value)}.xml";
                if (App.Modpacks.Directory.EnumerateFiles().Any((file) => file.Name != Source.Name && file.Name.ToLower() == desiredName.ToLower()))
                {
                    throw new ArgumentException("The identifier must be unique.");
                }
                string newDestination = Path.Combine(App.Modpacks.Directory.FullName, desiredName);
                Source.MoveTo(newDestination);
            }
        }

        public ObservableCollection<ModInfo> Mods { get; }

        public Modpack(string displayName)
        {
            var identifier = FilterIdentifier(displayName);
            XmlRoot = new XDocument(
                new XElement("modpack",
                    new XElement("displayName", displayName),
                    new XElement("mods")
                )
            );
            Mods = new ObservableCollection<ModInfo>();
            Source = new FileInfo(Path.Combine(App.Modpacks.Directory.FullName, $"{identifier}.xml"));
            Save();
        }

        public Modpack(FileInfo modpackXml)
        {
            Source = modpackXml;
            Load();
            Mods = new ObservableCollection<ModInfo>();
            Mods.CopyTo(XmlRoot.Element("modpack").Element("mods").Elements().Select((element) => App.Mods.Mods.First((mod) => mod.Identifier == element.Value)).ToArray(), 0);
        }

        public void Delete()
        {
            Source.Delete();
        }
    }
}
