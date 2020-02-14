using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos.Readers
{
    public sealed class HatFileReader : IHatfileReader
    {
        private readonly ZipArchive file;

        public HatFileReader(string path)
        {
            try
            {
                file = ZipFile.OpenRead(path);
            }
            catch
            {
                throw new InvalidOperationException();
            }
        }

        public void Dispose()
        {
            file.Dispose();
        }

        public IHatfileEntry GetEntry(string name)
        {
            var e = file.GetEntry(name);
            if (e is null) return null;
            else return new Entry(e);
        }

        private sealed class Entry : IHatfileEntry
        {
            private readonly ZipArchiveEntry entry;

            public Entry(ZipArchiveEntry e)
                => entry = e;

            public void Dispose() { }

            public Stream GetStream()
                => entry.Open();
        }
    }
}
