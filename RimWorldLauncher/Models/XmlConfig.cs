using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public abstract class XmlConfig
    {
        protected FileInfo Source { get; set; }
        protected XDocument XmlRoot { get; set; }

        public void Load()
        {
            using (var stream = Source.OpenRead())
            {
                XmlRoot = XDocument.Load(stream);
                stream.Close();
            }
        }

        public void Save()
        {
            using (var stream = Source.Open(FileMode.Create, FileAccess.Write))
            {
                XmlRoot.Save(stream);
                stream.Close();
            }
        }
    }
}
