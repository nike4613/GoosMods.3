using HatGoos.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    public static class HatfileReader
    {
        public static IHatfileReader Create(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    return new HatFolderReader(path);
                else if (File.Exists(path))
                    return new HatFileReader(path);
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }

    public interface IHatfileReader : IDisposable
    {
        /// <summary>
        /// gets the entry or <see langword="null"/> for a name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IHatfileEntry GetEntry(string name);
    }

    public interface IHatfileEntry : IDisposable
    {
        /// <summary>
        /// gets a read stream for the entry
        /// </summary>
        /// <returns></returns>
        Stream GetStream();
    }
}
