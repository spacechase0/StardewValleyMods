using Magic.Framework.Schools;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal abstract class Spell
    {
        /*********
        ** Accessors
        *********/
        public string ParentSchoolId { get; }
        public School ParentSchool => School.GetSchool(this.ParentSchoolId);
        public string Id { get; }
        public string FullId => this.ParentSchoolId + ":" + this.Id;

        /// <summary>Whether the spell can be cast while a menu is open.</summary>
        public bool CanCastInMenus { get; protected set; }

        public Texture2D[] Icons { get; protected set; }


        /*********
        ** Public methods
        *********/
        public virtual int GetMaxCastingLevel()
        {
            return 3;
        }

        public abstract int GetManaCost(Farmer player, int level);

        public virtual bool CanCast(Farmer player, int level)
        {
            return
                Game1.player.GetSpellBook().KnowsSpell(this.FullId, level)
                && player.GetCurrentMana() >= this.GetManaCost(player, level);
        }

        /// <summary>Get the spell's translated name.</summary>
        public virtual string GetTranslatedName()
        {
            return I18n.GetByKey($"spell.{this.FullId}.name");
        }

        /// <summary>Get the spell's translated description.</summary>
        public virtual string GetTranslatedDescription()
        {
            return I18n.GetByKey($"spell.{this.FullId}.desc");
        }

        /// <summary>Get a translated tooltip to show for the spell.</summary>
        /// <param name="level">The spell level, if applicable.</param>
        public string GetTooltip(int? level = null)
        {
            string name = level != null && this.GetMaxCastingLevel() > 1
                ? I18n.Tooltip_Spell_NameAndLevel(spellName: this.GetTranslatedName(), level + 1)
                : this.GetTranslatedName();

            return string.Concat(name, "\n", this.GetTranslatedDescription());
        }

        public abstract IActiveEffect OnCast(Farmer player, int level, int targetX, int targetY);

        public virtual void LoadIcon()
        {
            try
            {
                this.Icons = new Texture2D[this.GetMaxCastingLevel()];
                for (int i = 1; i <= this.GetMaxCastingLevel(); ++i)
                {
                    this.Icons[i - 1] = Content.LoadTexture("magic/" + this.ParentSchool.Id + "/" + this.Id + "/" + i + ".png");
                }
            }
            catch (ContentLoadException e)
            {
                Log.Warn("Failed to load icon for spell " + this.FullId + ": " + e);
            }
        }


        /*********
        ** Protected methods
        *********/
        protected Spell(string school, string id)
        {
            this.ParentSchoolId = school;
            this.Id = id;
        }

    }
}
