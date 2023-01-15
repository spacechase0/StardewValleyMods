using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Miniscript;
using SpaceCore.Framework.ExtEngine;
using SpaceCore.Framework.ExtEngine.Script;

namespace SpaceCore.Framework.ExtEngine
{
    internal static partial class ExtensionEngine
    {

    }
}

namespace StardewValley.Tools
{
    [XmlType("Mods_spacechase0_ScriptTool")]
    internal class ScriptTool : GenericTool
    {
        // TODO: Net stuff
        private string correspondingScript;
        private bool scriptDirty = true;

        private Interpreter interpreter;

        public string CorrespondingScript
        {
            get => correspondingScript;
            set { correspondingScript = value; scriptDirty = true; }
        }

        public ScriptTool()
        {
        }

        public void DoCheckAndUpdateScript()
        {
            if (!scriptDirty || correspondingScript == null)
            {
                //goto doUpdate;
            }
            scriptDirty = false;

            interpreter = ExtensionEngine.SetupInterpreter();
            interpreter.hostData = this;

            string script = File.ReadAllText(SpaceShared.Util.FetchFullPath(SpaceCore.SpaceCore.Instance.Helper.ModRegistry, correspondingScript));
            script += @"
_events = []
_lastReturn = null
while true
    while _events
        _nextEvent = _events.pull
        if _nextEvent.hasIndex(""arguments"") then
            _lastReturn = _nextEvent.func(_nextEvent.arguments)
        else
            _lastReturn = _nextEvent.func
        end if
    end while
    yield
end while
";
            interpreter.Reset(script);
            interpreter.Compile();

            interpreter.RunUntilDone(0.01); // First run

        doUpdate:
            ValList attachs = new();
            for (int i = 0; i < attachmentSlots(); ++i)
            {
                attachs.values.Add(ExtensionEngine.makeItemMap(attachments[i]));
            }
            interpreter.SetGlobalValue("attachments", attachs);
        }

        public void SyncScriptToWorld()
        {
            ValList attachs = interpreter.GetGlobalValue("attachments") as ValList;
            if (attachs != null)
            {
                for (int i = 0; i < attachmentSlots(); ++i)
                {
                    var val = attachs.values[i];
                    if (val is ValNull)
                        attachments[0] = null;
                    else if (val is ValMap vmap && vmap.map[new ValString("__item")] is ValItem vitem)
                        attachments[0] = vitem.item as StardewValley.Object;
                }
            }
        }

        public override Item getOne()
        {
            ScriptTool ret = new();
            // Should we be doing a LOT more here?
            ret.CorrespondingScript = CorrespondingScript;
            ret._GetOneFrom(this);
            return ret;
        }

