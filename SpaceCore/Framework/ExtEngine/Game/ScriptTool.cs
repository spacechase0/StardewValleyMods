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
                goto doUpdate;
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
            interpreter = interpreter; // placeholder until below has code
            // update attachments, mod data, etc.
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

        public override Object attach(Object o)
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
    }
}
