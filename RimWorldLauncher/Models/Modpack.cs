using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RimWorldLauncher.Models
{
    public partial class Modpack : IList<ModInfo>, MixinXmlConfig, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public static string FilterIdentifier(string unfilteredIdentifier)
        {
            return Regex.Replace(Regex.Replace(unfilteredIdentifier.ToLower(), @"[\s\-]", "_"), @"[^a-zA-Z0-9_\.]", "");
        }

        public string DisplayName
        {
            get => XmlRoot.Element("modpack").Element("displayName").Value;
            set
            {
                XmlRoot.Element("modpack").Element("displayName").Value = value;
                PropertyChanged(this, new PropertyChangedEventArgs("DisplayName"));
                this.Save();
            }
        }

        public string Identifier
        {
            get => Source.Name.EndsWith(".xml") ? Source.Name.Remove(Source.Name.Length - 4) : Source.Name;
            set
            {
                var desiredName = $"{FilterIdentifier(value)}.xml";
                if (App.Modpacks.Directory.EnumerateFiles().Any((file) => file.Name != Source.Name && file.Name.ToLower() == desiredName.ToLower()))
                {
                    throw new ArgumentException("The identifier must be unique.");
                }
                string newDestination = Path.Combine(App.Modpacks.Directory.FullName, desiredName);
                Source.MoveTo(newDestination);
                PropertyChanged(this, new PropertyChangedEventArgs("Identifier"));
            }
        }

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }

        public int Count => XmlRoot.Element("modpack").Element("mods").Elements().Count();

        public bool IsReadOnly => false;

        public ModInfo this[int index]
        {
            get => App.Mods.Mods.First((mod) => mod.Identifier == XmlRoot.Element("modpack").Element("mods").Elements().ElementAt(index).Value);
            set => XmlRoot.Element("modpack").Element("mods").Elements().ElementAt(index).Value = value.Identifier;
        }

        public Modpack(string displayName, string identifier)
        {
            identifier = FilterIdentifier(identifier);
            XmlRoot = new XDocument(
                new XElement("modpack",
                    new XElement("displayName", displayName),
                    new XElement("mods")
                )
            );
            Source = new FileInfo(Path.Combine(App.Modpacks.Directory.FullName, $"{identifier}.xml"));
            Source.Watch(Source_Changed);
            this.Save();
        }

        public Modpack(FileInfo modpackXml)
        {
            Source = modpackXml;
            Source.Watch(Source_Changed);
            this.Load();
        }

        private void Source_Changed(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.Invoke((Action)delegate {
                this.Load();
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        public void Delete()
        {
            Source.Delete();
        }

        public void Add(ModInfo item)
        {
            CopyTo(new ModInfo[] { item }, Count);
        }

        public void Insert(int index, ModInfo item)
        {
            CopyTo(new ModInfo[] { item }, index);
        }

        public void Clear()
        {
            XmlRoot.Element("modpack").Element("mods").RemoveAll();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.Save();
        }

        public bool Contains(ModInfo item)
        {
            return XmlRoot.Element("modpack").Element("mods").Elements().Any((element) => element.Value == item.Identifier);
        }

        public void CopyTo(ModInfo[] array, int arrayIndex)
        {
            var mods = XmlRoot.Element("modpack").Element("mods");
            var previousMod = arrayIndex > 0 ? mods.Elements().ElementAt(arrayIndex - 1) : null;
            foreach (var mod in array)
            {
                var newMod = new XElement("li", mod.Identifier);
                if (previousMod != null)
                {
                    previousMod.AddAfterSelf(newMod);
                }
                else
                {
                    mods.AddFirst(newMod);
                }
                previousMod = newMod;
            }
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    array,
                    arrayIndex
                )
            );
            this.Save();
        }

        public bool Remove(ModInfo item) => Remove(item, null);

        public bool Remove(ModInfo item, int? index)
        {
            var itemToRemove = XmlRoot.Element("modpack").Element("mods").Elements().FirstOrDefault((element) => element.Value == item.Identifier);
            if (itemToRemove == null)
            {
                return false;
            }
            var indexToRemove = index ?? XmlRoot.Element("modpack").Element("mods").Elements().FirstIndex((element) => element.Value == item.Identifier);
            itemToRemove.Remove();
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    new ModInfo[] { item },
                    indexToRemove
                )
            );
            this.Save();
            return true;
        }

        public int IndexOf(ModInfo item)
        {
            return XmlRoot.Element("modpack").Element("mods").Elements().FirstIndex((element) => element.Value == item.Identifier);
        }

        public void RemoveAt(int index)
        {
            var mod = this[index];
            Remove(mod, index);
        }

        public IEnumerator<ModInfo> GetEnumerator()
        {
            return new ModpackEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ModpackEnumerator(this);
        }

        private class ModpackEnumerator : IEnumerator<ModInfo>
        {
            public ModInfo Current => _Modpack[_Index];

            object IEnumerator.Current => Current as object;

            private Modpack _Modpack { get; }
            private int _Index { get; set; }

            public ModpackEnumerator(Modpack modpack)
            {
                _Index = -1;
                _Modpack = modpack;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return (++_Index < _Modpack.Count);
            }

            public void Reset()
            {
                _Index = 0;
            }
        }
    }
}
