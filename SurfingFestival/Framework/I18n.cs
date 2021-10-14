using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace SurfingFestival.Framework
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

        /// <summary>Get a translation equivalent to "Surfing Festival".</summary>
        public static string Festival_Name()
        {
            return I18n.GetByKey("festival.name");
        }

        /// <summary>Get a translation equivalent to "Items (right click to use):^ Stardrop Boost - Temporary speed boost.^ Pufferfish Projectile - Shoot a pufferfish at the next player ahead.^ Seeking Cloud - Zap the player in first place.^ Junimo Power - Invincibility and a small speed boost.".</summary>
        public static string Race_Instructions()
        {
            return I18n.GetByKey("race.instructions");
        }

        /// <summary>Get a translation equivalent to "Ready...".</summary>
        public static string Race_LewisStart_0()
        {
            return I18n.GetByKey("race.lewis-start.0");
        }

        /// <summary>Get a translation equivalent to "Set.....".</summary>
        public static string Race_LewisStart_1()
        {
            return I18n.GetByKey("race.lewis-start.1");
        }

        /// <summary>Get a translation equivalent to "Go!".</summary>
        public static string Race_LewisStart_2()
        {
            return I18n.GetByKey("race.lewis-start.2");
        }

        /// <summary>Get a translation equivalent to "Congratulations {{name}}!#$b#Every time I see you I learn something new about you! I don't think any of us knew you could surf so well.#$b#Come and get your prize.".</summary>
        /// <param name="name">The value to inject for the <c>{{name}}</c> token.</param>
        public static string Race_Winner_Player(object name)
        {
            return I18n.GetByKey("race.winner.player", new { name });
        }

        /// <summary>Get a translation equivalent to "Congratulations Harvey! Not bad for a first timer, here's your prize.".</summary>
        public static string Race_Winner_Harvey()
        {
            return I18n.GetByKey("race.winner.harvey");
        }

        /// <summary>Get a translation equivalent to "Congratulations Emily! You'll be tough competition to beat next year. Here's your prize.".</summary>
        public static string Race_Winner_Emily()
        {
            return I18n.GetByKey("race.winner.emily");
        }

        /// <summary>Get a translation equivalent to "Congratulations Maru! You were fantastic out there. Here's you're prize.".</summary>
        public static string Race_Winner_Maru()
        {
            return I18n.GetByKey("race.winner.maru");
        }

        /// <summary>Get a translation equivalent to "Congratulations Shane! Jas was cheering you on the entire time. Here's your prize.".</summary>
        public static string Race_Winner_Shane()
        {
            return I18n.GetByKey("race.winner.shane");
        }

        /// <summary>Get a translation equivalent to "Do you know why I was afraid to go surfing @?#$b#Somebody online was talking about a sea monster that lives underneath the water. I know I shouldn't believe everything I read but it sounded so authentic!".</summary>
        public static string Npc_AbigailSpouse()
        {
            return I18n.GetByKey("npc.Abigail_spouse");
        }

        /// <summary>Get a translation equivalent to "If you join in on the fun I'll be cheering you on!$h".</summary>
        public static string Npc_LeahSpouse()
        {
            return I18n.GetByKey("npc.Leah_spouse");
        }

        /// <summary>Get a translation equivalent to "Think you can beat me in the surfing competition? How about loser makes dinner tonight?".</summary>
        public static string Npc_MaruSpouse()
        {
            return I18n.GetByKey("npc.Maru_spouse");
        }

        /// <summary>Get a translation equivalent to "Have you decided if you're going to join in on the fun @? I'll be cheering you on! You can do it!$h".</summary>
        public static string Npc_PennySpouse()
        {
            return I18n.GetByKey("npc.Penny_spouse");
        }

        /// <summary>Get a translation equivalent to "I'm happy to stay on land and watch everyone surf. Emily thinks it would be good for me to try exciting things but this is a bit much for me.".</summary>
        public static string Npc_HaleySpouse()
        {
            return I18n.GetByKey("npc.Haley_spouse");
        }

        /// <summary>Get a translation equivalent to "What all do you think is under the water? Abigail sent me a post about a sea creature living underneath. Do you think that's true?".</summary>
        public static string Npc_SebastianSpouse()
        {
            return I18n.GetByKey("npc.Sebastian_spouse");
        }

        /// <summary>Get a translation equivalent to "I wanted to participate but my mom thinks it's best to not because of Vincent. She doesn't want him getting any ideas and I can't blame her. He's a wild little bother.".</summary>
        public static string Npc_SamSpouse()
        {
            return I18n.GetByKey("npc.Sam_spouse");
        }

        /// <summary>Get a translation equivalent to "You don't have to worry about me. I withdrew my entry since Sam couldn't join in. We were gonna see who was the fastest: me or the skateboarder.".</summary>
        public static string Npc_AlexSpouse()
        {
            return I18n.GetByKey("npc.Alex_spouse");
        }

        /// <summary>Get a translation equivalent to "Please don't get hurt @. I don't want you colliding into a rock and hitting your head.".</summary>
        public static string Npc_HarveySpouse()
        {
            return I18n.GetByKey("npc.Harvey_spouse");
        }

        /// <summary>Get a translation equivalent to "Sometimes I miss that old, drafty cabin. Our home is much nicer.".</summary>
        public static string Npc_ElliottSpouse()
        {
            return I18n.GetByKey("npc.Elliott_spouse");
        }

        /// <summary>Get a translation equivalent to "I still try to push Haley to try new things. She always talked about getting out of Pelican Town but where in the city can she ride some waves?".</summary>
        public static string Npc_EmilySpouse()
        {
            return I18n.GetByKey("npc.Emily_spouse");
        }

        /// <summary>Get a translation equivalent to "Do you plan on participating in the race @?#$b#I wanted to but I'm a little afraid of what might be in the water....$3".</summary>
        public static string Npc_Abigail()
        {
            return I18n.GetByKey("npc.Abigail");
        }

        /// <summary>Get a translation equivalent to "Haley always wants to try exciting things but she didn't want to participate in the race.#$b#So I took her place! Best of luck to you @!$h".</summary>
        public static string Npc_Emily()
        {
            return I18n.GetByKey("npc.Emily");
        }

        /// <summary>Get a translation equivalent to "Lewis asked me to make some surfboards since they were too expensive to import.".</summary>
        public static string Npc_Robin()
        {
            return I18n.GetByKey("npc.Robin");
        }

        /// <summary>Get a translation equivalent to "Did you know Maru surfed? I didn't! This valley is full of secrets, even some that aren't scientific.".</summary>
        public static string Npc_Demetrius()
        {
            return I18n.GetByKey("npc.Demetrius");
        }

        /// <summary>Get a translation equivalent to "Mmm, I'm a little nervous about the surfing competition but Harvey said he'd participate with me.".</summary>
        public static string Npc_Maru()
        {
            return I18n.GetByKey("npc.Maru");
        }

        /// <summary>Get a translation equivalent to "I could use some adrenaline but I'll stick to my bike.".</summary>
        public static string Npc_Sebastian()
        {
            return I18n.GetByKey("npc.Sebastian");
        }

        /// <summary>Get a translation equivalent to "I would much rather be swimming in the water than on top of it.".</summary>
        public static string Npc_Linus()
        {
            return I18n.GetByKey("npc.Linus");
        }

        /// <summary>Get a translation equivalent to "Take a look at all the surfboards Robin made. I'm honored to be able to sell such fine craftsmanship.".</summary>
        public static string Npc_Pierre()
        {
            return I18n.GetByKey("npc.Pierre");
        }

        /// <summary>Get a translation equivalent to "I always love these types of festivals. It's so nice to be surrounded by friends, food, and seeing the kids having fun.".</summary>
        public static string Npc_Caroline()
        {
            return I18n.GetByKey("npc.Caroline");
        }

        /// <summary>Get a translation equivalent to "I was going to show off these muscles and race Sam but he backed out last minute.".</summary>
        public static string Npc_Alex()
        {
            return I18n.GetByKey("npc.Alex");
        }

        /// <summary>Get a translation equivalent to "I used to surf back in my youth. Went to Oahu, and Tavarua if you can believe this old man.".</summary>
        public static string Npc_George()
        {
            return I18n.GetByKey("npc.George");
        }

        /// <summary>Get a translation equivalent to "Did you know George used to be a surfer back in the day?#$b#I didn't know him when he surfed but he's shown me pictures of him riding the waves.#$b#He's always been so handsome.$h".</summary>
        public static string Npc_Evelyn()
        {
            return I18n.GetByKey("npc.Evelyn");
        }

        /// <summary>Get a translation equivalent to "If you haven't already, don't forget to add some fuel to the bonfire.#$b#We want everyone to be warm while the competition is happening.".</summary>
        public static string Npc_Lewis()
        {
            return I18n.GetByKey("npc.Lewis");
        }

        /// <summary>Get a translation equivalent to "I've spent most of my life in Pelican Town @, and I never would have said somebody could surf on these waters.$s".</summary>
        public static string Npc_Clint()
        {
            return I18n.GetByKey("npc.Clint");
        }

        /// <summary>Get a translation equivalent to "I'm looking forward to the surfing competition later! Do you think I should start preparing for next year @?".</summary>
        public static string Npc_Penny()
        {
            return I18n.GetByKey("npc.Penny");
        }

        /// <summary>Get a translation equivalent to "We've never had a festival like this in the valley before you got here @. No way Lewis thought up this on his own.".</summary>
        public static string Npc_Pam()
        {
            return I18n.GetByKey("npc.Pam");
        }

        /// <summary>Get a translation equivalent to "Emily kept saying how I should try out surfing but I don't want my hair getting ruined.#$b#A swim cap? No! That would look horrible.$a".</summary>
        public static string Npc_Haley()
        {
            return I18n.GetByKey("npc.Haley");
        }

        /// <summary>Get a translation equivalent to "I managed to talk the boys out of surfing this year.#$b#Sam's a natural because of his skateboarding but I don't want him hurting himself on a rock or getting caught up in a net.#$b#Vincent? He's too young but maybe when he's older.".</summary>
        public static string Npc_Jodi()
        {
            return I18n.GetByKey("npc.Jodi");
        }

        /// <summary>Get a translation equivalent to "Jodi should have let the kids participate this year. They're young. They need to experience more positive things in their lives.".</summary>
        public static string Npc_Kent()
        {
            return I18n.GetByKey("npc.Kent");
        }

        /// <summary>Get a translation equivalent to "There isn't a sword or bow on this planet that can damage the waves. Best of luck to you @.".</summary>
        public static string Npc_Marlon()
        {
            return I18n.GetByKey("npc.Marlon");
        }

        /// <summary>Get a translation equivalent to "Alex wanted to race but I just want to relax. I had the perfect excuse too.".</summary>
        public static string Npc_Sam()
        {
            return I18n.GetByKey("npc.Sam");
        }

        /// <summary>Get a translation equivalent to "I'm staying on land to get inspiration for my next art piece. Maybe I'll call it Valley Waves.".</summary>
        public static string Npc_Leah()
        {
            return I18n.GetByKey("npc.Leah");
        }

        /// <summary>Get a translation equivalent to "I'm looking forward to the race later! We have an extra board if you want to participate @.".</summary>
        public static string Npc_Shane()
        {
            return I18n.GetByKey("npc.Shane");
        }

        /// <summary>Get a translation equivalent to "Lewis told me that Shane signed up for the race. It was startling at first but I know he can win.".</summary>
        public static string Npc_Marnie()
        {
            return I18n.GetByKey("npc.Marnie");
        }

        /// <summary>Get a translation equivalent to "I'm not terribly fond of my front lawn being taken over but this festival is quite fun.#$b#Perhaps I'll practice balancing on a surfboard in one of the tide pools.".</summary>
        public static string Npc_Elliott()
        {
            return I18n.GetByKey("npc.Elliott");
        }

        /// <summary>Get a translation equivalent to "I always love getting to spend a day out of the saloon. This festival is going to be great.$h".</summary>
        public static string Npc_Gus()
        {
            return I18n.GetByKey("npc.Gus");
        }

        /// <summary>Get a translation equivalent to "On the surface people aren't afraid of water. They ride it. But down here? The water rushes in through the cracks.".</summary>
        public static string Npc_Dwarf()
        {
            return I18n.GetByKey("npc.Dwarf");
        }

        /// <summary>Get a translation equivalent to "Do you require an arcane charm to improve your luck in the competition?".</summary>
        public static string Npc_Wizard()
        {
            return I18n.GetByKey("npc.Wizard");
        }

        /// <summary>Get a translation equivalent to "*gulp* Why did I agree to try this out. I've never been surfing before.$s".</summary>
        public static string Npc_Harvey()
        {
            return I18n.GetByKey("npc.Harvey");
        }

        /// <summary>Get a translation equivalent to "There's a festival happening! I'm so excited!".</summary>
        public static string Npc_Sandy()
        {
            return I18n.GetByKey("npc.Sandy");
        }

        /// <summary>Get a translation equivalent to "Shane's gonna win the surfing competition!$h".</summary>
        public static string Npc_Jas()
        {
            return I18n.GetByKey("npc.Jas");
        }

        /// <summary>Get a translation equivalent to "Mom said I'm not old enough to go surfing.$s".</summary>
        public static string Npc_Vincent()
        {
            return I18n.GetByKey("npc.Vincent");
        }

        /// <summary>Get a translation equivalent to "Sometimes I can hear the waves crashing against the dock. I hope the kids know what they're doing. The sea, she can be a rough one.".</summary>
        public static string Npc_Willy()
        {
            return I18n.GetByKey("npc.Willy");
        }

        /// <summary>Get a translation equivalent to "Laps: {{laps}}/2".</summary>
        /// <param name="laps">The value to inject for the <c>{{laps}}</c> token.</param>
        public static string Ui_Laps(object laps)
        {
            return I18n.GetByKey("ui.laps", new { laps });
        }

        /// <summary>Get a translation equivalent to "Ranking".</summary>
        public static string Ui_Ranking()
        {
            return I18n.GetByKey("ui.ranking");
        }

        /// <summary>Get a translation equivalent to "Add some wood to the bonfire".</summary>
        public static string Ui_Wood()
        {
            return I18n.GetByKey("ui.wood");
        }

        /// <summary>Get a translation equivalent to "You placed 50 wood on the bonfire.".</summary>
        public static string Dialog_Wood()
        {
            return I18n.GetByKey("dialog.wood");
        }

        /// <summary>Get a translation equivalent to "...That's odd. Why did the fire turn purple?".</summary>
        public static string Dialog_Shorts()
        {
            return I18n.GetByKey("dialog.shorts");
        }

        /// <summary>Get a translation equivalent to "You received 1500g.".</summary>
        public static string Dialog_PrizeMoney()
        {
            return I18n.GetByKey("dialog.prize-money");
        }

        /// <summary>Get a translation equivalent to "Stardrop Boost".</summary>
        public static string Item_Boost()
        {
            return I18n.GetByKey("item.boost");
        }

        /// <summary>Get a translation equivalent to "Pufferfish Projectile".</summary>
        public static string Item_HomingProjectile()
        {
            return I18n.GetByKey("item.homing-projectile");
        }

        /// <summary>Get a translation equivalent to "Seeking Cloud".</summary>
        public static string Item_FirstPlaceProjectile()
        {
            return I18n.GetByKey("item.first-place-projectile");
        }

        /// <summary>Get a translation equivalent to "Junimo Power".</summary>
        public static string Item_Invincibility()
        {
            return I18n.GetByKey("item.invincibility");
        }

        /// <summary>Get a translation equivalent to "A sign reads: Want to get ahead, now and in the future? Make an offer.".</summary>
        public static string Secret_Text()
        {
            return I18n.GetByKey("secret.text");
        }

        /// <summary>Get a translation equivalent to "Throw 100,000g into the box?".</summary>
        public static string Secret_Yes()
        {
            return I18n.GetByKey("secret.yes");
        }

        /// <summary>Get a translation equivalent to "Leave".</summary>
        public static string Secret_No()
        {
            return I18n.GetByKey("secret.no");
        }

        /// <summary>Get a translation equivalent to "You can't afford that.".</summary>
        public static string Secret_Broke()
        {
            return I18n.GetByKey("secret.broke");
        }

        /// <summary>Get a translation equivalent to "You feel different somehow.".</summary>
        public static string Secret_Purchased()
        {
            return I18n.GetByKey("secret.purchased");
        }

        /// <summary>Get a translation equivalent to "Brown Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardBrown_Name()
        {
            return I18n.GetByKey("craftables.surfboard-brown.name");
        }

        /// <summary>Get a translation equivalent to "A brown surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardBrown_Description()
        {
            return I18n.GetByKey("craftables.surfboard-brown.description");
        }

        /// <summary>Get a translation equivalent to "Cool Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardCool_Name()
        {
            return I18n.GetByKey("craftables.surfboard-cool.name");
        }

        /// <summary>Get a translation equivalent to "A surfboard with a cool, bright green pattern on it that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardCool_Description()
        {
            return I18n.GetByKey("craftables.surfboard-cool.description");
        }

        /// <summary>Get a translation equivalent to "Hibiscus Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardHibiscus_Name()
        {
            return I18n.GetByKey("craftables.surfboard-hibiscus.name");
        }

        /// <summary>Get a translation equivalent to "A pink and yellow surfboard with hibiscus on it that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardHibiscus_Description()
        {
            return I18n.GetByKey("craftables.surfboard-hibiscus.description");
        }

        /// <summary>Get a translation equivalent to "Red Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardRed_Name()
        {
            return I18n.GetByKey("craftables.surfboard-red.name");
        }

        /// <summary>Get a translation equivalent to "A red surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardRed_Description()
        {
            return I18n.GetByKey("craftables.surfboard-red.description");
        }

        /// <summary>Get a translation equivalent to "Stripe Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardStripe_Name()
        {
            return I18n.GetByKey("craftables.surfboard-stripe.name");
        }

        /// <summary>Get a translation equivalent to "A blue stiped surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardStripe_Description()
        {
            return I18n.GetByKey("craftables.surfboard-stripe.description");
        }

        /// <summary>Get a translation equivalent to "White Surfboard (Scarecrow)".</summary>
        public static string Craftables_SurfboardWhite_Name()
        {
            return I18n.GetByKey("craftables.surfboard-white.name");
        }

        /// <summary>Get a translation equivalent to "A white surfboard that also functions as a scarecrow.".</summary>
        public static string Craftables_SurfboardWhite_Description()
        {
            return I18n.GetByKey("craftables.surfboard-white.description");
        }

        /// <summary>Get a translation equivalent to "Surfing Trophy".</summary>
        public static string Craftables_SurfingTrophy_Name()
        {
            return I18n.GetByKey("craftables.surfing-trophy.name");
        }

        /// <summary>Get a translation equivalent to "A trophy for winning the surfing festival.".</summary>
        public static string Craftables_SurfingTrophy_Description()
        {
            return I18n.GetByKey("craftables.surfing-trophy.description");
        }

        /// <summary>Get a translation equivalent to "Black Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskBlack_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-black.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskBlack_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-black.description");
        }

        /// <summary>Get a translation equivalent to "Blue Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskBlue_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-blue.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskBlue_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-blue.description");
        }

        /// <summary>Get a translation equivalent to "Green Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskGreen_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-green.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskGreen_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-green.description");
        }

        /// <summary>Get a translation equivalent to "Orange Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskOrange_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-orange.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskOrange_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-orange.description");
        }

        /// <summary>Get a translation equivalent to "Pink Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskPink_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-pink.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskPink_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-pink.description");
        }

        /// <summary>Get a translation equivalent to "Red Snorkel Mask".</summary>
        public static string Hats_SnorkelMaskRed_Name()
        {
            return I18n.GetByKey("hats.snorkel-mask-red.name");
        }

        /// <summary>Get a translation equivalent to "A snorkel mask - serves no other purpose than decorative.".</summary>
        public static string Hats_SnorkelMaskRed_Description()
        {
            return I18n.GetByKey("hats.snorkel-mask-red.description");
        }

        /// <summary>Get a translation equivalent to "Cheap Amulet?".</summary>
        public static string Items_CheapAmulet_Name()
        {
            return I18n.GetByKey("items.cheap-amulet.name");
        }

        /// <summary>Get a translation equivalent to "Something seems off about this amulet. Wonder why it's so cheap.".</summary>
        public static string Items_CheapAmulet_Description()
        {
            return I18n.GetByKey("items.cheap-amulet.description");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static Translation GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

