using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Movies;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley.Objects;

namespace AutocorrectValley;

public class Configuration
{
    public enum HingedAmount
    {
        SemiHinged,
        Unhinged,
    }

    public HingedAmount Hingedness { get; set; } = HingedAmount.Unhinged;
}

public class Mod : StardewModdingAPI.Mod
{
    private static Mod instance;

    private Configuration Config;
    private SymSpell sym;

    public override void Entry(IModHelper helper)
    {
        instance = this;
        Log.Monitor = Monitor;
        Config = Helper.ReadConfig<Configuration>();
        I18n.Init(Helper.Translation);

        sym = new(100000);
        if (!sym.LoadDictionary(Path.Combine(Helper.DirectoryPath, "assets", "frequency_dictionary_en_82_765.txt"), 0, 1))
        {
            Log.Error("Failed to read dictionary!");
            return;
        }

        Helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;
        Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
    }

    private string CorruptAutocorrect(string str)
    {
        string[] terms = str.Split(' ');

        StringBuilder result = new( str.Length );
        for (int i = 0; i < terms.Length; i++)
        {
            var suggestions = sym.Lookup(terms[i], SymSpell.Verbosity.All, 2, true);

            switch (Config.Hingedness)
            {
                case Configuration.HingedAmount.SemiHinged:
                    {
                        var replacement = suggestions.Where(si => !si.term.EqualsIgnoreCase(terms[i]) && !si.term.EqualsIgnoreCase(terms[i] + 's')).FirstOrDefault();
                        result.Append(replacement?.term ?? terms[i]);
                    }
                    break;

                case Configuration.HingedAmount.Unhinged:
                    {
                        var replacement = suggestions.Where(si => !si.term.EqualsIgnoreCase(terms[i]) && !si.term.EqualsIgnoreCase(terms[i] + 's')).LastOrDefault();
                        result.Append(replacement?.term ?? terms[i]);
                    }
                    break;
            }

            if (i < terms.Length - 1)
            {
                result.Append(' ');
            }
        }

        return result.ToString();
    }

    private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
    {
        var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (gmcm != null)
        {
            gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config), true);
            gmcm.AddTextOption(ModManifest, () => Config.Hingedness.ToString(), (val) => Config.Hingedness = Enum.Parse<Configuration.HingedAmount>(val), I18n.Config_Hingedness_Name, I18n.Config_Hingedness_Description, [Configuration.HingedAmount.SemiHinged.ToString(), Configuration.HingedAmount.Unhinged.ToString()], (val) =>
            {
                switch (Enum.Parse<Configuration.HingedAmount>(val))
                {
                    case Configuration.HingedAmount.SemiHinged: return I18n.Config_Hingedness_Value_SemiHinged();
                    case Configuration.HingedAmount.Unhinged: return I18n.Config_Hingedness_Value_Unhinged();
                }
                return "???";
            });
        }
    }

    private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
    {
        // These all use e.Name instead of e.NameWithoutLocale because only english is supported
        if (e.Name.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, BigCraftableData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Boots"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, string>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key];
                    string[] toks = val.Split('/');
                    if (toks[6].StartsWith("[LocalizedText") && toks[6].EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    toks[6] = CorruptAutocorrect(toks[6]);
                    data[key] = string.Join('/', toks);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Concessions"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, ConcessionItemData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Furniture"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, string>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key];
                    string[] toks = val.Split('/');
                    if (toks[7].StartsWith("[LocalizedText") && toks[7].EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    toks[7] = CorruptAutocorrect(toks[7]);
                    data[key] = string.Join('/', toks);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/hats"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, string>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key];
                    string[] toks = val.Split('/');
                    if (toks[5].StartsWith("[LocalizedText") && toks[5].EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    toks[5] = CorruptAutocorrect(toks[5]);
                    data[key] = string.Join('/', toks);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Mannequins"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, MannequinData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, ObjectData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Pants"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, PantsData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Shirts"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, ShirtData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Tools"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, ToolData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Trinkets"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, TrinketData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Data/Weapons"))
        {
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, WeaponData>().Data;
                foreach (var key in data.Keys)
                {
                    string val = data[key].DisplayName;
                    if (val.StartsWith("[LocalizedText") && val.EndsWith(']'))
                    {
                        // These are vanilla names and handled elsewhere.
                        // This block is still here so it works with modded names, most of which work differently using Content Patcher features.
                        continue;
                    }
                    data[key].DisplayName = CorruptAutocorrect(val);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
        else if (e.Name.IsEquivalentTo("Strings/1_6_Strings") ||
                 e.Name.IsEquivalentTo("Strings/BigCraftables") ||
                 e.Name.IsEquivalentTo("Strings/Furniture") ||
                 e.Name.IsEquivalentTo("Strings/MovieConcessions") ||
                 e.Name.IsEquivalentTo("Strings/Objects") ||
                 e.Name.IsEquivalentTo("Strings/Pants") ||
                 e.Name.IsEquivalentTo("Strings/Shirts") ||
                 e.Name.IsEquivalentTo("Strings/Weapons"))
        {
            bool doAllKeys = e.Name.IsEquivalentTo("Strings/Furniture");
            e.Edit(ad =>
            {
                var data = ad.AsDictionary<string, string>().Data;
                foreach (var key in data.Keys)
                {
                    if (!doAllKeys && !key.EndsWith("_Name"))
                        continue;
                    data[key] = CorruptAutocorrect(data[key]);
                }
            }, StardewModdingAPI.Events.AssetEditPriority.Late + 1000);
        }
    }
}
