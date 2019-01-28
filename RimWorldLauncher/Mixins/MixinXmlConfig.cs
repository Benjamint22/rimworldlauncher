using System.IO;
using System.Xml.Linq;

namespace RimWorldLauncher.Mixins
{
    /// <summary>
    ///     A mixin that allows saving and loading of a XDocument to a FileInfo.
    /// </summary>
    public interface IMixinXmlConfig
    {
        FileInfo Source { get; set; }
        XDocument XmlRoot { get; set; }
    }

    public static class XmlConfigExtension
    {
        public static void Load(this IMixinXmlConfig config, int tries = 10)
        {
            try
            {
                using (var stream = new FileStream(config.Source.FullName, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.None, 100))
                {
                    config.XmlRoot = XDocument.Load(stream);
                    stream.Close();
                }
            }
            catch (IOException)
            {
                if (tries > 0)
                    config.Load(tries - 1);
                else
                    throw;
            }
        }

        public static void Save(this IMixinXmlConfig config)
        {
            var watcher = config.Source.GetWatcher();
            if (watcher != null) watcher.EnableRaisingEvents = false;
            using (var stream = config.Source.Open(FileMode.Create, FileAccess.Write))
            {
                config.XmlRoot.Save(stream);
                stream.Close();
            }

            if (watcher != null) watcher.EnableRaisingEvents = true;
        }
    }
}