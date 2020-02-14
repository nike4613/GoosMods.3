using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    public struct Hat
    {
        public Bitmap Image { get; }
        public HatSettings Settings { get; }
        public Hat(Bitmap bm, HatSettings settings)
        {
            Image = bm;
            Settings = settings;
        }
    }
}
