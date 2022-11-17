using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miniscript;
using SpaceCore.Framework.ExtEngine.Models;
using SpaceCore.Framework.ExtEngine.Script;
using SpaceCore.UI;
using SpaceShared;
using StardewValley;
using StardewValley.Menus;

namespace SpaceCore.Framework.ExtEngine
{
    internal class ExtensionMenu : IClickableMenu
    {
        internal UiContentModel origModel;
        private RootElement ui;
        private Dictionary<string, List<Element>> elemsById = new();
        private List<Element> allElements;
        private Interpreter interpreter;
        private List<Element> tooltip = new();

        private static Func<Element, Value> makeElemMap;

        public ExtensionMenu(UiContentModel uiModel)
        {
            origModel = uiModel;
            ui = ( RootElement ) uiModel.CreateUi(out allElements, out _ ); // can ignore last one since it has to be root element
            foreach (var elem in allElements)
            {
                InitElement(elem);
            }

            interpreter = ExtensionEngine.SetupInterpreter();
            interpreter.hostData = this;
            AdditionalSetup();

            // The below string isn't a constant so that I can edit + hot reload
            interpreter.Reset(uiModel.Script + @"
_events = []
while true
    while _events
        _nextEvent = _events.pull
        if _nextEvent.hasIndex(""arguments"") then
            _nextEvent.func _nextEvent.arguments
        else
            _nextEvent.func
        end if
    end while
    yield
end while
");
            interpreter.Compile();

            Value initFunc = interpreter.GetGlobalValue("init");
            if (initFunc == null) return;
            var events = interpreter.GetGlobalValue("_events") as ValList;
            if (events == null) return;

            ValMap callInit = new();
            callInit.map.Add(new ValString("func"), initFunc);
            events.values.Add(callInit);

            interpreter.RunUntilDone(0.01);
        }

        private void InitElement(Element elem)
        {
            var extra = elem.UserData as UiExtraData;
            if (extra.Id != null)
            {
                if (!elemsById.ContainsKey(extra.Id))
                    elemsById.Add(extra.Id, new());
                elemsById[extra.Id].Add(elem);
            }
            if (extra.OnClickFunction != null)
            {
                var callbackProp = elem.GetType().GetProperty("Callback");
                if (callbackProp == null || callbackProp.PropertyType != typeof(Action<Element>))
                {
                    Log.Warn($"In {origModel.UiFile}, element {elem} was given click callback but does not support that");
                }
                else
                {
                    Action<Element> func = (elem) =>
                    {
                        // TODO: Pass element into this
                        Value callFunc = interpreter.GetGlobalValue(extra.OnClickFunction);
                        if (callFunc == null)
                        {
                            Log.Warn($"In {origModel.UiFile}, failed to find click function {extra.OnClickFunction} in {origModel.ScriptFile}");
                            return;
                        }
                        var events = interpreter.GetGlobalValue("_events") as ValList;
                        if (events == null)
                        {
                            Log.Warn("No events queue???");
                            return;
                        }

                        ValMap args = new();
                        args.map.Add(new ValString("element"), makeElemMap(elem));
                        ValMap call = new();
                        call.map.Add(new ValString("func"), callFunc);
                        call.map.Add(new ValString("arguments"), args);

                        events.values.Add(call);
                    };
                    callbackProp.SetValue(elem, func);
                }
            }
            if (extra.TooltipTitle != null || extra.TooltipText != null)
                tooltip.Add(elem);
        }

        // TODO: Change this when I can attach them to just the interpreter
        static bool didIntrinsics = false;
        private void AdditionalSetup()
        {
            if (didIntrinsics) return;
            didIntrinsics = true;
            var cgc = Intrinsic.Create("__containerGetChildren");
            var cmc = Intrinsic.Create("__containerMakeChild");
            var egp = Intrinsic.Create("__elementGetParent");
            makeElemMap = (Element elem) =>
            {
                ValMap ret = new();
                ret.map.Add(new ValString("__elem"), new ValUiElement(elem));
                ret.map.Add(new ValString("x"), new ValNumber(elem.LocalPosition.X));
                ret.map.Add(new ValString("y"), new ValNumber(elem.LocalPosition.Y));
                ret.map.Add(new ValString("getParent"), egp.GetFunc());
                ret.map.Add(new ValString("scriptData"), (elem.UserData as UiExtraData).ScriptData);
                
                if (elem is Container)
                {
                    ret.map.Add(new ValString("getChildren"), cgc.GetFunc());
                    ret.map.Add(new ValString("makeChild"), cmc.GetFunc());
                }
                ret.assignOverride = (key, val) =>
                {
                    elem = elem;
                    switch (key.ToString())
                    {
                        case "x":
                            elem.LocalPosition = new(val.FloatValue(), elem.LocalPosition.Y);
                            return true;
                        case "y":
                            elem.LocalPosition = new(elem.LocalPosition.X, val.FloatValue());
                            return true;
                        case "scriptData":
                            (elem.UserData as UiExtraData).ScriptData = val;
                            return true;
                    }
                    return true;
                };
                return ret;
            };

            var i = Intrinsic.Create("getElementsWithId");
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
                    ret.values.Add(makeElemMap(elem));
                }
                return new Intrinsic.Result(ret);
            };

