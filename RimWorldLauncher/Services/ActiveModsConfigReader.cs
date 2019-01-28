using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Models;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ActiveModsConfigReader : IMixinXmlConfig
    {
        public ActiveModsConfigReader()
        {
            Source = App.Config.ReadDataFolder().GetDirectories().First(directory => directory.Name == "Config")
                .GetFiles().First(file => file.Name == Resources.ActiveModsConfigName);
            this.Load();
        }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public IEnumerable<ModInfo> GetActiveMods()
        {
            return XmlRoot.Element("ModsConfigData")
                ?.Element("activeMods")
                ?.Elements().Select(
                modElement => modElement.Value == "Core"
                    ? null
                    : App.Mods.Mods.First(mod => mod.Identifier == modElement.Value)
            ).Where(
                mod => mod != null
            );
        }

        public void SetActiveMods(IEnumerable<ModInfo> mods)
        {
            var activeModsElement = XmlRoot.Element("ModsConfigData")?.Element("activeMods");
            Debug.Assert(activeModsElement != null, nameof(activeModsElement) + " != null");
            activeModsElement.RemoveNodes();
            activeModsElement.Add(new XElement("li", "Core"));
            foreach (var mod in mods) activeModsElement.Add(new XElement("li", mod.Identifier));
            this.Save();
        }
    }
}