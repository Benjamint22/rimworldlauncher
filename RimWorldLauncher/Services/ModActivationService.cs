using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimWorldLauncher.Classes;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class ModActivationService : IMixinXmlConfig
    {
        public ModActivationService()
        {
            Source = App.Config.FetchDataFolder().GetDirectories().First(directory => directory.Name == "Config")
                .GetFiles().First(file => file.Name == Resources.ActiveModsConfigName);
            this.Load();
        }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public IEnumerable<ModInfo> FetchActiveMods()
        {
            return XmlRoot.Element("ModsConfigData")
                ?.Element("activeMods")
                ?.Elements().Select(
                    modElement => modElement.Value == "Core"
                        ? null
                        : App.Mods.ModsList.First(mod => mod.Identifier == modElement.Value)
                ).Where(
                    mod => mod != null
                );
        }

        public void UpdateActiveMods(IEnumerable<ModInfo> mods)
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