            cgc.code = (ctx, prevResult) =>
            {
                var map = ctx.self as ValMap;
                var elem = (map.map[new ValString("__elem")] as ValUiElement).Element as Container;

                ValList ret = new();
                foreach (var child in elem.Children)
                {
                    ret.values.Add(makeElemMap(child));
                }

                return new Intrinsic.Result(ret);
            };

            cmc.AddParam("type");
            cmc.AddParam("params");
            cmc.code = (ctx, prevResult) =>
            {
                var map = ctx.self as ValMap;
                var parent = (map.map[new ValString("__elem")] as ValUiElement).Element as Container;

                var menu = ctx.interpreter.hostData as ExtensionMenu;

                string type = ctx.GetVar("type").ToString();
                ValMap ps = ctx.GetVar("params") as ValMap;

                // quick hack
                UiDeserializer ud = new();
                if (!ud.types.ContainsKey(type))
                {
                    Log.Warn($"Script {menu.origModel.ScriptFile} tried to create UI element type {type}, which does not exist");
                    return Intrinsic.Result.Null;
                }

                Element elem = (Element) ud.types[type].GetConstructor(new Type[0]).Invoke(new object[0]);
                elem.UserData = new UiExtraData();
                foreach (var entry in ps.map)
                {
                    List<string> extra = new();
                    ud.LoadPropertyToElement(menu.origModel.ScriptFile.Substring(0, menu.origModel.ScriptFile.IndexOf('/')), elem, entry.Key.ToString(), entry.Value.ToString(), extra);
                    if (extra.Contains("CenterH"))
                    {
                        Vector2 size = new( Game1.viewport.Size.Width, Game1.viewport.Size.Height );
                        if (parent != null)
                        {
                            size = parent.Bounds.Size.ToVector2();
                        }
                        elem.LocalPosition += new Vector2( (size - elem.Bounds.Size.ToVector2()).X / 2, 0 );
                    }
                    if (extra.Contains("CenterV"))
                    {
                        Vector2 size = new(Game1.viewport.Size.Width, Game1.viewport.Size.Height);
                        if (parent != null)
                        {
                            size = parent.Bounds.Size.ToVector2();
                        }
                        elem.LocalPosition += new Vector2(0, (size - elem.Bounds.Size.ToVector2()).Y / 2);
                    }
                }
                parent.AddChild(elem);
                menu.InitElement(elem);

                return new Intrinsic.Result(makeElemMap(elem));
            };

            egp.code = (ctx, prevResult) =>
            {
                var map = ctx.self as ValMap;
                var elem = (map.map[new ValString("__elem")] as ValUiElement).Element;
                return elem.Parent == null ? Intrinsic.Result.Null : new Intrinsic.Result(makeElemMap(elem.Parent));
            };

            i = Intrinsic.Create("getRoot");
            i.code = (ctx, prevResult) =>
            {
                var menu = ctx.interpreter.hostData as ExtensionMenu;
                var elem = menu.ui;

                return new Intrinsic.Result(makeElemMap(elem));
            };
        }

        private ValMap MakeContext()
        {
            return new ValMap();
        }

        public override void update(GameTime time)
        {
            base.update(time);

            ui.Update();

            Value updateFunc = interpreter.GetGlobalValue("update");
            if (updateFunc != null)
            {
                var events = interpreter.GetGlobalValue("_events") as ValList;
                if (events != null)
                {
                    //events.values.Add(updateFunc);

                    ValMap callUpdate = new();
                    callUpdate.map.Add(new ValString("func"), updateFunc);
                    //callUpdate.map.Add(new ValString("arguments"), ValNull.instance);
                    events.values.Add(callUpdate);
                }
            }

            interpreter.RunUntilDone(0.01);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            ui.Draw(b);

            foreach (var elem in tooltip)
            {
                if (elem.Hover)
                {
                    var extra = elem.UserData as UiExtraData;
                    if (extra.TooltipText != null && extra.TooltipTitle != null)
                        drawToolTip(b, extra.TooltipText, extra.TooltipTitle, null);
                    else if (extra.TooltipText != null || extra.TooltipTitle != null)
                        drawHoverText(b, extra.TooltipText ?? extra.TooltipTitle, Game1.smallFont);
                }
            }

            drawMouse(b);
        }
    }
}
