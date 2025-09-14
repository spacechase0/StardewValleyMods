using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SpaceCore.Patches;
using SpaceShared;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;

namespace SpaceCore.Framework
{
    /// <summary>Manages the serialization for <see cref="LoadGameMenuPatcher"/> and <see cref="SaveGamePatcher"/>.</summary>
    internal class SerializerManager
    {
        /*********
        ** Fields
        *********/
        /// <summary>Whether PyTK is installed.</summary>
        private readonly bool HasPyTk;

        /// <summary>Whether SpaceCore's custom types have been added to the <see cref="SaveGame"/> serializers.</summary>
        private bool InitializedSerializers;

        // Update these each game update
        private readonly Type[] VanillaMainTypes =
        {
            typeof(Character),
            typeof(GameLocation),
            typeof(Item),
            typeof(Quest),
            typeof(TerrainFeature)
        };
        private readonly Type[] VanillaFarmerTypes =
        {
            typeof(Item)
        };
        private readonly Type[] VanillaGameLocationTypes =
        {
            typeof(Character),
            typeof(Item),
            typeof(TerrainFeature)
        };
        private readonly Type[] VanillaDescriptionElementTypes =
        {
            typeof(Character),
            typeof(Item)
        };
        private readonly Type[] VanillaLegacyDescriptionElementTypes =
        {
            typeof(DescriptionElement),
            typeof(Character),
            typeof(Item)
        };


        /*********
        ** Accessors
        *********/
        public readonly string Filename = "spacecore-serialization.json";
        public readonly string FarmerFilename = "spacecore-serialization-farmer.json";

        /// <summary>The save filename currently being loaded, if any.</summary>
        public string LoadFileContext = null;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modRegistry">The mod registry to check for installed mods.</param>
        public SerializerManager(IModRegistry modRegistry)
        {
            this.HasPyTk = modRegistry.IsLoaded("Platonymous.Toolkit");
        }

        public void InitializeSerializers()
        {
            // skip if already initialized
            if (this.InitializedSerializers)
                return;
            this.InitializedSerializers = true;

            // add custom types to save serializers
            //
            // When PyTK is installed, this is needed even if we're not making any actual changes
            // to the serializers. (Just notifying it doesn't seem to be enough.) This can be
            // tested by installing Seed Bag, spawning one in the inventory, then checking whether
            // it has the two attachment slots when you reload.
            if (SpaceCore.ModTypes.Any() || this.HasPyTk)
            {
                Log.Trace($"Reinitializing serializers for {SpaceCore.ModTypes.Count} mod types...");

                SaveGame.serializer = this.InitializeSerializer(typeof(SaveGame), this.VanillaMainTypes);
                SaveGame.farmerSerializer = this.InitializeSerializer(typeof(Farmer), this.VanillaFarmerTypes);
                SaveGame.locationSerializer = this.InitializeSerializer(typeof(GameLocation), this.VanillaGameLocationTypes);
                SaveGame.descriptionElementSerializer = this.InitializeSerializer(typeof(DescriptionElement), this.VanillaDescriptionElementTypes);
                SaveGame.legacyDescriptionElementSerializer = this.InitializeSerializer(typeof(DescriptionElement), this.VanillaLegacyDescriptionElementTypes);
            }
        }

        private Dictionary<Type, XmlSerializer> serializersAlreadyDone = new();

        public XmlSerializer InitializeSerializer(Type baseType, Type[] extra = null)
        {
            if (serializersAlreadyDone.ContainsKey(baseType))
                return serializersAlreadyDone[baseType];

            var types = extra?.Length > 0
                ? extra.Concat(SpaceCore.ModTypes)
                : SpaceCore.ModTypes;

            XmlSerializer serializer = new(baseType, types.ToArray());
            serializersAlreadyDone.Add(baseType, serializer);
            this.NotifyPyTk(serializer);
            return serializer;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Notify PyTK that the serializers were changed, if it's installed.</summary>
        /// <param name="serializer">The XML serializer which changed.</param>
        private void NotifyPyTk(XmlSerializer serializer)
        {
            if (!this.HasPyTk)
                return;

            const string errorPrefix = "PyTK is installed, but we couldn't notify it about serializer changes. PyTK serialization might not work correctly.\nTechnical details:";
            try
            {
                // fetch PyTK mod
                var mod = Type.GetType("PyTK.PyTKMod, PyTK");
                if (mod == null)
                {
                    Log.Monitor.LogOnce($"{errorPrefix} couldn't fetch its mod instance.");
                    return;
                }

                // fetch notify method
                const string methodName = "SerializersReinitialized";
                var method = mod.GetMethod(methodName);
                if (method == null)
                {
                    Log.Monitor.LogOnce($"{errorPrefix} couldn't fetch its '{methodName}' method.");
                    return;
                }

                // notify
                method.Invoke(null, new object[] { serializer });
            }
            catch (Exception ex)
            {
                Log.Monitor.LogOnce($"{errorPrefix} {ex}", LogLevel.Warn);
            }
        }
    }
}
