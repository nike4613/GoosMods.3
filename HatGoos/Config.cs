using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nett;

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

        [TomlComment(" Valid values: \"" + nameof(HatType.Default) + "\", \"" + nameof(HatType.Custom)
            + "\", and \"" + nameof(HatType.None) + "\".", CommentLocation.Prepend)]
        [TomlComment("   \"" + nameof(HatType.Default) + "\" uses the default hat", CommentLocation.Prepend)]
        [TomlComment("   \"" + nameof(HatType.Custom) + "\" uses the hat file specified in \""
            + nameof(CustomHatPath) + "\"", CommentLocation.Prepend)]
        [TomlComment("   \"" + nameof(HatType.None) + "\" shows no hat at all", CommentLocation.Prepend)]
        public HatType HatMode { get; set; } = HatType.Default;

        [TomlComment("")]
        [TomlComment(" This may be a path to a 'hatfile', a folder in the structure of a 'hatfile', or an image supported by System.Drawing.Bitmap.", CommentLocation.Prepend)]
        [TomlComment(" A 'hatfile' is a ZIP archive containing a file named \"" + HatGoosMod.HatfileMeta + "\".", CommentLocation.Prepend)]
        [TomlComment("   This TOML configuration file should have the following entries:", CommentLocation.Prepend)]
        [TomlComment("   - " + nameof(HatSettings.ImageName) + ": the name of the entry in the hatfile archive that represents the hat image", CommentLocation.Prepend)]
        [TomlComment("   - " + nameof(HatSettings.HorizontalSize) + ": the size of the horizonal axis of the hat in terms of head diameters", CommentLocation.Prepend)]
        [TomlComment("   - " + nameof(HatSettings.HatPosition) + ": the vertical position of the hat in head radii", CommentLocation.Prepend)]
        [TomlComment(" You may override the settings specified in the hatfile's configuration using an [" + nameof(Overrides) + "] field in the same structure.")]
        public string CustomHatPath { get; set; } = "";

        public HatSettings Overrides { get; set; } = null;
    }

    public class HatSettings
    {
        public string ImageName { get; set; } = null;
        public float? HorizontalSize { get; set; } = null;
        public float? HatPosition { get; set; } = null;

        public HatSettings WithOverride(HatSettings over)
            => new HatSettings
            {
                HorizontalSize = over?.HorizontalSize ?? HorizontalSize,
                HatPosition = over?.HatPosition ?? HatPosition,
                ImageName = over?.ImageName ?? ImageName
            };
    }

}
