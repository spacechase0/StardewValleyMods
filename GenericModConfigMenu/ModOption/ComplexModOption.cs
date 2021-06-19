using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace GenericModConfigMenu.ModOption
{
    internal class ComplexModOption : BaseModOption
    {
        private object State;
        private readonly Func<Vector2, object, object> UpdateFunc;
        private readonly Func<SpriteBatch, Vector2, object, object> DrawFunc;
        private readonly Action<object> SaveFunc;

        public ComplexModOption(string name, string desc, Func<Vector2, object, object> update, Func<SpriteBatch, Vector2, object, object> draw, Action<object> save, IManifest mod)
            : base(name, desc, name, mod)
        {
            this.UpdateFunc = update;
            this.DrawFunc = draw;
            this.SaveFunc = save;
        }

        public override void SyncToMod()
        {
            this.State = null;
        }

        public override void Save()
        {
            this.SaveFunc.Invoke(this.State);
        }

        public void Update(Vector2 position)
        {
            this.State = this.UpdateFunc.Invoke(position, this.State);
        }

        public void Draw(SpriteBatch b, Vector2 position)
        {
            if (this.State == null)
                return;

            this.State = this.DrawFunc.Invoke(b, position, this.State);
        }
    }
}
