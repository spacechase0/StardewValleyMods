using System;
using System.Collections.Generic;
using System.IO;
using DynamicGameAssets.Framework.ContentPacks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace DynamicGameAssets.PackData
{
    public class ContentPack
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

        protected internal Dictionary<string, CommonPackData> items = new();

        protected internal List<BasePackData> others = new();

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

        public IManifest GetManifest()
        {
            return smapiPack.Manifest;
        }

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

        public bool TryLoad(AssetRequestedEventArgs e)
        {
            string localPath = e.NameWithoutLocale.BaseName.Substring(this.smapiPack.Manifest.UniqueID.Length + 5);
            if (this.smapiPack.HasFile(localPath))
            {
                e.LoadFrom(() => this.smapiPack.ModContent.Load<Texture2D>(localPath), AssetLoadPriority.Exclusive);
                return true;
            }
            return false;
        }

        private void LoadConfig()
        {
            if (!this.smapiPack.HasFile("config-schema.json"))
                return;

            var configMenu = Mod.instance.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu == null)
                return;

            configMenu.Unregister(this.smapiPack.Manifest);
            configMenu.Register(
                mod: this.smapiPack.Manifest,
                reset: this.ResetToDefaultConfig,
                save: () => this.smapiPack.WriteJsonFile("config.json", this.currConfig)
            );
            configMenu.AddParagraph(
                mod: this.smapiPack.Manifest,
                text: () => "Note: If in-game, config values may not take effect until the next in-game day."
            );

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

                configMenu.AddPage(
                    mod: this.smapiPack.Manifest,
                    pageId: d.OnPage
                );
                switch (d.ElementType)
                {
                    case ConfigPackData.ConfigElementType.Label:
                        if (d.PageToGoTo != null)
                        {
                            configMenu.AddPageLink(
                                mod: this.smapiPack.Manifest,
                                pageId: d.PageToGoTo,
                                text: () => d.Name,
                                tooltip: () => d.Description
                            );
                        }
                        else
                        {
                            configMenu.AddSectionTitle(
                                mod: this.smapiPack.Manifest,
                                text: () => d.Name,
                                tooltip: () => d.Description
                            );
                        }

                        break;

                    case ConfigPackData.ConfigElementType.Paragraph:
                        configMenu.AddParagraph(
                            mod: this.smapiPack.Manifest,
                            text: () => d.Name
                        );
                        break;

                    case ConfigPackData.ConfigElementType.Image:
                        configMenu.AddImage(
                            mod: this.smapiPack.Manifest,
                            texture: () => Game1.content.Load<Texture2D>(Path.Combine("DGA", this.smapiPack.Manifest.UniqueID, d.ImagePath)),
                            texturePixelArea: d.ImageRect,
                            scale: d.ImageScale
                        );
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

                        string[] valid = d.ValidValues?.Split(',', StringSplitOptions.TrimEntries);
                        switch (d.ValueType)
                        {
                            case ConfigPackData.ConfigValueType.Boolean:
                                configMenu.AddBoolOption(
                                    mod: this.smapiPack.Manifest,
                                    name: () => d.Name,
                                    tooltip: () => d.Description,
                                    getValue: () => this.currConfig.Values[key].ToString() == "true",
                                    setValue: value => this.currConfig.Values[key] = value ? "true" : "false"
                                );
                                break;

                            case ConfigPackData.ConfigValueType.Integer:
                                configMenu.AddNumberOption(
                                    mod: this.smapiPack.Manifest,
                                    name: () => d.Name,
                                    tooltip: () => d.Description,
                                    getValue: () => int.Parse(this.currConfig.Values[key].ToString()),
                                    setValue: value => this.currConfig.Values[key] = value.ToString(),
                                    min: valid?.Length > 2 ? int.Parse(valid[0]) : null,
                                    max: valid?.Length > 2 ? int.Parse(valid[1]) : null,
                                    interval: valid?.Length > 3 ? int.Parse(valid[2]) : null
                                );
                                break;

                            case ConfigPackData.ConfigValueType.Float:
                                configMenu.AddNumberOption(
                                    mod: this.smapiPack.Manifest,
                                    name: () => d.Name,
                                    tooltip: () => d.Description,
                                    getValue: () => float.Parse(this.currConfig.Values[key].ToString()),
                                    setValue: value => this.currConfig.Values[key] = value.ToString(),
                                    min: valid?.Length > 2 ? float.Parse(valid[0]) : null,
                                    max: valid?.Length > 2 ? float.Parse(valid[1]) : null,
                                    interval: valid?.Length > 3 ? float.Parse(valid[2]) : null
                                );
                                break;

                            case ConfigPackData.ConfigValueType.String:
                                configMenu.AddTextOption(
                                    mod: this.smapiPack.Manifest,
                                    name: () => d.Name,
                                    tooltip: () => d.Description,
                                    getValue: () => this.currConfig.Values[key].ToString(),
                                    setValue: value => this.currConfig.Values[key] = value,
                                    allowedValues: valid?.Length > 1 ? valid : null
                                );
                                break;
                        }
                        break;
                }
            }

            if (writeConfig)
                this.smapiPack.WriteJsonFile("config.json", this.currConfig);
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
                        if (texture.Width < xSize)
                        {
                            Log.Warn($"Underwidth texture in \"{frame.FilePath}\" in {this.smapiPack.Manifest.Name} ({this.smapiPack.Manifest.UniqueID})!");
                        }
                        texture.Name = PathUtilities.NormalizeAssetName($"DGA/{this.smapiPack.Manifest.UniqueID}/{frame.FilePath}");
                    }
                    catch
                    {
                        texture = null;
                    }
                }
                else
                {
                    Log.Error($"No such \"{frame.FilePath}\" in {this.smapiPack.Manifest.Name} ({this.smapiPack.Manifest.UniqueID})!");
                    texture = null;
                }

                texture ??= Game1.staminaRect;
                
                this.TextureCache[frame.FilePath] = texture;
            }

            // build texture + source rectangle
            int spriteColumns = texture.Width > xSize ? texture.Width / xSize : 1;
            int index = frame.SpriteIndex;
            int rectWidth = texture.Width > xSize ? xSize : texture.Width;
            int rectHeight = texture.Height > ySize ? ySize : texture.Height;
            return new TexturedRect
            {
                Texture = texture,
                Rect = new Rectangle(index % spriteColumns * xSize, index / spriteColumns * ySize, rectWidth, rectHeight)
            };
        }
    }
}
