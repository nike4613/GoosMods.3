using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos.Readers
{
    public sealed class HatFolderReader : IHatfileReader
    {
        private readonly DirectoryInfo dir;

        public HatFolderReader(string path)
        {
            dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new InvalidOperationException();
        }

        public IHatfileEntry GetEntry(string name)
        {
            var f = dir.EnumerateFiles(name, SearchOption.TopDirectoryOnly).FirstOrDefault(e => e.Name == name);
            if (f is null) return null;
            else return new Entry(f);
        }

        public void Dispose() { }

        private sealed class Entry : IHatfileEntry
        {
            private readonly FileInfo file;
            private Stream s;

            public Entry(FileInfo f)
                => file = f;

            public void Dispose() { }

            public Stream GetStream()
                => s ?? (s = file.OpenRead());
        }
    }
}
