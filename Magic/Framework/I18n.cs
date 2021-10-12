using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Magic.Framework
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "It is time for you to learn the ways of magic.".</summary>
        public static string Event_Wizard_1()
        {
            return I18n.GetByKey("event.wizard.1");
        }

        /// <summary>Get a translation equivalent to "I have... adjusted the Yoba altar in Pierre's shop for you to use.".</summary>
        public static string Event_Wizard_2()
        {
            return I18n.GetByKey("event.wizard.2");
        }

        /// <summary>Get a translation equivalent to "I will teach you the basic of Arcane magic, but the rest you must discover on your own. ".</summary>
        public static string Event_Wizard_3()
        {
            return I18n.GetByKey("event.wizard.3");
        }

        /// <summary>Get a translation equivalent to "The Arcane 'Analyze' spell can be used to discover most spells. However, certain more powerful spells must be discovered in other ways.".</summary>
        public static string Event_Wizard_4()
        {
            return I18n.GetByKey("event.wizard.4");
        }

        /// <summary>Get a translation equivalent to "Every school of magic has 3 normal spells, and a fourth ancient spell. Even I do not know where these spells lie.".</summary>
        public static string Event_Wizard_5()
        {
            return I18n.GetByKey("event.wizard.5");
        }

        /// <summary>Get a translation equivalent to "To help you get started, I will give you a hint. Try using Analyze on your watering can.".</summary>
        public static string Event_Wizard_6()
        {
            return I18n.GetByKey("event.wizard.6");
        }

        /// <summary>Get a translation equivalent to "(To start casting a spell, press Q. Then press 1-4 to choose the spell.)".</summary>
        public static string Event_Wizard_7()
        {
            return I18n.GetByKey("event.wizard.7");
        }

        /// <summary>Get a translation equivalent to "(Press TAB to switch between spell sets.)".</summary>
        public static string Event_Wizard_8()
        {
            return I18n.GetByKey("event.wizard.8");
        }

        /// <summary>Get a translation equivalent to "(These controls can be adjusted in Magic's config file.)".</summary>
        public static string Event_Wizard_9()
        {
            return I18n.GetByKey("event.wizard.9");
        }

        /// <summary>Get a translation equivalent to "MAGIC".</summary>
        public static string Event_Wizard_Abovehead()
        {
            return I18n.GetByKey("event.wizard.abovehead");
        }

        /// <summary>Get a translation equivalent to "Magical Analytics".</summary>
        public static string Tv_Analyzehints_Name()
        {
            return I18n.GetByKey("tv.analyzehints.name");
        }

        /// <summary>Get a translation equivalent to "*static*".</summary>
        public static string Tv_Analyzehints_Notmagical()
        {
            return I18n.GetByKey("tv.analyzehints.notmagical");
        }

        /// <summary>Get a translation equivalent to "Looking to use magic to work around the farm? Try analyzing your tools.".</summary>
        public static string Tv_Analyzehints_1()
        {
            return I18n.GetByKey("tv.analyzehints.1");
        }

        /// <summary>Get a translation equivalent to "Need a quick escape? I wonder if some boots will give you some inspiration.".</summary>
        public static string Tv_Analyzehints_2()
        {
            return I18n.GetByKey("tv.analyzehints.2");
        }

        /// <summary>Get a translation equivalent to "Sunlight is pretty nice, but not always available. I wonder if artificial lights can be recreated?".</summary>
        public static string Tv_Analyzehints_3()
        {
            return I18n.GetByKey("tv.analyzehints.3");
        }

        /// <summary>Get a translation equivalent to "Some basic minerals seem to contain some magical energy.".</summary>
        public static string Tv_Analyzehints_4()
        {
            return I18n.GetByKey("tv.analyzehints.4");
        }

        /// <summary>Get a translation equivalent to "The other day a meteor fell in my backyard. It seemed really mysterious.".</summary>
        public static string Tv_Analyzehints_5()
        {
            return I18n.GetByKey("tv.analyzehints.5");
        }

        /// <summary>Get a translation equivalent to "Coffee is delicious. Why not recreate its effects with a spell?".</summary>
        public static string Tv_Analyzehints_6()
        {
            return I18n.GetByKey("tv.analyzehints.6");
        }

        /// <summary>Get a translation equivalent to "The other day I saw someone sprout roots from the ground and keep a monster from moving, almost like a crop.".</summary>
        public static string Tv_Analyzehints_7()
        {
            return I18n.GetByKey("tv.analyzehints.7");
        }

        /// <summary>Get a translation equivalent to "I bet you could shoot a blast of ice to slow something down. I wonder where you could study ice...?".</summary>
        public static string Tv_Analyzehints_8()
        {
            return I18n.GetByKey("tv.analyzehints.8");
        }

        /// <summary>Get a translation equivalent to "I saw an dark altar in someone's house the other day. Well, I say 'house'. It was more like a hut.".</summary>
        public static string Tv_Analyzehints_9()
        {
            return I18n.GetByKey("tv.analyzehints.9");
        }

        /// <summary>Get a translation equivalent to "Health potions sure are inconvenient to carry around. Maybe we can heal ourselves with magic?".</summary>
        public static string Tv_Analyzehints_10()
        {
            return I18n.GetByKey("tv.analyzehints.10");
        }

        /// <summary>Get a translation equivalent to "Most people use ladders to descend underground, but I saw one person cast a spell and zip straight through the floor.".</summary>
        public static string Tv_Analyzehints_11()
        {
            return I18n.GetByKey("tv.analyzehints.11");
        }

        /// <summary>Get a translation equivalent to "I saw someone cast a spell to regain mana!? It looked like it hurt. Rumor has it they learned this technique while fishing for Lava Eels.".</summary>
        public static string Tv_Analyzehints_12()
        {
            return I18n.GetByKey("tv.analyzehints.12");
        }

        /// <summary>Get a translation equivalent to "You learned a spell: {{spellName}}".</summary>
        /// <param name="spellName">The value to inject for the <c>{{spellName}}</c> token.</param>
        public static string Spell_Learn(object spellName)
        {
            return I18n.GetByKey("spell.learn", new { spellName });
        }

        /// <summary>Get a translation equivalent to "You learned an ancient spell: {{spellName}}".</summary>
        /// <param name="spellName">The value to inject for the <c>{{spellName}}</c> token.</param>
        public static string Spell_Learn_Ancient(object spellName)
        {
            return I18n.GetByKey("spell.learn.ancient", new { spellName });
        }

        /// <summary>Get a translation equivalent to "A glowing altar".</summary>
        public static string Altar_Glow()
        {
            return I18n.GetByKey("altar.glow");
        }

        /// <summary>Get a translation equivalent to "arcane".</summary>
        public static string School_Arcane_Name()
        {
            return I18n.GetByKey("school.arcane.name");
        }

        /// <summary>Get a translation equivalent to "toil".</summary>
        public static string School_Toil_Name()
        {
            return I18n.GetByKey("school.toil.name");
        }

        /// <summary>Get a translation equivalent to "nature".</summary>
        public static string School_Nature_Name()
        {
            return I18n.GetByKey("school.nature.name");
        }

        /// <summary>Get a translation equivalent to "life".</summary>
        public static string School_Life_Name()
        {
            return I18n.GetByKey("school.life.name");
        }

        /// <summary>Get a translation equivalent to "elemental".</summary>
        public static string School_Elemental_Name()
        {
            return I18n.GetByKey("school.elemental.name");
        }

        /// <summary>Get a translation equivalent to "eldritch".</summary>
        public static string School_Eldritch_Name()
        {
            return I18n.GetByKey("school.eldritch.name");
        }

        /// <summary>Get a translation equivalent to "Analyze".</summary>
        public static string Spell_ArcaneAnalyze_Name()
        {
            return I18n.GetByKey("spell.arcane:analyze.name");
        }

        /// <summary>Get a translation equivalent to "Examine an item for magical inspiration.".</summary>
        public static string Spell_ArcaneAnalyze_Desc()
        {
            return I18n.GetByKey("spell.arcane:analyze.desc");
        }

        /// <summary>Get a translation equivalent to "Magic Missile".</summary>
        public static string Spell_ArcaneMagicmissle_Name()
        {
            return I18n.GetByKey("spell.arcane:magicmissle.name");
        }

        /// <summary>Get a translation equivalent to "A somewhat weak but seeking projectile.".</summary>
        public static string Spell_ArcaneMagicmissle_Desc()
        {
            return I18n.GetByKey("spell.arcane:magicmissle.desc");
        }

        /// <summary>Get a translation equivalent to "Disenchant".</summary>
        public static string Spell_ArcaneDisenchant_Name()
        {
            return I18n.GetByKey("spell.arcane:disenchant.name");
        }

        /// <summary>Get a translation equivalent to "Lower the quality of an item.\nSale price difference between old and new quality is refunded into wallet.".</summary>
        public static string Spell_ArcaneDisenchant_Desc()
        {
            return I18n.GetByKey("spell.arcane:disenchant.desc");
        }

        /// <summary>Get a translation equivalent to "Enchant".</summary>
        public static string Spell_ArcaneEnchant_Name()
        {
            return I18n.GetByKey("spell.arcane:enchant.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Money\nRaise the quality of an item.\nPrice is sale price difference between old and new quality.".</summary>
        public static string Spell_ArcaneEnchant_Desc()
        {
            return I18n.GetByKey("spell.arcane:enchant.desc");
        }

        /// <summary>Get a translation equivalent to "Rewind".</summary>
        public static string Spell_ArcaneRewind_Name()
        {
            return I18n.GetByKey("spell.arcane:rewind.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Gold Bar\nTurn back time.".</summary>
        public static string Spell_ArcaneRewind_Desc()
        {
            return I18n.GetByKey("spell.arcane:rewind.desc");
        }

        /// <summary>Get a translation equivalent to "Clear Debris".</summary>
        public static string Spell_ToilCleardebris_Name()
        {
            return I18n.GetByKey("spell.toil:cleardebris.name");
        }

        /// <summary>Get a translation equivalent to "Clear debris around the targeted area.\nHigher levels can clear larger types of debris.".</summary>
        public static string Spell_ToilCleardebris_Desc()
        {
            return I18n.GetByKey("spell.toil:cleardebris.desc");
        }

        /// <summary>Get a translation equivalent to "Till".</summary>
        public static string Spell_ToilTill_Name()
        {
            return I18n.GetByKey("spell.toil:till.name");
        }

        /// <summary>Get a translation equivalent to "Till soil around the targeted area.\nHigher levels indicate a larger area.".</summary>
        public static string Spell_ToilTill_Desc()
        {
            return I18n.GetByKey("spell.toil:till.desc");
        }

        /// <summary>Get a translation equivalent to "Water".</summary>
        public static string Spell_ToilWater_Name()
        {
            return I18n.GetByKey("spell.toil:water.name");
        }

        /// <summary>Get a translation equivalent to "Water soil around the targeted area.\nHigher levels indicate a larger area.".</summary>
        public static string Spell_ToilWater_Desc()
        {
            return I18n.GetByKey("spell.toil:water.desc");
        }

        /// <summary>Get a translation equivalent to "Blink".</summary>
        public static string Spell_ToilBlink_Name()
        {
            return I18n.GetByKey("spell.toil:blink.name");
        }

        /// <summary>Get a translation equivalent to "Teleport to the targeted area.".</summary>
        public static string Spell_ToilBlink_Desc()
        {
            return I18n.GetByKey("spell.toil:blink.desc");
        }

        /// <summary>Get a translation equivalent to "Lantern".</summary>
        public static string Spell_NatureLantern_Name()
        {
            return I18n.GetByKey("spell.nature:lantern.name");
        }

        /// <summary>Get a translation equivalent to "Summon a light to follow you.\nHigher levels create more light.".</summary>
        public static string Spell_NatureLantern_Desc()
        {
            return I18n.GetByKey("spell.nature:lantern.desc");
        }

        /// <summary>Get a translation equivalent to "Tendrils".</summary>
        public static string Spell_NatureTendrils_Name()
        {
            return I18n.GetByKey("spell.nature:tendrils.name");
        }

        /// <summary>Get a translation equivalent to "Summon tendrils to stop a foe in their path.".</summary>
        public static string Spell_NatureTendrils_Desc()
        {
            return I18n.GetByKey("spell.nature:tendrils.desc");
        }

        /// <summary>Get a translation equivalent to "Shockwave".</summary>
        public static string Spell_NatureShockwave_Name()
        {
            return I18n.GetByKey("spell.nature:shockwave.name");
        }

        /// <summary>Get a translation equivalent to "Jump into the air and slam down, sending out a shockwave.\n Higher levels are larger and fiercer.".</summary>
        public static string Spell_NatureShockwave_Desc()
        {
            return I18n.GetByKey("spell.nature:shockwave.desc");
        }

        /// <summary>Get a translation equivalent to "Photosynthesis".</summary>
        public static string Spell_NaturePhotosynthesis_Name()
        {
            return I18n.GetByKey("spell.nature:photosynthesis.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Prismatic Shard\n\nAdvance all plants in your farming areas a stage.".</summary>
        public static string Spell_NaturePhotosynthesis_Desc()
        {
            return I18n.GetByKey("spell.nature:photosynthesis.desc");
        }

        /// <summary>Get a translation equivalent to "Heal".</summary>
        public static string Spell_LifeHeal_Name()
        {
            return I18n.GetByKey("spell.life:heal.name");
        }

        /// <summary>Get a translation equivalent to "Restore your health at the cost of mana.\nHigher levels heal more at a better cost ratio.".</summary>
        public static string Spell_LifeHeal_Desc()
        {
            return I18n.GetByKey("spell.life:heal.desc");
        }

        /// <summary>Get a translation equivalent to "Haste".</summary>
        public static string Spell_LifeHaste_Name()
        {
            return I18n.GetByKey("spell.life:haste.name");
        }

        /// <summary>Get a translation equivalent to "Move as swift as the wind.\nHigher levels make you move faster for longer.".</summary>
        public static string Spell_LifeHaste_Desc()
        {
            return I18n.GetByKey("spell.life:haste.desc");
        }

        /// <summary>Get a translation equivalent to "Buff".</summary>
        public static string Spell_LifeBuff_Name()
        {
            return I18n.GetByKey("spell.life:buff.name");
        }

        /// <summary>Get a translation equivalent to "Improve every facet of your being.\nHigher levels improve yourself more for longer.".</summary>
        public static string Spell_LifeBuff_Desc()
        {
            return I18n.GetByKey("spell.life:buff.desc");
        }

        /// <summary>Get a translation equivalent to "Evac".</summary>
        public static string Spell_LifeEvac_Name()
        {
            return I18n.GetByKey("spell.life:evac.name");
        }

        /// <summary>Get a translation equivalent to "Teleport to where you entered the area.".</summary>
        public static string Spell_LifeEvac_Desc()
        {
            return I18n.GetByKey("spell.life:evac.desc");
        }

        /// <summary>Get a translation equivalent to "Fireball".</summary>
        public static string Spell_ElementalFireball_Name()
        {
            return I18n.GetByKey("spell.elemental:fireball.name");
        }

        /// <summary>Get a translation equivalent to "Shoot a devastating ball of fire at your enemies.\nHigher levels improve damage and speed.".</summary>
        public static string Spell_ElementalFireball_Desc()
        {
            return I18n.GetByKey("spell.elemental:fireball.desc");
        }

        /// <summary>Get a translation equivalent to "Frostbolt".</summary>
        public static string Spell_ElementalFrostbolt_Name()
        {
            return I18n.GetByKey("spell.elemental:frostbolt.name");
        }

        /// <summary>Get a translation equivalent to "Shoot a piercing bolt of ice at your enemies.\nHigher levels improve damage and speed.".</summary>
        public static string Spell_ElementalFrostbolt_Desc()
        {
            return I18n.GetByKey("spell.elemental:frostbolt.desc");
        }

        /// <summary>Get a translation equivalent to "Descend".</summary>
        public static string Spell_ElementalDescend_Name()
        {
            return I18n.GetByKey("spell.elemental:descend.name");
        }

        /// <summary>Get a translation equivalent to "Descend in the mines.\nHigher levels increase the floors passed, with no additional mana cost.".</summary>
        public static string Spell_ElementalDescend_Desc()
        {
            return I18n.GetByKey("spell.elemental:descend.desc");
        }

        /// <summary>Get a translation equivalent to "Teleport".</summary>
        public static string Spell_ElementalTeleport_Name()
        {
            return I18n.GetByKey("spell.elemental:teleport.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Travel Core\n\nWhen outside, teleport to any outdoors location.".</summary>
        public static string Spell_ElementalTeleport_Desc()
        {
            return I18n.GetByKey("spell.elemental:teleport.desc");
        }

        /// <summary>Get a translation equivalent to "Meteor".</summary>
        public static string Spell_EldritchMeteor_Name()
        {
            return I18n.GetByKey("spell.eldritch:meteor.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Iridium Ore".</summary>
        public static string Spell_EldritchMeteor_Desc()
        {
            return I18n.GetByKey("spell.eldritch:meteor.desc");
        }

        /// <summary>Get a translation equivalent to "Blood Mana".</summary>
        public static string Spell_EldritchBloodmana_Name()
        {
            return I18n.GetByKey("spell.eldritch:bloodmana.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: Your life".</summary>
        public static string Spell_EldritchBloodmana_Desc()
        {
            return I18n.GetByKey("spell.eldritch:bloodmana.desc");
        }

        /// <summary>Get a translation equivalent to "Luck Steal".</summary>
        public static string Spell_EldritchLucksteal_Name()
        {
            return I18n.GetByKey("spell.eldritch:lucksteal.name");
        }

        /// <summary>Get a translation equivalent to "Catalyst: The happiness of your loved ones".</summary>
        public static string Spell_EldritchLucksteal_Desc()
        {
            return I18n.GetByKey("spell.eldritch:lucksteal.desc");
        }

        /// <summary>Get a translation equivalent to "Spirit".</summary>
        public static string Spell_EldritchSpirit_Name()
        {
            return I18n.GetByKey("spell.eldritch:spirit.name");
        }

        /// <summary>Get a translation equivalent to "Summon a lost spirit to fight for you.".</summary>
        public static string Spell_EldritchSpirit_Desc()
        {
            return I18n.GetByKey("spell.eldritch:spirit.desc");
        }

        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        public static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

