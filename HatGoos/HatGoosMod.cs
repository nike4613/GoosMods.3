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
using System.IO.Compression;
using System.Windows.Forms;

namespace HatGoos
{
    public class HatGoosMod : IMod
    {
        private Config config = null;

        private Hat hat = default;

        public const string Name = nameof(HatGoos);
        public const string HatfileMeta = "hat.toml";

        private readonly TomlSettings tomlSettings = TomlSettings.Create(c => c
            .AllowImplicitConversions(TomlSettings.ConversionSets.All)
            .ConfigureType<float?>(b => b
                .WithConversionFor<TomlFloat>(v => v
                    .ToToml(f => f.Value)
                    .FromToml(t => (float)t.Value))));

        public void Init()
        {
            InjectionPoints.PostModsLoaded += PostModsLoaded;
            InjectionPoints.PostRenderEvent += PostRenderEvent;
        }

        private void PostModsLoaded()
        {
            try
            {
                // load configs
                var path = Path.Combine(API.Helper.getModDirectory(this), "config.toml");
                if (File.Exists(path))
                    config = Toml.ReadFile<Config>(path, tomlSettings);
                else
                    config = new Config();

                Toml.WriteFile(config, path, tomlSettings);

                // initialize hat image
                switch (config.HatMode)
                {
                    case Config.HatType.None:
                        hat = new Hat(new Bitmap(1, 1), GetDefaultHatSettings(tomlSettings));
                        hat.Image.SetPixel(0, 0, Color.Transparent);
                        break;
                    case Config.HatType.Default:
                        hat = new Hat(Resources.DefaultImage, GetDefaultHatSettings(tomlSettings));
                        break;
                    case Config.HatType.Custom:
                        hat = ReadFromHatfile(config.CustomHatPath, config.Overrides, tomlSettings);
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error initializing {Name}: {e}",
                    Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                hat = default;
            }
        }

        private static Hat ReadFromHatfile(string path, HatSettings overrides, TomlSettings tomlSettings)
        {
            try
            {
                using (var zf = HatfileReader.Create(path))
                {
                    var entry = zf.GetEntry(HatfileMeta);
                    if (entry == null)
                    {
                        MessageBox.Show($"Custom hat at '{path}' is invalid. It does not have '{HatfileMeta}'.",
                            Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return default;
                    }

                    HatSettings settings;
                    using (var str = entry.GetStream())
                        settings = Toml.ReadStream<HatSettings>(str, tomlSettings).WithOverride(overrides);

                    var imageEntry = zf.GetEntry(settings.ImageName);
                    if (imageEntry == null)
                    {
                        MessageBox.Show($"Could not find image '{settings.ImageName}' in hat '{path}'.",
                            Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return default;
                    }

                    Bitmap hatImage;
                    using (var str = imageEntry.GetStream())
                        hatImage = new Bitmap(str);
                    return new Hat(hatImage, settings);
                }
            }
            catch
            {
                try
                {
                    var hatImage = new Bitmap(path);
                    var settings = GetDefaultHatSettings(tomlSettings).WithOverride(overrides);
                    return new Hat(hatImage, settings);
                }
                catch
                {
                    MessageBox.Show($"Could not open custom hat at '{path}'. Make sure that the file can be read.",
                        Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return default;
                }
            }
        }

        private static HatSettings GetDefaultHatSettings(TomlSettings s = null)
        {
            using (var ms = new MemoryStream(Resources.DefaultHatConfig))
                return Toml.ReadStream<HatSettings>(ms, s);
        }

        private void PostRenderEvent(GooseEntity goose, Graphics g)
        {
            // I can't actually do anything useful per goose entity, because there isn't a way to
            //   register info when a goose entity is created

            if (hat.Image == null) return;

            var direction = goose.direction + 90f; ;
            var headPoint = goose.rig.neckHeadPoint;

            var vertBase = (Rig.HeadRadius1 / 2) * hat.Settings.HatPosition.Value;

            var bmp = hat.Image;
            var baseOffset = Rig.HeadRadius1 * hat.Settings.HorizontalSize.Value / 2;
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

            g.DrawImage(hat.Image, asPoints);
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
