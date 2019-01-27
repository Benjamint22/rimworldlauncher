using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public class ActiveModsConfigReader : MixinXmlConfig
    {
        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public ActiveModsConfigReader()
        {
            Source = App.Config.ReadDataFolder().GetDirectories().First((directory) => directory.Name == "Config")
                .GetFiles().First((file) => file.Name == Properties.Resources.ActiveModsConfigName);
            this.Load();
        }

        public IEnumerable<ModInfo> GetActiveMods()
        {
            return XmlRoot.Element("ModsConfigData").Element("activeMods").Elements().Select(
                (modElement) => modElement.Value == "Core" ? null : App.Mods.Mods.First((mod) => mod.Identifier == modElement.Value)
            ).Where(
                (mod) => mod != null    
            );
        }

        public void SetActiveMods(IEnumerable<ModInfo> mods)
        {
            var activeModsElement = XmlRoot.Element("ModsConfigData").Element("activeMods");
            activeModsElement.RemoveNodes();
            activeModsElement.Add(new XElement("li", "Core"));
            foreach (var mod in mods)
            {
                activeModsElement.Add(new XElement("li", mod.Identifier));
            }
            this.Save();
        }
    }
}
