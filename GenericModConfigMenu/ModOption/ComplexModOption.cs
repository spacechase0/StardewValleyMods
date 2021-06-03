using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu.ModOption
{
    internal class ComplexModOption : BaseModOption
    {
        private object state;
        private Func<Vector2, object, object> updateFunc;
        private Func<SpriteBatch, Vector2, object, object> drawFunc;
        private Action<object> saveFunc;

        public ComplexModOption( string name, string desc, Func<Vector2, object, object> update, Func<SpriteBatch, Vector2, object, object> draw, Action<object> save, IManifest mod )
        :   base( name, desc, name, mod )
        {
            updateFunc = update;
            drawFunc = draw;
            saveFunc = save;
        }

        public override void SyncToMod()
        {
            state = null;
        }

        public override void Save()
        {
            saveFunc.Invoke(state);
        }

        public void Update(Vector2 position)
        {
            state = updateFunc.Invoke(position, state);
        }

        public void Draw(SpriteBatch b, Vector2 position)
        {
            if (state == null)
                return;

            state = drawFunc.Invoke(b, position, state);
        }
    }
}
