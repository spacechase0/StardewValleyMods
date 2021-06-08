using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using System.Xml.Serialization;
using Harmony;
using Newtonsoft.Json;
using Spacechase.Shared.Harmony;
using SpaceCore.Framework;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Overrides
{
    /// <summary>Applies Harmony patches to <see cref="SaveGame"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "The naming is determined by Harmony.")]
    internal class SaveGamePatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages the custom save serialization.</summary>
        private static SerializerManager SerializerManager;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="serializerManager">Manages the custom save serialization.</param>
        public SaveGamePatcher(SerializerManager serializerManager)
        {
            SaveGamePatcher.SerializerManager = serializerManager;
        }

        /// <inheritdoc />
        public override void Apply(HarmonyInstance harmony, IMonitor monitor)
        {
            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.GetSerializer)),
                prefix: this.GetHarmonyMethod(nameof(Before_GetSerializer))
            );

            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.Load)),
                prefix: this.GetHarmonyMethod(nameof(Before_Load), priority: Priority.First)
            );

            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.Save)),
                prefix: this.GetHarmonyMethod(nameof(Before_Save))
            );

            harmony.Patch(
                original: this.RequireMethod<SaveGame>(nameof(SaveGame.loadDataToLocations)),
                prefix: this.GetHarmonyMethod(nameof(Before_LoadDataToLocations))
            );

            foreach (var method in SaveGamePatcher.GetLoadEnumeratorMethods())
            {
                harmony.Patch(
                    original: method,
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_GetLoadEnumerator))
                );
            }

            foreach (var method in SaveGamePatcher.GetSaveEnumeratorMethods())
            {
                harmony.Patch(
                    original: method,
                    transpiler: this.GetHarmonyMethod(nameof(Transpile_GetSaveEnumerator))
                );
            }
        }

        /// <summary>Get the <see cref="SaveGame.getLoadEnumerator"/> methods that should be patched.</summary>
        public static IEnumerable<MethodBase> GetLoadEnumeratorMethods()
        {
            List<MethodBase> ret = new List<MethodBase>();
            foreach (var type in typeof(SaveGame).GetNestedTypes(BindingFlags.NonPublic))
            {
                if (type.Name.Contains("getLoadEnumerator"))
                {
                    // Primary enumerator method
                    ret.Add(type.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
                }
                else if (type.Name.Contains("<>") && type.Name != "<>c")
                {
                    foreach (var meth in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (meth.Name.Contains("getLoadEnumerator"))
                        {
                            // A lambda inside the enumerator
                            ret.Add(meth);
                            break;
                        }
                    }
                }
            }
            if (ret.Count != 2)
            {
                Log.warn($"Found {ret.Count} transpiler targets, expected 2");
                foreach (var meth in ret)
                {
                    Log.trace("\t" + meth.Name + " " + meth);
                }
            }
            ret.Add(PatchHelper.RequireMethod<LoadGameMenu>("FindSaveGames"));
            return ret;
        }

        /// <summary>Get the <see cref="SaveGame.getSaveEnumerator"/> methods that should be patched.</summary>
        public static IEnumerable<MethodBase> GetSaveEnumeratorMethods()
        {
            List<MethodBase> ret = new List<MethodBase>();
            foreach (var type in typeof(SaveGame).GetNestedTypes(BindingFlags.NonPublic))
            {
                if (type.Name.Contains("getSaveEnumerator"))
                {
                    // Primary enumerator method
                    ret.Add(type.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
                }
            }
            if (ret.Count != 1)
            {
                Log.warn($"Found {ret.Count} transpiler targets, expected 1");
                foreach (var meth in ret)
                {
                    Log.trace("\t" + meth.Name + " " + meth);
                }
            }
            return ret;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Patches
        ****/
        /// <summary>The method to call before <see cref="SaveGame.GetSerializer"/>.</summary>
        private static bool Before_GetSerializer(Type type, ref XmlSerializer __result)
        {
            __result = SaveGamePatcher.SerializerManager.InitializeSerializer(type);
            return false;
        }

        /// <summary>The method to call before <see cref="SaveGame.Load"/>.</summary>
        private static void Before_Load(string filename)
        {
            SaveGamePatcher.SerializerManager.InitializeSerializers();
            SaveGamePatcher.SerializerManager.loadFileContext = filename;
        }

        /// <summary>The method to call before <see cref="SaveGame.Save"/>.</summary>
        private static void Before_Save()
        {
            // Save is done as well for the case of creating a new save without loading one

            SaveGamePatcher.SerializerManager.InitializeSerializers();
        }

        /// <summary>The method to call before <see cref="SaveGame.loadDataToLocations"/>.</summary>
        private static void Before_LoadDataToLocations(List<GameLocation> gamelocations)
        {
            foreach (var loc in gamelocations)
            {
                var objs = loc.netObjects.Pairs.ToArray();
                foreach (var pair in objs)
                {
                    if (pair.Value == null)
                        loc.netObjects.Remove(pair.Key);
                }
                var tfs = loc.terrainFeatures.Pairs.ToArray();
                foreach (var pair in tfs)
                {
                    if (pair.Value == null)
                        loc.terrainFeatures.Remove(pair.Key);
                }
            }
        }

        /// <summary>The method which transpiles <see cref="SaveGame.getLoadEnumerator"/>.</summary>
        internal static IEnumerable<CodeInstruction> Transpile_GetLoadEnumerator(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "Deserialize")
                {
                    insn.opcode = OpCodes.Call;
                    insn.operand = PatchHelper.RequireMethod<SaveGamePatcher>(nameof(DeserializeProxy));

                    // We'll need the file path too since we can't use the current save constant.
                    if (original.DeclaringType == typeof(LoadGameMenu))
                    {
                        newInsns.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));
                        newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
                    }
                    else
                    {
                        newInsns.Add(new CodeInstruction(OpCodes.Ldnull));
                        newInsns.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        /// <summary>The method which transpiles <see cref="SaveGame.getSaveEnumerator"/>.</summary>
        internal static IEnumerable<CodeInstruction> Transpile_GetSaveEnumerator(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            int skip = 0;

            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (skip > 0)
                {
                    --skip;
                    continue;
                }

                if (insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == "Serialize")
                {
                    insn.opcode = OpCodes.Call;
                    insn.operand = PatchHelper.RequireMethod<SaveGamePatcher>(nameof(SerializeProxy));

                    // So, I couldn't figure out how to get SerializeProxy to not write the start/end tags of SaveGame
                    // So instead, remove when SaveGame does it.
                    // Search a few instructions before since one occurs at a different distance than the other.
                    for (int i = 1; i < 6; ++i)
                    {
                        if (newInsns[newInsns.Count - i].opcode == OpCodes.Callvirt && (newInsns[newInsns.Count - i].operand as MethodInfo).Name == "WriteStartDocument")
                        {
                            newInsns.RemoveAt(newInsns.Count - i);
                            newInsns.RemoveAt(newInsns.Count - i);
                            skip = 2;
                            break;
                        }
                    }
                }
                newInsns.Add(insn);
            }

            return newInsns;
        }

        /****
        ** GetLoadEnumerator helpers
        ****/
        private static Type FindModType(string xmlType)
        {
            return SpaceCore.modTypes.SingleOrDefault(t => t.GetCustomAttribute<XmlTypeAttribute>().TypeName == xmlType);
        }

        private static void RestoreModNodes(XmlDocument doc, XmlNode node, List<KeyValuePair<string, string>> modNodes, string currPath = "")
        {
            var processed = new List<KeyValuePair<string, string>>();
            foreach (var modNode in modNodes)
            {
                if (!modNode.Key.StartsWith($"{currPath}/"))
                    continue;

                string idxStr = modNode.Key.Substring(currPath.Length + 1);
                if (!idxStr.Contains('/'))
                {
                    var newDoc = new XmlDocument();
                    newDoc.LoadXml(modNode.Value);

                    var attr = newDoc.DocumentElement.Attributes["xsi:type"];
                    if (attr == null || FindModType(attr.Value) == null)
                        continue;

                    var newNode = doc.ImportNode(newDoc.DocumentElement, true);

                    int idx = int.Parse(idxStr);
                    if (idx == 0)
                        node.PrependChild(newNode);
                    else
                        node.InsertAfter(newNode, node.ChildNodes[idx - 1]);

                    processed.Add(modNode);
                }
            }
            foreach (var p in processed)
                modNodes.Remove(p);

            for (int i = 0; i < node.ChildNodes.Count; ++i)
            {
                //Log.trace( "child " + i + "/" + node.ChildNodes.Count );
                RestoreModNodes(doc, node.ChildNodes[i] as XmlNode, modNodes, $"{currPath}/{i}");
            }
        }

        private static object DeserializeProxy(XmlSerializer serializer, Stream stream, string farmerPath, bool fromSaveGame)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);

            string filePath = null;
            if (fromSaveGame)
            {
                farmerPath = Path.Combine(Constants.SavesPath, SaveGamePatcher.SerializerManager.loadFileContext);
                if (serializer == SaveGame.farmerSerializer)
                    filePath = Path.Combine(farmerPath, SaveGamePatcher.SerializerManager.FarmerFilename);
                else
                    filePath = Path.Combine(farmerPath, SaveGamePatcher.SerializerManager.Filename);
            }
            else
                filePath = Path.Combine(Path.GetDirectoryName(farmerPath), SaveGamePatcher.SerializerManager.FarmerFilename);

            if (File.Exists(filePath))
            {
                List<KeyValuePair<string, string>> modNodes = null;
                modNodes = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(File.ReadAllText(filePath));
                if (modNodes != null)
                    RestoreModNodes(doc, doc, modNodes, "/1"); // <?xml ... ?> is 1
            }

            return serializer.Deserialize(new XmlTextReader(new StringReader(doc.OuterXml)));
        }

        /****
        ** GetSaveEnumerator helpers
        ****/
        private static bool FindAndRemoveModNodes(XmlNode node, List<KeyValuePair<string, string>> modNodes, string currPath = "")
        {
            if (node.HasChildNodes)
            {
                for (int i = node.ChildNodes.Count - 1; i >= 0; --i)
                {
                    var child = node.ChildNodes[i];
                    if (FindAndRemoveModNodes(child, modNodes, $"{currPath}/{i}"))
                    {
                        modNodes.Insert(0, new KeyValuePair<string, string>($"{currPath}/{i}", child.OuterXml));
                        node.RemoveChild(child);
                    }
                }
            }

            if (node.Attributes == null)
                return false;

            var attr = node.Attributes["xsi:type"];
            if (attr == null)
                return false;

            if (attr.Value.StartsWith("Mods_"))
                return true;
            return false;
        }

        private static void SerializeProxy(XmlSerializer serializer, XmlWriter origWriter, object obj)
        {
            //Log.trace( "Start serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
            using (var ms = new MemoryStream())
            {
                using var writer = XmlWriter.Create(ms, new XmlWriterSettings() { CloseOutput = false });

                serializer.Serialize(writer, obj);
                XmlDocument doc = new XmlDocument();
                ms.Position = 0;
                doc.Load(ms);

                var modNodes = new List<KeyValuePair<string, string>>();
                FindAndRemoveModNodes(doc, modNodes, "/1"); // <?xml ... ?> is /0

                doc.WriteContentTo(origWriter);
                if (serializer == SaveGame.farmerSerializer)
                    File.WriteAllText(Path.Combine(Constants.CurrentSavePath, SaveGamePatcher.SerializerManager.FarmerFilename), JsonConvert.SerializeObject(modNodes));
                else
                    File.WriteAllText(Path.Combine(Constants.CurrentSavePath, SaveGamePatcher.SerializerManager.Filename), JsonConvert.SerializeObject(modNodes));
                //Log.trace( "Mid serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
            }
            //Log.trace( "End serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
        }
    }
}
