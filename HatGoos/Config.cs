using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HatGoos
{
    public class Config
    {
        public enum HatType
        { 
            Default, 
            None, 
            Custom 
        }
        public HatType HatMode { get; set; } = HatType.Default;
        public float HorizontalSize { get; set; } = 1.5f;
        public float HatPosition { get; set; } = .6f;
        public string CustomHatPath { get; set; } = "";
    }
}
