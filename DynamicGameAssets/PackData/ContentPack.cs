using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DynamicGameAssets.Framework.ContentPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ContentPack : IAssetLoader
    {
        internal class ConfigModel
        {
            [JsonExtensionData]
            public Dictionary<string, JToken> Values = new();
        }

        internal IContentPack smapiPack;

        internal ISemanticVersion conditionVersion;

        /// <summary>The textures loaded from this content pack.</summary>
        private readonly IDictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

        /// <summary>The animations parsed from this content pack, indexed by the raw animation descriptor they were parsed from (e.g. <c>items16.png:1..2@333, items16.png:3@334</c>).</summary>
        private readonly IDictionary<string, TextureAnimation> TextureAnimationCache = new Dictionary<string, TextureAnimation>();

        protected internal Dictionary<string, CommonPackData> items = new Dictionary<string, CommonPackData>();

        protected internal List<BasePackData> others = new List<BasePackData>();

        internal Dictionary<ContentIndexPackData?, List<BasePackData>> enableIndex = new();

        private readonly List<ConfigPackData> configs = new();
        internal Dictionary<string, ConfigPackData> configIndex = new();
        internal ConfigModel currConfig = new();

        public ContentPack(IContentPack pack, int formatVer, ISemanticVersion condVer)
        {
            this.smapiPack = pack;

            if (pack.Manifest.UniqueID != "null")
            {
                this.conditionVersion = condVer;
                switch (formatVer)
                {
                    case 1: new ContentPackLoaderV1(this).Load(); break;
                    case 2: new ContentPackLoaderV2(this).Load(); break;
                    default:
                        throw new Exception("Invalid content pack format version: " + pack.Manifest.ExtraFields["DGA.FormatVersion"].ToString());
                }

                this.LoadConfig(); // TODO: Move this to pack loader as well, once it diverges
            }
        }

        public ContentPack(IContentPack pack)
            : this(pack, pack.Manifest.UniqueID == "null" ? 1 : int.Parse(pack.Manifest.ExtraFields["DGA.FormatVersion"].ToString()), pack.Manifest.UniqueID == "null" ? null : new SemanticVersion(pack.Manifest.ExtraFields["DGA.ConditionsFormatVersion"].ToString())) { }

        public List<CommonPackData> GetItems()
        {
            return new List<CommonPackData>(this.items.Values);
        }

        public CommonPackData Find(string item)
        {
            if (this.items.ContainsKey(item) && this.items[item].Enabled)
                return this.items[item];
            return null;
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            string path = asset.AssetName.Replace('\\', '/');
            string start = "DGA/" + this.smapiPack.Manifest.UniqueID + "/";
            if (!path.StartsWith(start) || !path.EndsWith(".png"))
                return false;
            return this.smapiPack.HasFile(path.Substring(start.Length));
        }

        public T Load<T>(IAssetInfo asset)
        {
            string path = asset.AssetName.Replace('\\', '/');
            string start = "DGA/" + this.smapiPack.Manifest.UniqueID + "/";
            if (!path.StartsWith(start) || !path.EndsWith(".png"))
                return default(T);
            return (T)(object)this.smapiPack.LoadAsset<Texture2D>(path.Substring(start.Length));
        }

        private void LoadConfig()
        {
            if (!this.smapiPack.HasFile("config-schema.json"))
                return;

            var gmcm = Mod.instance.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm == null)
                return;

            gmcm.UnregisterModConfig(this.smapiPack.Manifest);
            gmcm.RegisterModConfig(this.smapiPack.Manifest, this.ResetToDefaultConfig, () => this.smapiPack.WriteJsonFile("config.json", this.currConfig));
            gmcm.SetDefaultIngameOptinValue(this.smapiPack.Manifest, true);
            gmcm.RegisterParagraph(this.smapiPack.Manifest, "Note: If in-game, config values may not take effect until the next in-game day.");

            var readConfig = this.smapiPack.ReadJsonFile<ConfigModel>("config.json");
            bool writeConfig = false;
            if (readConfig == null)
            {
                readConfig = new ConfigModel();
                writeConfig = true;
            }

            var data = this.smapiPack.LoadAsset<List<ConfigPackData>>("config-schema.json") ?? new List<ConfigPackData>();
            foreach (var d in data)
            {
                Log.Trace($"Loading config entry {d.Name}...");
                this.configs.Add(d);

                gmcm.StartNewPage(this.smapiPack.Manifest, d.OnPage);
                switch (d.ElementType)
                {
                    case ConfigPackData.ConfigElementType.Label:
                        if (d.PageToGoTo != null)
                            gmcm.RegisterPageLabel(this.smapiPack.Manifest, d.Name, d.Description, d.PageToGoTo);
                        else
                            gmcm.RegisterLabel(this.smapiPack.Manifest, d.Name, d.Description);
                        break;

                    case ConfigPackData.ConfigElementType.Paragraph:
                        gmcm.RegisterParagraph(this.smapiPack.Manifest, d.Name);
                        break;

                    case ConfigPackData.ConfigElementType.Image:
                        gmcm.RegisterImage(this.smapiPack.Manifest, Path.Combine("DGA", this.smapiPack.Manifest.UniqueID, d.ImagePath), d.ImageRect, d.ImageScale);
                        break;

                    case ConfigPackData.ConfigElementType.ConfigOption:
                        string key = d.Name;
                        if (!string.IsNullOrEmpty(d.OnPage))
                            key = d.OnPage + "/" + key;
                        if (this.configIndex.ContainsKey(key))
                        {
                            Log.Error("Duplicate config key: " + key);
                            continue;
                        }

                        this.configIndex.Add(key, d);
                        this.currConfig.Values.Add(key, readConfig.Values.ContainsKey(key) ? readConfig.Values[key] : d.DefaultValue);

                        string[] valid = d.ValidValues?.Split(',')?.Select(s => s.Trim())?.ToArray();
                        switch (d.ValueType)
                        {
                            case ConfigPackData.ConfigValueType.Boolean:
                                gmcm.RegisterSimpleOption(this.smapiPack.Manifest, d.Name, d.Description, () => this.currConfig.Values[key].ToString() == "true" ? true : false, (v) => this.currConfig.Values[key] = v ? "true" : "false");
                                break;

                            case ConfigPackData.ConfigValueType.Integer:
                                if (valid?.Length == 2)
                                    gmcm.RegisterClampedOption(this.smapiPack.Manifest, d.Name, d.Description, () => int.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString(), int.Parse(valid[0]), int.Parse(valid[1]));
                                else if (valid?.Length == 3)
                                    gmcm.RegisterClampedOption(this.smapiPack.Manifest, d.Name, d.Description, () => int.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString(), int.Parse(valid[0]), int.Parse(valid[1]), int.Parse(valid[2]));
                                else
                                    gmcm.RegisterSimpleOption(this.smapiPack.Manifest, d.Name, d.Description, () => int.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString());
                                break;

                            case ConfigPackData.ConfigValueType.Float:
                                if (valid?.Length == 2)
                                    gmcm.RegisterClampedOption(this.smapiPack.Manifest, d.Name, d.Description, () => float.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString(), float.Parse(valid[0]), float.Parse(valid[1]));
                                else if (valid?.Length == 3)
                                    gmcm.RegisterClampedOption(this.smapiPack.Manifest, d.Name, d.Description, () => float.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString(), float.Parse(valid[0]), float.Parse(valid[1]), float.Parse(valid[2]));
                                else
                                    gmcm.RegisterSimpleOption(this.smapiPack.Manifest, d.Name, d.Description, () => float.Parse(this.currConfig.Values[key].ToString()), (v) => this.currConfig.Values[key] = v.ToString());
                                break;

                            case ConfigPackData.ConfigValueType.String:
                                if (valid?.Length > 1)
                                    gmcm.RegisterChoiceOption(this.smapiPack.Manifest, d.Name, d.Description, () => this.currConfig.Values[key].ToString(), (v) => this.currConfig.Values[key] = v, valid);
                                else
                                    gmcm.RegisterSimpleOption(this.smapiPack.Manifest, d.Name, d.Description, () => this.currConfig.Values[key].ToString(), (v) => this.currConfig.Values[key] = v);
                                break;
                        }
                        break;
                }
            }

            if (writeConfig)
            {
                this.smapiPack.WriteJsonFile("config.json", this.currConfig);
            }
        }

        private void ResetToDefaultConfig()
        {
            foreach (var config in this.configs)
            {
                if (!this.currConfig.Values.ContainsKey(config.Name))
                    this.currConfig.Values.Add(config.Name, config.DefaultValue);
                else
                    this.currConfig.Values[config.Name] = config.DefaultValue;
            }
        }

        /// <summary>Get the current frame for an animation descriptor.</summary>
        /// <param name="descriptor">The animation descriptor. See 'texture animations' in the author guide for a description of the format.</param>
        internal TextureAnimationFrame GetTextureFrame(string descriptor)
        {
            if (string.IsNullOrWhiteSpace(descriptor))
                return null;

            // parse animation frames
            if (!this.TextureAnimationCache.TryGetValue(descriptor, out TextureAnimation animation))
                this.TextureAnimationCache[descriptor] = animation = TextureAnimation.ParseFrom(descriptor);

            // get current frame
            return animation.GetCurrentFrame(Mod.State.AnimationFrames);
        }

        internal TexturedRect GetMultiTexture(string[] paths, int decider, int xSize, int ySize)
        {
            if (paths == null)
                return new TexturedRect() { Texture = Game1.staminaRect, Rect = null };

            return this.GetTexture(paths[decider % paths.Length], xSize, ySize);
        }

        /// <summary>Get the texture and source rectangle to show for an animation descriptor during the current game tick.</summary>
        /// <param name="descriptor">The animation descriptor. See 'texture animations' in the author guide for a description of the format.</param>
        /// <param name="xSize">The sprite width in pixels.</param>
        /// <param name="ySize">The sprite height in pixels.</param>
        internal TexturedRect GetTexture(string descriptor, int xSize, int ySize)
        {
            // get current animation frame
            TextureAnimationFrame frame = this.GetTextureFrame(descriptor);
            if (frame == null)
                return new TexturedRect { Texture = Game1.staminaRect, Rect = null };

            // load texture
            if (!this.TextureCache.TryGetValue(frame.FilePath, out Texture2D texture))
            {
                if (this.smapiPack.HasFile(frame.FilePath))
                {
                    try
                    {
                        texture = this.smapiPack.LoadAsset<Texture2D>(frame.FilePath);
                        texture.Name = PathUtilities.NormalizeAssetName($"DGA/{this.smapiPack.Manifest.UniqueID}/{frame.FilePath}");
                    }
                    catch
                    {
                        texture = null;
                    }
                }
                else
                {
                    Log.Warn($"No such \"{frame.FilePath}\" in {this.smapiPack.Manifest.Name} ({this.smapiPack.Manifest.UniqueID})!");
                    texture = null;
                }

                texture ??= Game1.staminaRect;
                this.TextureCache[frame.FilePath] = texture;
            }

            // build texture + source rectangle
            int spriteColumns = texture.Width / xSize;
            int index = frame.SpriteIndex;
            return new TexturedRect
            {
                Texture = texture,
                Rect = new Rectangle(index % spriteColumns * xSize, index / spriteColumns * ySize, xSize, ySize)
            };
        }
    }
}
