using Magic.Schools;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared;
using StardewValley;

namespace Magic.Spells
{
    public abstract class Spell
    {
        public string ParentSchoolId { get; }
        public School ParentSchool { get { return School.getSchool(this.ParentSchoolId); } }
        public string Id { get; }
        public string FullId { get { return this.ParentSchoolId + ":" + this.Id; } }
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

        public virtual int getMaxCastingLevel()
        {
            return 3;
        }

        public abstract int getManaCost(Farmer player, int level);

        public virtual bool canCast(Farmer player, int level)
        {
            return player.knowsSpell(this.FullId, level) && player.getCurrentMana() >= this.getManaCost(player, level);
        }

        public virtual string getTranslatedName()
        {
            return Mod.instance.Helper.Translation.Get("spell." + this.FullId + ".name");
        }
        public virtual string getTranslatedDescription()
        {
            return Mod.instance.Helper.Translation.Get("spell." + this.FullId + ".desc");
        }

        public abstract IActiveEffect onCast(Farmer player, int level, int targetX, int targetY);

        public virtual void loadIcon()
        {
            try
            {
                this.Icons = new Texture2D[this.getMaxCastingLevel()];
                for (int i = 1; i <= this.getMaxCastingLevel(); ++i)
                {
                    this.Icons[i - 1] = Content.loadTexture("magic/" + this.ParentSchool.Id + "/" + this.Id + "/" + i + ".png");
                }
            }
            catch (ContentLoadException e)
            {
                Log.warn("Failed to load icon for spell " + this.FullId + ": " + e);
            }
        }
    }
}
