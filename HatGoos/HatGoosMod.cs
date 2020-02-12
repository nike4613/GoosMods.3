using GooseShared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nett;
using System.IO;

namespace HatGoos
{
    public class HatGoosMod : IMod
    {
        private Config config = null;
        private Bitmap hatImage = null;

        public void Init()
        {
            InjectionPoints.PostModsLoaded += PostModsLoaded;
            InjectionPoints.PostRenderEvent += PostRenderEvent;
        }

        private void PostModsLoaded()
        {
            // load configs
            var path = Path.Combine(API.Helper.getModDirectory(this), "config.toml");
            if (File.Exists(path))
                config = Toml.ReadFile<Config>(path);
            else
                config = new Config();
            Toml.WriteFile(config, path);

            // initialize hat image
            switch (config.HatMode)
            {
                case Config.HatType.None:
                    hatImage = new Bitmap(1, 1);
                    hatImage.SetPixel(0, 0, Color.Transparent);
                    break;
                case Config.HatType.Default:
                    hatImage = Resources.Default;
                    break;
                case Config.HatType.Custom:
                    hatImage = new Bitmap(Config.CustomHatPath);
                    break;
            }
        }

        private void PostRenderEvent(GooseEntity goose, Graphics g)
        {
            // I can't actually do anything useful per goose entity, because there isn't a way to
            //   register info when a goose entity is created


        }
    }
}
