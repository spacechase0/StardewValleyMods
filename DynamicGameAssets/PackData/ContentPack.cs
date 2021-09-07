using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DynamicGameAssets.PackData.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
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

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        private Dictionary<string, int[]> animInfo = new Dictionary<string, int[]>(); // Index is full animation descriptor (items16.png:1@333/items16.png:2@333/items16.png:3@334), value is [frameDur1, frameDur2, frameDur3, ..., totalFrameDur]

        protected internal Dictionary<string, CommonPackData> items = new Dictionary<string, CommonPackData>();

        protected internal List<BasePackData> others = new List<BasePackData>();

        internal Dictionary<ContentIndexPackData?, List<BasePackData>> enableIndex = new();

        private List<ConfigPackData> configs = new();
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

        internal string GetTextureFrame(string path)
        {
            string[] frames = path.Split(',');
            int[] frameDurs = null;
            if (this.animInfo.ContainsKey(path))
                frameDurs = this.animInfo[path];
            else
            {
                int total = 0;
                var frameData = new List<int>();
                for (int i = 0; i < frames.Length; ++i)
                {
                    int dur = 1;
                    int at = frames[i].IndexOf('@');
                    if (at != -1)
                        dur = int.Parse(frames[i].Substring(at + 1).Trim());

                    frameData.Add(dur);
                    total += dur;
                }
                frameData.Add(total);
                this.animInfo.Add(path, frameDurs = frameData.ToArray());
            }

            int spot = Mod.State.AnimationFrames % frameDurs[frames.Length];
            for (int i = 0; i < frames.Length; ++i)
            {
                spot -= frameDurs[i];
                if (spot < 0)
                    return frames[i].Trim();
            }

            throw new Exception("This should never happen (" + path + ")");
        }

        internal TexturedRect GetMultiTexture(string[] paths, int decider, int xSize, int ySize)
        {
            if (paths == null)
                return new TexturedRect() { Texture = Game1.staminaRect, Rect = null };

            return this.GetTexture(paths[decider % paths.Length], xSize, ySize);
        }

        internal TexturedRect GetTexture(string path_, int xSize, int ySize)
        {
            if (path_ == null)
                return new TexturedRect() { Texture = Game1.staminaRect, Rect = null };

            string path = path_;
            if (path.Contains(','))
            {
                return this.GetTexture(this.GetTextureFrame(path), xSize, ySize);
            }
            else
            {
                int at = path.IndexOf('@');
                if (at != -1)
                    path = path.Substring(0, at);

                int colon = path.IndexOf(':');
                string pathItself = colon == -1 ? path : path.Substring(0, colon);
                if (this.textures.ContainsKey(pathItself))
                {
                    if (colon == -1)
                        return new TexturedRect() { Texture = this.textures[pathItself], Rect = null };
                    else
                    {
                        int sections = this.textures[pathItself].Width / xSize;
                        int ind = int.Parse(path.Substring(colon + 1));

                        return new TexturedRect()
                        {
                            Texture = this.textures[pathItself],
                            Rect = new Rectangle(ind % sections * xSize, ind / sections * ySize, xSize, ySize)
                        };
                    }
                }

                if (!this.smapiPack.HasFile(pathItself))
                    Log.Warn("No such \"" + pathItself + "\" in " + this.smapiPack.Manifest.Name + " (" + this.smapiPack.Manifest.UniqueID + ")!");

                Texture2D t;
                try
                {
                    t = this.smapiPack.LoadAsset<Texture2D>(pathItself);
                    t.Name = Path.Combine("DGA", this.smapiPack.Manifest.UniqueID, pathItself).Replace('\\', '/');
                }
                catch (Exception e)
                {
                    t = Game1.staminaRect;
                }

                this.textures.Add(pathItself, t);

                return this.GetTexture(path_, xSize, ySize);
            }
        }
    }
}
