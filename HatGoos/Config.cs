using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    class Config
    {
        public enum HatType
        { 
            Default, 
            None, 
            Custom 
        }
        public HatType HatMode = HatType.Default;
        public float HorizontalSize = 1.5f;
        public float HatPosition = .6f;
        public string CustomHatPath = "";
    }
}
