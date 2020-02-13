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
        private Bitmap hatImage = null;

        private HatSettings settings = null;

        public const string Name = nameof(HatGoos);

        public void Init()
        {
            InjectionPoints.PostModsLoaded += PostModsLoaded;
            InjectionPoints.PostRenderEvent += PostRenderEvent;
        }

        private void PostModsLoaded()
        {
            try
            {
                var tomlSettings = TomlSettings.Create(c => c
                    .AllowImplicitConversions(TomlSettings.ConversionSets.All)
                    .ConfigureType<float?>(b => b
                        .WithConversionFor<TomlFloat>(v => v
                            .ToToml(f => f.Value)
                            .FromToml(t => (float)t.Value))));

                // load configs
                var path = Path.Combine(API.Helper.getModDirectory(this), "config.toml");
                if (File.Exists(path))
                    config = Toml.ReadFile<Config>(path, tomlSettings);
                else
                    config = new Config();

                if (config.HatMode == Config.HatType.Custom && !File.Exists(config.CustomHatPath))
                    config.HatMode = Config.HatType.Default;

                Toml.WriteFile(config, path, tomlSettings);

                // initialize hat image
                switch (config.HatMode)
                {
                    case Config.HatType.None:
                        hatImage = new Bitmap(1, 1);
                        hatImage.SetPixel(0, 0, Color.Transparent);
                        break;
                    case Config.HatType.Default:
                        hatImage = Resources.DefaultImage;
                        settings = GetDefaultHatSettings(tomlSettings);
                        break;
                    case Config.HatType.Custom:
                        try
                        {
                            using (var zf = ZipFile.OpenRead(config.CustomHatPath))
                            {
                                var entry = zf.GetEntry("hat.toml");
                                if (entry == null)
                                {
                                    MessageBox.Show($"Custom hat at '{config.CustomHatPath}' is invalid. It does not have 'hat.toml'.",
                                        Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                }

                                settings = Toml.ReadStream<HatSettings>(entry.Open(), tomlSettings)
                                    .WithOverride(config.Overrides);

                                var imageEntry = zf.GetEntry(settings.ImageName);
                                if (imageEntry == null)
                                {
                                    MessageBox.Show($"Could not find image '{settings.ImageName}' in hat '{config.CustomHatPath}'.",
                                        Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                }

                                hatImage = new Bitmap(imageEntry.Open());
                            }
                        }
                        catch (NotSupportedException)
                        {
                            try
                            {
                                hatImage = new Bitmap(config.CustomHatPath);
                                settings = GetDefaultHatSettings(tomlSettings).WithOverride(config.Overrides);
                            }
                            catch
                            {
                                MessageBox.Show($"Could not open custom hat at '{config.CustomHatPath}'. Make sure that the file can be read.",
                                    Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error initializing {Name}: {e}",
                    Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                hatImage = null; settings = null;
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

            if (hatImage == null || settings == null) return;

            var direction = goose.direction + 90f; ;
            var headPoint = goose.rig.neckHeadPoint;

            var vertBase = (Rig.HeadRadius1 / 2) * settings.HatPosition.Value;

            var bmp = hatImage;
            var baseOffset = Rig.HeadRadius1 * settings.HorizontalSize.Value / 2;
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
