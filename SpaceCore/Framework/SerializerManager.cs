using System;
using System.Linq;
using System.Xml.Serialization;
using SpaceCore.Patches;
using SpaceShared;
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
        private bool InitializedSerializers;

        // Update these each game update
        private readonly Type[] VanillaMainTypes = new Type[25]
        {
            typeof(Tool),
            typeof(GameLocation),
            typeof(Duggy),
            typeof(Bug),
            typeof(BigSlime),
            typeof(Ghost),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Quest),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
        };
        private readonly Type[] VanillaFarmerTypes = new Type[1]
        {
            typeof(Tool)
        };
        private readonly Type[] VanillaGameLocationTypes = new Type[24]
        {
            typeof(Tool),
            typeof(Duggy),
            typeof(Ghost),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Bug),
            typeof(BigSlime),
            typeof(BreakableContainer),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
            typeof(TerrainFeature)
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
        public void InitializeSerializers()
        {
            if (this.InitializedSerializers || !SpaceCore.ModTypes.Any())
                return;
            this.InitializedSerializers = true;

            Log.Trace("Reinitializing serializers...");

            SaveGame.serializer = this.InitializeSerializer(typeof(SaveGame), this.VanillaMainTypes);
            SaveGame.farmerSerializer = this.InitializeSerializer(typeof(Farmer), this.VanillaFarmerTypes);
            SaveGame.locationSerializer = this.InitializeSerializer(typeof(GameLocation), this.VanillaGameLocationTypes);

            this.NotifyPyTk();
        }

        public XmlSerializer InitializeSerializer(Type baseType, Type[] extra = null)
        {
            var types = extra?.Length > 0
                ? extra.Concat(SpaceCore.ModTypes)
                : SpaceCore.ModTypes;

            return new(baseType, types.ToArray());
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Notify PyTK that the serializers were changed, if it's installed.</summary>
        private void NotifyPyTk()
        {
            if (SpaceCore.Instance.Helper.ModRegistry.IsLoaded("Platonymous.Toolkit"))
            {
                try
                {
                    var pytk = Type.GetType("PyTK.PyTKMod, PyTK");
                    pytk.GetMethod("SerializersReinitialized").Invoke(null, new object[] { null });
                }
                catch (Exception e)
                {
                    Log.Trace("Exception, probably because PyTK hasn't released yet: " + e);
                }
            }
        }
    }
}
