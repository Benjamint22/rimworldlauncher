using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;

namespace RimWorldLauncher.Services
{
    public class InvalidConfigDirectoryException : Exception
    {
    }

    public abstract class ConfigDirectory
    {
        public DirectoryInfo Directory { get; private set; }
        
        public ConfigDirectory(string path)
        {
            var info = FileSystemInfoExtensions.FromPath(path);
            if (info == null || !info.Exists) throw new DirectoryNotFoundException();
            if (!IsValid(info)) throw new InvalidConfigDirectoryException();
            Directory = info;
        }

        protected abstract bool IsValid(DirectoryInfo directoryInfo);
    }

    public class GameDirectory: ConfigDirectory
    {
        public GameDirectory(string path) : base(path)
        {
        }

        protected override bool IsValid(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateFiles()
                .Any(directory => directory.Name == Properties.Resources.LauncherName);
        }
    }
    
    public class DataDirectory: ConfigDirectory
    {
        public DataDirectory(string path) : base(path)
        {
        }

        protected override bool IsValid(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateDirectories()
                .Any(directory => directory.Name == Properties.Resources.SavesFolderName);
        }
    }
    
    public class LauncherConfig : IMixinXmlConfig
    {       
        public LauncherConfig()
        {
            var configFile = new FileInfo("launcher.xml");
            if (configFile.Exists)
                LoadConfig(configFile);
            else
                InitializeConfig(configFile);
        }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public DirectoryInfo ReadGameFolder()
        {
            return FileSystemInfoExtensions.FromPath(
                XmlRoot.Element("configuration")?.Element("gameFolder")?.Value
            );
        }

        public void SetGameFolder(GameDirectory directory)
        {
            // ReSharper disable PossibleNullReferenceException
            XmlRoot.Element("configuration").Element("gameFolder").Value = directory.Directory.FullName;
            // ReSharper restore PossibleNullReferenceException
        }

        public DirectoryInfo ReadDataFolder()
        {
            return FileSystemInfoExtensions.FromPath(
                XmlRoot?.Element("configuration")?.Element("dataFolder")?.Value
            );
        }

        public void SetDataFolder(DataDirectory directory)
        {
            // ReSharper disable PossibleNullReferenceException
            XmlRoot.Element("configuration").Element("dataFolder").Value = directory.Directory.FullName;
            // ReSharper restore PossibleNullReferenceException
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
            try
            {
                SetDataFolder(new DataDirectory(@"%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"));
            }
            catch (InvalidConfigDirectoryException)
            {}
            this.Save();
        }
    }
}