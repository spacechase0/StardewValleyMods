using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HarmonyLib;
using Newtonsoft.Json;
using Spacechase.Shared.Patching;
using SpaceCore.Framework;
using SpaceCore.Framework.Serialization;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;

namespace SpaceCore.Patches
{
    /// <summary>Applies Harmony patches to <see cref="SaveGame"/>.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = DiagnosticMessages.NamedForHarmony)]
    internal class SaveGamePatcher : BasePatcher
    {
        /*********
        ** Fields
        *********/
        /// <summary>Manages the custom save serialization.</summary>
        internal static SerializerManager SerializerManager;


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
        public override void Apply(Harmony harmony, IMonitor monitor)
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
                original: this.RequireMethod<SaveGameMenu>(nameof(SaveGameMenu.update)),
                transpiler: this.GetHarmonyMethod(nameof(Transpile_SaveGameMenuUpdate))
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
                Log.Warn($"{nameof(GetLoadEnumeratorMethods)}: Found {ret.Count} transpiler targets, expected 2");
                foreach (var meth in ret)
                {
                    Log.Trace("\t" + meth.Name + " " + meth);
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
                Log.Warn($"{nameof(GetSaveEnumeratorMethods)}: Found {ret.Count} transpiler targets, expected 1");
                foreach (var meth in ret)
                {
                    Log.Trace("\t" + meth.Name + " " + meth);
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
            SaveGamePatcher.SerializerManager.LoadFileContext = filename;
        }

        /// <summary>The method to call before <see cref="SaveGame.Save"/>.</summary>
        private static void Before_Save()
        {
            // Save is done as well for the case of creating a new save without loading one

            SaveGamePatcher.SerializerManager.InitializeSerializers();
        }

        /// <summary>The method to call before <see cref="SaveGame.loadDataToLocations"/>.</summary>
        private static void Before_LoadDataToLocations(List<GameLocation> fromLocations)
        {
            foreach (var loc in fromLocations)
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

        /// <summary>The method which transpiles <see cref="SaveGame.Save"/>.</summary>
        internal static IEnumerable<CodeInstruction> Transpile_SaveGameMenuUpdate(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> newInsns = new();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Call && insn.operand is MethodInfo minfo && minfo.DeclaringType == typeof(SaveGame) && minfo.Name == nameof(SaveGame.Save))
                    newInsns.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SaveGamePatcher), nameof(Before_Save))));
                newInsns.Add(insn);
            }
            return newInsns;
        }

        /// <summary>The method which transpiles <see cref="SaveGame.getLoadEnumerator"/>.</summary>
        internal static IEnumerable<CodeInstruction> Transpile_GetLoadEnumerator(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            var newInsns = new List<CodeInstruction>();
            foreach (var insn in insns)
            {
                if (insn.opcode == OpCodes.Callvirt && (insn.operand as MethodInfo).Name == nameof(XmlSerializer.Deserialize))
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
            return SpaceCore.ModTypes.SingleOrDefault(t => t.GetCustomAttribute<XmlTypeAttribute>().TypeName == xmlType);
        }

        /// <summary>Recursively restore custom mod types in the save XML.</summary>
        /// <param name="doc">The XML document to scan.</param>
        /// <param name="node">The subtree node from which to scan.</param>
        /// <param name="modNodes">The custom mod elements to insert.</param>
        /// <param name="curPath">The <see cref="OptimizedModNode.Path"/> value for the current subtree root.</param>
        private static void RestoreModNodes(XmlDocument doc, XmlNode node, OptimizedModNodeList modNodes, string curPath)
        {
            // skip subtree if it doesn't contain any mod nodes
            if (!modNodes.AncestorPaths.Contains(curPath))
                return;

            // insert elements into this node
            if (modNodes.ModNodesByParent.TryGetValue(curPath, out OptimizedModNode[] nodes))
            {
                foreach (OptimizedModNode modNode in nodes.OrderBy(p => p.Index))
                {
                    // load XML element to insert
                    var newDoc = new XmlDocument();
                    newDoc.LoadXml(modNode.XmlNode);

                    // skip if the mod isn't installed
                    XmlAttribute attr = newDoc.DocumentElement.Attributes["xsi:type"];
                    if (attr == null || SaveGamePatcher.FindModType(attr.Value) == null)
                        continue;

                    // insert mod node
                    var newNode = doc.ImportNode(newDoc.DocumentElement, true);
                    if (modNode.Index == 0)
                        node.PrependChild(newNode);
                    else
                        node.InsertAfter(newNode, node.ChildNodes[modNode.Index - 1]);
                }
            }

            // scan children
            for (int i = 0; i < node.ChildNodes.Count; ++i)
                SaveGamePatcher.RestoreModNodes(doc: doc, node: node.ChildNodes[i], modNodes: modNodes, curPath: $"{curPath}/{i}");
        }

        private static object DeserializeProxy(XmlSerializer serializer, Stream stream, string farmerPath, bool fromSaveGame)
        {
            // load XML
            XmlDocument doc = new();
            doc.Load(stream);

            // get path to serialized SpaceCore data file
            string filePath;
            if (fromSaveGame)
            {
                farmerPath = Path.Combine(Constants.SavesPath, SaveGamePatcher.SerializerManager.LoadFileContext);
                string filename = serializer == SaveGame.farmerSerializer
                    ? SaveGamePatcher.SerializerManager.FarmerFilename
                    : SaveGamePatcher.SerializerManager.Filename;
                filePath = Path.Combine(farmerPath, filename);
            }
            else
                filePath = Path.Combine(Path.GetDirectoryName(farmerPath), SaveGamePatcher.SerializerManager.FarmerFilename);

            // restore mod nodes
            OptimizedModNodeList modNodes = OptimizedModNodeList.LoadFromFile(filePath);
            if (modNodes.ModNodes.Any())
                SaveGamePatcher.RestoreModNodes(doc, doc, modNodes, "/1"); // <?xml ... ?> is 1

            // deserialize XML
            using var reader = new XmlTextReader(new StringReader(doc.OuterXml));
            return serializer.Deserialize(reader);
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
                    if (SaveGamePatcher.FindAndRemoveModNodes(child, modNodes, $"{currPath}/{i}"))
                    {
                        modNodes.Add(new KeyValuePair<string, string>($"{currPath}/{i}", child.OuterXml));
                        node.RemoveChild(child);
                    }
                }
            }

            var attr = node.Attributes?["xsi:type"];
            if (attr == null)
                return false;

            if (attr.Value.StartsWith("Mods_"))
                return true;
            return false;
        }

        private static void SerializeProxy(XmlSerializer serializer, XmlWriter origWriter, object obj)
        {
            //Log.trace( "Start serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
            using var ms = new MemoryStream();
            using var writer = XmlWriter.Create(ms, new XmlWriterSettings { CloseOutput = false });

            serializer.Serialize(writer, obj);
            XmlDocument doc = new XmlDocument();
            ms.Position = 0;
            doc.Load(ms);

            var modNodes = new List<KeyValuePair<string, string>>();
            SaveGamePatcher.FindAndRemoveModNodes(doc, modNodes, "/1"); // <?xml ... ?> is /0

            doc.WriteContentTo(origWriter);
            // To fix serialize bug in mobile platform
            if (Constants.TargetPlatform == GamePlatform.Android)
                origWriter.Flush();
            string filename = serializer == SaveGame.farmerSerializer
                ? SaveGamePatcher.SerializerManager.FarmerFilename
                : SaveGamePatcher.SerializerManager.Filename;

            File.WriteAllText(
                Path.Combine(Constants.CurrentSavePath, filename),
                JsonConvert.SerializeObject(modNodes)
            );
            //Log.trace( "Mid serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
            //Log.trace( "End serialize\t" + System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 );
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.WriteXml))]
    // *grumbles something about inlining and MacOS*
    public static class RedirectGetSerializerForNonWindowsPatch1
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            List<CodeInstruction> ret = new();

            foreach (var insn in insns)
            {
                if ( insn.operand is MethodInfo meth && meth == AccessTools.Method( typeof(SaveGame), nameof(SaveGame.GetSerializer ) ) )
                {
                    insn.operand = AccessTools.Method(typeof(RedirectGetSerializerForNonWindowsPatch1), nameof(RedirectGetSerializerForNonWindowsPatch1.GetSerializerProxy));
                }
                ret.Add(insn);
            }

            return ret;
        }

        public static XmlSerializer GetSerializerProxy(Type type)
        {
            return SaveGamePatcher.SerializerManager.InitializeSerializer(type);
        }
    }
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.ReadXml))]
    public static class RedirectGetSerializerForNonWindowsPatch2
    {
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator gen, MethodBase original, IEnumerable<CodeInstruction> insns)
        {
            return RedirectGetSerializerForNonWindowsPatch1.Transpiler(gen, original, insns);
        }
    }
}
