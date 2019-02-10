using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;
using RimWorldLauncher.Properties;

namespace RimWorldLauncher.Services
{
    public class InvalidConfigDirectoryException : Exception
    {
    }

    public abstract class ConfigDirectory
    {
        public ConfigDirectory(string path)
        {
            var info = FileSystemInfoExtensions.FromPath(path);
            if (info == null || !info.Exists) throw new DirectoryNotFoundException();
            if (!IsValid(info)) throw new InvalidConfigDirectoryException();
            Directory = info;
        }

        public DirectoryInfo Directory { get; }

        protected abstract bool IsValid(DirectoryInfo directoryInfo);
    }

    public class GameDirectory : ConfigDirectory
    {
        public GameDirectory(string path) : base(path)
        {
        }

        protected override bool IsValid(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateFiles()
                .Any(directory => directory.Name == Resources.LauncherName);
        }
    }

    public class DataDirectory : ConfigDirectory
    {
        public DataDirectory(string path) : base(path)
        {
        }

        protected override bool IsValid(DirectoryInfo directoryInfo)
        {
            return directoryInfo.EnumerateDirectories()
                .Any(directory => directory.Name == Resources.SavesFolderName);
        }
    }

    public class ConfigurationService : IMixinXmlConfig
    {
        public ConfigurationService()
        {
            var configFile = new FileInfo("launcher.xml");
            if (configFile.Exists)
                LoadConfig(configFile);
            else
                InitializeConfig(configFile);
        }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public DirectoryInfo FetchGameFolder()
        {
            return FileSystemInfoExtensions.FromPath(
                XmlRoot.Element("configuration")?.Element("gameFolder")?.Value
            );
        }

        public void UpdateGameFolder(GameDirectory directory)
        {
            // ReSharper disable PossibleNullReferenceException
            XmlRoot.Element("configuration").Element("gameFolder").Value = directory.Directory.FullName;
            // ReSharper restore PossibleNullReferenceException
            this.Save();
        }

        public DirectoryInfo FetchDataFolder()
        {
            return FileSystemInfoExtensions.FromPath(
                XmlRoot?.Element("configuration")?.Element("dataFolder")?.Value
            );
        }

        public void UpdateDataFolder(DataDirectory directory)
        {
            // ReSharper disable PossibleNullReferenceException
            XmlRoot.Element("configuration").Element("dataFolder").Value = directory.Directory.FullName;
            // ReSharper restore PossibleNullReferenceException
            this.Save();
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
                UpdateDataFolder(new DataDirectory(@"%APPDATA%\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"));
            }
            catch (InvalidConfigDirectoryException)
            {
            }
            this.Save();
        }
    }
}