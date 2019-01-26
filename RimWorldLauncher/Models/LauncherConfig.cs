using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;

namespace RimWorldLauncher.Models
{
    public class LauncherConfig : MixinXmlConfig
    {
        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public LauncherConfig()
        {
            FileInfo configFile = new FileInfo("launcher.xml");
            if (configFile.Exists)
            {
                LoadConfig(configFile);
            }
            else
            {
                InitializeConfig(configFile);
            }
        }

        public DirectoryInfo ReadGameFolder()
        {
            return CastPath(XmlRoot.Element("configuration").Element("gameFolder").Value);
        }

        public bool SetGameFolder(string folder)
        {
            var info = CastPath(folder);
            if (info == null)
            {
                return false;
            }
            XmlRoot.Element("configuration").Element("gameFolder").Value = info.FullName;
            return true;
        }

        public DirectoryInfo ReadDataFolder()
        {
            return CastPath(XmlRoot.Element("configuration").Element("dataFolder").Value);
        }

        public bool SetDataFolder(string folder)
        {
            var info = CastPath(folder);
            if (info == null)
            {
                return false;
            }
            XmlRoot.Element("configuration").Element("dataFolder").Value = info.FullName;
            return true;
        }

        private DirectoryInfo CastPath(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            DirectoryInfo folder;
            try
            {
                folder = new DirectoryInfo(path);
            }
            catch (Exception)
            {
                return null;
            }

            return !folder.Exists ? null : folder;
        }

        private void LoadConfig(FileInfo config)
        {
            Source = config;
            this.Load();
        }

        private void InitializeConfig(FileInfo config)
        {
            Source = config;
            XmlRoot = new XDocument(
                new XElement("configuration", 
                    new XElement("gameFolder", ""), 
                    new XElement("dataFolder", "")
                )
            );
            SetDataFolder(@"%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios");
            this.Save();
        }
    }
}
