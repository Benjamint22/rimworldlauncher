using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    /// <summary>
    /// A mixin that allows saving and loading of a XDocument to a FileInfo.
    /// </summary>
    public interface MixinXmlConfig
    {
        FileInfo Source { get; set; }
        XDocument XmlRoot { get; set; }
    }

    public static class IXmlConfigExtension
    {
        public static void Load(this MixinXmlConfig config, int tries = 10)
        {
            try
            {
                using (var stream = new FileStream(config.Source.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100))
                {
                    config.XmlRoot = XDocument.Load(stream);
                    stream.Close();
                }
            }
            catch (IOException e)
            {
                if (tries > 0)
                {
                    config.Load(tries - 1);
                }
                else
                {
                    throw e;
                }
            }
        }

        public static void Save(this MixinXmlConfig config)
        {
            var watcher = config.Source.GetWatcher();
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
            using (var stream = config.Source.Open(FileMode.Create, FileAccess.Write))
            {
                config.XmlRoot.Save(stream);
                stream.Close();
            }
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = true;
            }
        }
    }
}
