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
        public string CustomHatPath { get; set; } = "";
        public HatSettings Overrides { get; set; } = null;
    }

    public class HatSettings
    {
        public float? HorizontalSize { get; set; }
        public float? HatPosition { get; set; }
        public string ImageName { get; set; }

        public HatSettings WithOverride(HatSettings over)
            => new HatSettings
            {
                HorizontalSize = over?.HorizontalSize ?? HorizontalSize,
                HatPosition = over?.HatPosition ?? HatPosition,
                ImageName = over?.ImageName ?? ImageName
            };
    }

}
