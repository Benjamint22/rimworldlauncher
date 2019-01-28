﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using RimWorldLauncher.Mixins;

namespace RimWorldLauncher.Models
{
    public class Modpack : IList<ModInfo>, IMixinXmlConfig, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public Modpack(string displayName, string identifier)
        {
            identifier = identifier.Sanitize();
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

        public string DisplayName
        {
            get => XmlRoot.Element("modpack").Element("displayName").Value;
            set
            {
                XmlRoot.Element("modpack").Element("displayName").Value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayName"));
                this.Save();
            }
        }

        public string Identifier
        {
            get => Source.Name.EndsWith(".xml") ? Source.Name.Remove(Source.Name.Length - 4) : Source.Name;
            set
            {
                var desiredName = $"{value.Sanitize()}.xml";
                if (App.Modpacks.Directory.EnumerateFiles().Any(file =>
                    file.Name != Source.Name && file.Name.ToLower() == desiredName.ToLower()))
                    throw new ArgumentException("The identifier must be unique.");
                var newDestination = Path.Combine(App.Modpacks.Directory.FullName, desiredName);
                Source.MoveTo(newDestination);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Identifier"));
            }
        }

        public int Count => XmlRoot.Element("modpack").Element("mods").Elements().Count();

        public bool IsReadOnly => false;

        public ModInfo this[int index]
        {
            get => App.Mods.Mods.First(mod =>
                mod.Identifier == XmlRoot.Element("modpack").Element("mods").Elements().ElementAt(index).Value);
            set => XmlRoot.Element("modpack").Element("mods").Elements().ElementAt(index).Value = value.Identifier;
        }

        public void Add(ModInfo item)
        {
            CopyTo(new[] {item}, Count);
        }

        public void Insert(int index, ModInfo item)
        {
            CopyTo(new[] {item}, index);
        }

        public void Clear()
        {
            XmlRoot.Element("modpack").Element("mods").RemoveAll();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            this.Save();
        }

        public bool Contains(ModInfo item)
        {
            return XmlRoot.Element("modpack").Element("mods").Elements()
                .Any(element => element.Value == item.Identifier);
        }

        public void CopyTo(ModInfo[] array, int arrayIndex)
        {
            var mods = XmlRoot.Element("modpack").Element("mods");
            var previousMod = arrayIndex > 0 ? mods.Elements().ElementAt(arrayIndex - 1) : null;
            foreach (var mod in array)
            {
                var newMod = new XElement("li", mod.Identifier);
                if (previousMod != null)
                    previousMod.AddAfterSelf(newMod);
                else
                    mods.AddFirst(newMod);
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

        public bool Remove(ModInfo item)
        {
            return Remove(item, null);
        }

        public int IndexOf(ModInfo item)
        {
            return XmlRoot.Element("modpack").Element("mods").Elements()
                .FirstIndex(element => element.Value == item.Identifier);
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

        public FileInfo Source { get; set; }
        public XDocument XmlRoot { get; set; }
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void Source_Changed(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                this.Load();
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        public void Delete()
        {
            Source.Delete();
        }

        public bool Remove(ModInfo item, int? index)
        {
            var itemToRemove = XmlRoot.Element("modpack").Element("mods").Elements()
                .FirstOrDefault(element => element.Value == item.Identifier);
            if (itemToRemove == null) return false;
            var indexToRemove = index ?? XmlRoot.Element("modpack").Element("mods").Elements()
                                    .FirstIndex(element => element.Value == item.Identifier);
            itemToRemove.Remove();
            CollectionChanged?.Invoke(
                this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    new[] {item},
                    indexToRemove
                )
            );
            this.Save();
            return true;
        }

        private class ModpackEnumerator : IEnumerator<ModInfo>
        {
            public ModpackEnumerator(Modpack modpack)
            {
                Index = -1;
                Modpack = modpack;
            }

            private Modpack Modpack { get; }
            private int Index { get; set; }
            public ModInfo Current => Modpack[Index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return ++Index < Modpack.Count;
            }

            public void Reset()
            {
                Index = 0;
            }
        }
    }
}