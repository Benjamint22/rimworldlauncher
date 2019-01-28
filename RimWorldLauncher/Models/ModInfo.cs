using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public class InvalidModDirectoryException : Exception
    {
    }

    public class InvalidModManifestException : Exception
    {
    }

    public class ModInfo
    {
        private static readonly BitmapImage DefaultPreview =
            new BitmapImage(new Uri("/Images/DefaultPreview.png", UriKind.Relative));

        public ModInfo(DirectoryInfo modDirectory)
        {
            // Open the About directory
            if (modDirectory == null) throw new ArgumentNullException();
            var aboutDirectory = modDirectory
                .EnumerateDirectories().FirstOrDefault(directory => directory.Name == "About");
            if (aboutDirectory == null) throw new InvalidModDirectoryException();

            // Look for About.xml and Manifest.xml
            FileInfo previewFile = null;
            FileInfo aboutFile = null;
            FileInfo manifestFile = null;
            foreach (var file in aboutDirectory.EnumerateFiles())
            {
                if (previewFile != null && aboutFile != null && manifestFile != null) break;

                if (previewFile == null && file.Name.ToLower() == "preview.png")
                    previewFile = file;
                else if (aboutFile == null && file.Name.ToLower() == "about.xml")
                    aboutFile = file;
                else if (manifestFile == null && file.Name.ToLower() == "manifest.xml") manifestFile = file;
            }

            if (aboutFile == null) throw new InvalidModDirectoryException();

            // Preview
            if (previewFile != null)
                using (var stream = previewFile.OpenRead())
                {
                    Preview = new BitmapImage();
                    Preview.BeginInit();
                    Preview.StreamSource = stream;
                    Preview.CacheOption = BitmapCacheOption.OnLoad;
                    Preview.EndInit();
                    Preview.Freeze();
                }
            else
                Preview = DefaultPreview;

            // About
            // Copy info from About.xml
            var aboutXml = XDocument.Load(aboutFile.OpenRead());
            var aboutMetaData = aboutXml.Element("ModMetaData");
            if (aboutMetaData == null) throw new InvalidModManifestException();

            Identifier = modDirectory.Name;
            DisplayName = aboutMetaData.Element("name")?.Value ?? Identifier;
            Author = aboutMetaData.Element("author")?.Value;
            TargetGameVersion = aboutMetaData.Element("targetVersion")?.Value;
            Url = aboutMetaData.Element("url")?.Value;
            Description = aboutMetaData.Element("description")?.Value?.Replace("\\n", "\n");

            // Manifest
            if (manifestFile != null)
            {
                // Copy info from Manifest.xml
                var manifestXml = XDocument.Load(manifestFile.OpenRead());
                var manifestMetaData = manifestXml.Element("Manifest");
                if (manifestMetaData == null) throw new InvalidModManifestException();

                //Identifier = manifestMetaData.Element("identifier")?.Value ?? $"{DisplayName}_{GetRandomNumberSeries(6)}";
                ModVersion = manifestMetaData.Element("version")?.Value;
            }
            else
            {
                // Generate fake manifest
                ModVersion = "?";
            }
        }

        public string DisplayName { get; }
        public BitmapImage Preview { get; }
        public string Identifier { get; }
        public string Author { get; }
        public string ModVersion { get; }
        public string TargetGameVersion { get; }
        public string Url { get; }
        public string Description { get; }
    }
}