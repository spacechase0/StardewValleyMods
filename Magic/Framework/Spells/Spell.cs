using Magic.Framework.Schools;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace Magic.Framework.Spells
{
    internal abstract class Spell
    {
        public string ParentSchoolId { get; }
        public School ParentSchool => School.GetSchool(this.ParentSchoolId);
        public string Id { get; }
        public string FullId => this.ParentSchoolId + ":" + this.Id;

        public Texture2D[] Icons
        {
            get;
            protected set;
        }

        protected Spell(string school, string id)
        {
            this.ParentSchoolId = school;
            this.Id = id;
        }

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

        public virtual string GetTranslatedName()
        {
            return Mod.Instance.Helper.Translation.Get("spell." + this.FullId + ".name");
        }
        public virtual string GetTranslatedDescription()
        {
            return Mod.Instance.Helper.Translation.Get("spell." + this.FullId + ".desc");
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
    }
}