        public override bool canThisBeAttached(Object o)
        {
            try
            {
                DoCheckAndUpdateScript();

                if (o == null)
                    return true;

                Value validateAttachmentFunc = interpreter.GetGlobalValue("validateAttachment");
                if (validateAttachmentFunc != null)
                {
                    var events = interpreter.GetGlobalValue("_events") as ValList;
                    if (events != null)
                    {
                        var item = ExtensionEngine.makeItemMap(o);

                        for (int i = 0; i < attachmentSlots(); ++i)
                        {
                            ValMap args = new();
                            args.map.Add(new ValString("slotIndex"), new ValNumber(i));
                            args.map.Add(new ValString("item"), item);

                            ValMap call = new();
                            call.map.Add(new ValString("func"), validateAttachmentFunc);
                            call.map.Add(new ValString("arguments"), args);
                            events.values.Add(call);

                            interpreter.RunUntilDone(0.001);

                            bool allowed = interpreter.GetGlobalValue("_lastReturn").BoolValue();

                            if (!allowed)
                                continue;
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    for (int i = 0; i < attachmentSlots(); ++i)
                    {
                        if (attachments[i] == null || attachments[i].canStackWith(o) && attachments[i].Stack < attachments[i].maximumStackSize())
                        {
                            return true;
                        }
                    }
                    return base.canThisBeAttached(o);
                }
            }
            finally
            {
                SyncScriptToWorld();
            }
        }

        public override Object attach(Object o)
        {
            try
            {
                DoCheckAndUpdateScript();

                if (o == null)
                {
                    for (int i = 0; i < attachmentSlots(); ++i)
                    {
                        if (attachments[i] != null)
                        {
                            Game1.playSound("dwop");
                            var ret = attachments[i];
                            attachments[i] = null;
                            return ret;
                        }
                    }

                    return o;
                }
                else
                {
                    Value validateAttachmentFunc = interpreter.GetGlobalValue("validateAttachment");
                    if (validateAttachmentFunc != null)
                    {
                        var events = interpreter.GetGlobalValue("_events") as ValList;
                        if (events != null)
                        {
                            var item = ExtensionEngine.makeItemMap(o);

                            for (int i = 0; i < attachmentSlots(); ++i)
                            {
                                ValMap args = new();
                                args.map.Add(new ValString("slotIndex"), new ValNumber(i));
                                args.map.Add(new ValString("item"), item);

                                ValMap call = new();
                                call.map.Add(new ValString("func"), validateAttachmentFunc);
                                call.map.Add(new ValString("arguments"), args);
                                events.values.Add(call);

                                interpreter.RunUntilDone(0.001);

                                bool allowed = interpreter.GetGlobalValue("_lastReturn").BoolValue();

                                if (!allowed) // Not allowed in this slot
                                    continue;

                                Game1.playSound("button1");
                                if (attachments[i] == null)
                                {
                                    attachments[i] = o;
                                    return null;
                                }
                                else if (attachments[i].canStackWith(o) && attachments[i].Stack < attachments[i].maximumStackSize())
                                {
                                    int leftover = attachments[i].addToStack(o);
                                    o.Stack = leftover;
                                    if (o.Stack == 0)
                                        return null;
                                    return o;
                                }
                            }
                        }

                        return o;
                    }
                    else
                    {
                        for (int i = 0; i < attachmentSlots(); ++i)
                        {
                            if (attachments[i] == null)
                            {
                                Game1.playSound("button1");
                                attachments[i] = o;
                                return null;
                            }
                            else if (attachments[i].canStackWith(o))
                            {
                                Game1.playSound("button1");
                                o.Stack = attachments[i].addToStack(o);
                                if (o.Stack == 0)
                                    return null;
                                else
                                    return o;
                            }
                        }

                        return o;
                    }
                }
            }
            finally
            {
                SyncScriptToWorld();
            }
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            int ix = x, iy = y;
            for (int i = 0; i < attachmentSlots(); ++i)
            {
                b.Draw(Game1.menuTexture, new Vector2(ix, iy), new Rectangle(128, 128, 64, 64), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0.86f);
                attachments[i]?.drawInMenu(b, new Vector2(x, y), 1);
                y += 64 + 4;
            }
        }

        public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
        {
            lastUser = who;
            if (!instantUse)
            {
                who.Halt();
                Update(who.FacingDirection, 0, who);
                if (upgradeLevel <= 0)
                {
                    who.EndUsingTool();
                    return true;
                }
            }

            if (instantUse)
            {
                Game1.toolAnimationDone(who);
                who.canReleaseTool = false;
                who.UsingTool = false;
            }
            else
            {
                switch (who.FacingDirection)
                {
                    case 0:
                        who.FarmerSprite.setCurrentFrame(176);
                        this.Update(0, 0, who);
                        break;
                    case 1:
                        who.FarmerSprite.setCurrentFrame(168);
                        this.Update(1, 0, who);
                        break;
                    case 2:
                        who.FarmerSprite.setCurrentFrame(160);
                        this.Update(2, 0, who);
                        break;
                    case 3:
                        who.FarmerSprite.setCurrentFrame(184);
                        this.Update(3, 0, who);
                        break;
                }
            }

            return false;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
        {
            try
            {
                DoCheckAndUpdateScript();

                Value doFunctionFunc = interpreter.GetGlobalValue("doFunction");
                if (doFunctionFunc != null)
                {
                    var events = interpreter.GetGlobalValue("_events") as ValList;

                    ValMap args = new();
                    args.map.Add(new ValString("location"), ExtensionEngine.makeLocationMap(location));
                    args.map.Add(new ValString("farmer"), ExtensionEngine.makeFarmerMap(who));
                    args.map.Add(new ValString("x"), new ValNumber(x));
                    args.map.Add(new ValString("y"), new ValNumber(y));
                    args.map.Add(new ValString("power"), new ValNumber(who.toolPower));

                    ValMap call = new();
                    call.map.Add(new ValString("func"), doFunctionFunc);
                    call.map.Add(new ValString("arguments"), args);
                    events.values.Add(call);

                    interpreter.RunUntilDone(0.01);
                }
            }
            finally
            {
                SyncScriptToWorld();
            }
        }

        public override void draw(SpriteBatch b)
        {
        }
    }
}
