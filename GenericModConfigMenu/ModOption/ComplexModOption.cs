using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class ComplexModOption : BaseModOption
    {
        private object state;
        private Func<Vector2, object, object> updateFunc;
        private Func<SpriteBatch, Vector2, object, object> drawFunc;
        private Action<object> saveFunc;

        public ComplexModOption(string name, string desc, Func<Vector2, object, object> update, Func<SpriteBatch, Vector2, object, object> draw, Action<object> save, IManifest mod)
            : base(name, desc, name, mod)
        {
            this.updateFunc = update;
            this.drawFunc = draw;
            this.saveFunc = save;
        }

        public override void SyncToMod()
        {
            this.state = null;
        }

        public override void Save()
        {
            this.saveFunc.Invoke(this.state);
        }

        public void Update(Vector2 position)
        {
            this.state = this.updateFunc.Invoke(position, this.state);
        }

        public void Draw(SpriteBatch b, Vector2 position)
        {
            if (this.state == null)
                return;

            this.state = this.drawFunc.Invoke(b, position, this.state);
        }
    }
}
