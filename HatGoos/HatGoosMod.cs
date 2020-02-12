using GooseShared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nett;
using System.IO;
using SamEngine;

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

            if (config.HatMode == Config.HatType.Custom && !File.Exists(config.CustomHatPath))
                config.HatMode = Config.HatType.Default;

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
                    try
                    {
                        hatImage = new Bitmap(config.CustomHatPath);
                    }
                    catch
                    {
                        hatImage = null;
                    }
                    break;
            }
        }

        private void PostRenderEvent(GooseEntity goose, Graphics g)
        {
            // I can't actually do anything useful per goose entity, because there isn't a way to
            //   register info when a goose entity is created

            if (hatImage == null) return;

            var direction = goose.direction + 90f; ;
            var headPoint = goose.rig.neckHeadPoint;

            var vertBase = (Rig.HeadRadius1 / 2) * config.HatPosition;

            var bmp = hatImage;
            var baseOffset = Rig.HeadRadius1 * config.HorizontalSize / 2;
            var vertOffset = (((float)bmp.Height) / (float)bmp.Width) * baseOffset * 2;

            var set = new[] {
                new Vector2(-baseOffset, vertBase + vertOffset),
                new Vector2(baseOffset, vertBase + vertOffset),
                new Vector2(-baseOffset, vertBase)
            };

            float? sin = null, cos = null;
            var asPoints = set.Select(v => Rotate(v, direction, ref sin, ref cos))
                              .Select(v => v + headPoint)
                              .Select(ToPoint).ToArray();

            g.DrawImage(hatImage, asPoints);
        }

        private static Vector2 Rotate(Vector2 point, float degrees, ref float? sin, ref float? cos)
        {
            if (sin == null) sin = Sin(degrees);
            if (cos == null) cos = Cos(degrees);
            return new Vector2(point.x * cos.Value - point.y * sin.Value,
                               point.y * cos.Value + point.x * sin.Value);
        }

        private static float Sin(float deg)
            => (float)Math.Sin(deg * (Math.PI / 180d));
        private static float Cos(float deg)
            => (float)Math.Cos(deg * (Math.PI / 180d));

        private static Point ToPoint(Vector2 vector)
            => new Point((int)vector.x, (int)vector.y);
    }
}
