using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miniscript;
using SpaceCore.Framework.ExtEngine.Models;
using SpaceCore.UI;
using StardewValley.Menus;

namespace SpaceCore.Framework.ExtEngine
{
    internal class ExtensionMenu : IClickableMenu
    {
        private RootElement ui;
        private Dictionary<string, List<Element>> elemsById;
        private Interpreter interpreter;

        private static string ScriptPrefix = @"";
        private static string ScriptEventLoop = @"
_events = []
while true
    if _events.len > 0 then
        _nextEvent = _events.pull
//        _nextEvent.function _nextEvent.arguments
        _nextEvent
    end if
    yield
end while
";

        public ExtensionMenu(UiContentModel uiModel)
        {
            ui = ( RootElement ) uiModel.CreateUi( out elemsById );

            interpreter = ExtensionEngine.SetupInterpreter();
            interpreter.hostData = this;
            AdditionalSetup();

            interpreter.Reset(ScriptPrefix + uiModel.Script + ScriptEventLoop);
            interpreter.Compile();
            interpreter.RunUntilDone(0.01);
        }

        // TODO: Change this when I can attach them to just the interpreter
        static bool didIntrinsics = false;
        private void AdditionalSetup()
        {
            if (didIntrinsics) return;
            didIntrinsics = true;

            var i = Intrinsic.Create("getElements");
            i.AddParam("id");
            i.code = (ctx, prevResult) =>
            {
                ValList ret = new();
                string id = ctx.GetVar("id").ToString();

                var menu = ctx.interpreter.hostData as ExtensionMenu;
                if (!menu.elemsById.ContainsKey(id))
                    return new Intrinsic.Result(ret);

                foreach (var elem in menu.elemsById[id])
                {
                    ValMap entry = new();
                    entry.map.Add(new ValString("x"), new ValNumber(elem.LocalPosition.X));
                    entry.map.Add(new ValString("y"), new ValNumber(elem.LocalPosition.Y));
                    entry.assignOverride = (key, value) =>
                    {
                        switch (key.ToString())
                        {
                            case "x": elem.LocalPosition = new(value.FloatValue(), elem.LocalPosition.Y); break;
                            case "y": elem.LocalPosition = new(elem.LocalPosition.X, value.FloatValue()); break;
                        }
                        return true;
                    };
                    ret.values.Add(entry);
                }
                return new Intrinsic.Result(ret);
            };
        }

        private ValMap MakeContext()
        {
            return new ValMap();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            Value updateFunc = interpreter.GetGlobalValue("update");
            if (updateFunc == null) return;
            var events = interpreter.GetGlobalValue("_events") as ValList;
            if (events == null) return;

            //ValMap callUpdate = new();
            //callUpdate.map.Add(new ValString("function"), updateFunc);
            //callUpdate.map.Add(new ValString("arguments"), MakeContext());
            //events.values.Add(callUpdate);

            events.values.Add(updateFunc);

            interpreter.RunUntilDone(0.01);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            ui.Draw(b);
            drawMouse(b);
        }
    }
}
