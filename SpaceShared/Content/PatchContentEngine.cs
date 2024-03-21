using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Minigames;

#if IS_SPACECORE
namespace SpaceCore.Content
{
#else
namespace SpaceShared.Content
{
#endif
    public abstract class ContentEntry
    {
        private static Token AlwaysTrue = new Token() { Value = "TRUE" };

        public PatchContentEngine Engine { get; }
        public Statement Original { get; }

        public string Id { get; set; } = "";
        public string File { get; set; }
        public SourceElement Path { get; set; }
        public SourceElement Condition { get; set; } = AlwaysTrue;
        public int Priority { get; set; } = (int)AssetLoadPriority.Medium;

        protected bool ConditionsMet { get; private set; }

        public ContentEntry(PatchContentEngine engine, Statement statement)
        {
            Engine = engine;
            Original = statement;

            if (Original.FuncCall.Parameters.Count < 1)
                throw new ArgumentException($"Not enough parameters to Entry function at {Original.FilePath}:{Original.Line}:{Original.Column}");
            if (Original.FuncCall.Parameters[0] is not Token)
                throw new ArgumentException($"Entry function file parameter must be a string at {Original.FilePath}:{Original.Line}:{Original.Column}");
            if (Original.FuncCall.Parameters.Count >= 2 && Original.FuncCall.Parameters[1] is not Token and not Statement)
                throw new ArgumentException($"Entry function path parameter, if provided, must be a string at {Original.FuncCall.Parameters[1].FilePath}:{Original.FuncCall.Parameters[1].Line}:{Original.FuncCall.Parameters[1].Column}");
            if (Original.FuncCall.Parameters.Count >= 3 && Original.FuncCall.Parameters[2] is not Token and not Statement)
                throw new ArgumentException($"Entry function condition parameter, if provided, must be a string at {Original.FuncCall.Parameters[2].FilePath}:{Original.FuncCall.Parameters[2].Line}:{Original.FuncCall.Parameters[2].Column}");
            if (Original.FuncCall.Parameters.Count >= 4 && Original.FuncCall.Parameters[3] is not Token)
                throw new ArgumentException($"Entry function priority parameter, if provided, must be 'Low', 'Medium', 'High', 'Exclusive', or an integer, at {Original.FuncCall.Parameters[3].FilePath}:{Original.FuncCall.Parameters[3].Line}:{Original.FuncCall.Parameters[3].Column}");
            if (Original.FuncCall.Parameters.Count >= 5 && Original.FuncCall.Parameters[4] is not Token)
                throw new ArgumentException($"Entry function ID parameter, if provided, must be a string, at {Original.FuncCall.Parameters[4].FilePath}:{Original.FuncCall.Parameters[4].Line}:{Original.FuncCall.Parameters[4].Column}");

            File = (Original.FuncCall.Parameters[0] as Token).Value.Replace('\\', '/');
            if (Original.FuncCall.Parameters.Count >= 2)
                Path = Original.FuncCall.Parameters[1];
            if (Original.FuncCall.Parameters.Count >= 3)
                Condition = Original.FuncCall.Parameters[2];
            if (Original.FuncCall.Parameters.Count >= 4)
            {
                switch ((Original.FuncCall.Parameters[3] as Token).Value)
                {
                    case "Low": Priority = (int)AssetLoadPriority.Low; break;
                    case "Medium": Priority = (int)AssetLoadPriority.Medium; break;
                    case "High": Priority = (int)AssetLoadPriority.High; break;
                    case "Exclusive": Priority = (int)AssetLoadPriority.Exclusive; break;
                    default:
                        if (!int.TryParse((Original.FuncCall.Parameters[3] as Token).Value, out int priority))
                            throw new ArgumentException($"Entry function priority parameter, if provided, be 'Low', 'Medium', 'High', 'Exclusive', or an integer, at {Original.FuncCall.Parameters[3].FilePath}:{Original.FuncCall.Parameters[3].Line}:{Original.FuncCall.Parameters[3].Column}");
                        Priority = priority;
                        break;
                }
            }
            if (Original.FuncCall.Parameters.Count >= 5)
            {
                Id = (Original.FuncCall.Parameters[4] as Token).Value;
            }
        }

        public void CheckConditions()
        {
            Token condition = Condition.SimplifyToToken(Engine);

            if (condition.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                ConditionsMet = true;
            else if (condition.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                ConditionsMet = false;
            else
                ConditionsMet = GameStateQuery.CheckConditions(condition.Value);
        }

        public void Process(AssetRequestedEventArgs e)
        {
            if (ConditionsMet)
                ProcessImpl(e);
        }

        public abstract void ProcessImpl(AssetRequestedEventArgs e);

        protected void Populate(object obj, Block block)
        {
            foreach (var entry in block.Contents.Keys)
            {
                string key = entry.Value;

                var members = obj.GetType().GetMember(key);
                foreach (var member in members)
                {
                    if (member is PropertyInfo prop)
                    {
                        object val = CreateFromSourceElement(prop.PropertyType, block.Contents[entry]);
                        prop.SetValue(obj, val);
                        break;
                    }
                    else if (member is FieldInfo field)
                    {
                        object val = CreateFromSourceElement(field.FieldType, block.Contents[entry]);
                        field.SetValue(obj, val);
                        break;
                    }
                }
            }
        }

        protected object CreateFromSourceElement(Type type, SourceElement se)
        {
            if (type.IsPrimitive)
            {
                string str = se.SimplifyToToken(Engine).Value;
                return Convert.ChangeType(str, type);
            }
            else if (type == typeof(string))
            {
                return se.SimplifyToToken(Engine).Value;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                if (se is Token tok && tok.IsNull())
                {
                    return null;
                }
                else
                {
                    if (se is not Block block)
                        throw new ArgumentException($"Looking for a block at {se.FilePath}:{se.Line}:{se.Column}");

                    IDictionary dict = (IDictionary)Activator.CreateInstance(type);
                    foreach (var kvp in block.Contents)
                    {
                        dict.Add(CreateFromSourceElement(type.GetGenericArguments()[0], kvp.Key),
                                 CreateFromSourceElement(type.GetGenericArguments()[1], kvp.Value));
                    }
                    return dict;
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (se is Token tok && tok.IsNull())
                {
                    return null;
                }
                else
                {
                    if (se is not Array array)
                        throw new ArgumentException($"Looking for an array at {se.FilePath}:{se.Line}:{se.Column}");

                    IList list = (IList)Activator.CreateInstance(type);
                    foreach (var val in array.Contents)
                    {
                        list.Add(CreateFromSourceElement(type.GetGenericArguments()[0], val));
                    }
                    return list;
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (se is Token tok)
                {
                    if (tok.IsNull())
                        return null;
                }
                else if (se is Statement statement)
                {
                    // Terrible way to do this but I'm getting tired
                    try
                    {
                        var t = statement.SimplifyToToken(Engine);
                        if (t.IsNull())
                            return null;
                    }
                    catch (ArgumentException e) { }
                }

                var innerType = type.GetGenericArguments()[0];
                return CreateFromSourceElement(innerType, se);
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, se.SimplifyToToken(Engine).Value);
            }
            else
            {
                if (se is Token tok && tok.IsNull())
                {
                    return null;
                }
                else
                {
                    if (se is not Block block)
                        throw new ArgumentException($"Looking for a block at {se.FilePath}:{se.Line}:{se.Column}");

                    object obj = Activator.CreateInstance(type);
                    Populate(obj, block);

                    // TODO: A version that checks for any method with [OnDeserialized]
                    var desMethod = obj.GetType().GetMethod("OnDeserialized", new[] { typeof(StreamingContext) } );
                    if (desMethod != null)
                    {
                        desMethod.Invoke(obj, new object[]
                        {
                            new StreamingContext( StreamingContextStates.File )
                        });
                    }

                    return obj;
                }
            }
        }
    }

    public class EntryContentEntry : ContentEntry
    {
        public Block Data { get; set; }
        public EntryContentEntry(PatchContentEngine engine, Statement statement)
        : base(engine, statement)
        {
            if (Original.FuncCall.Function != "Entry")
                throw new ArgumentException("Not a content entry?");
            if (Original.Data is not Block block)
                throw new ArgumentException($"Entry function data must be a block at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");

            Data = block;
        }

        public override void ProcessImpl(AssetRequestedEventArgs e)
        {
            e.Edit((asset) =>
            {
                Queue<string> parts = new Queue<string>(Path.SimplifyToToken(Engine).Value.Split('/'));

                object target = FindTarget(asset.Data, parts);
                if (parts.Count == 1)
                {
                    if ( target.GetType().IsGenericType &&
                         target.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        object newObj = Activator.CreateInstance(target.GetType().GetGenericArguments()[1]);
                        (target as IDictionary).Add(parts.Peek(), newObj);
                        target = newObj;
                    }
                    else
                    {
                        string key = parts.Peek();
                        var members = target.GetType().GetMember(key);
                        bool created = false;
                        foreach (var member in members)
                        {
                            if (member is PropertyInfo prop)
                            {
                                object obj = Activator.CreateInstance(prop.PropertyType);
                                prop.SetValue(target, obj);
                                target = obj;
                                created = true;
                                break;
                            }
                            else if (member is FieldInfo field)
                            {
                                object obj = Activator.CreateInstance(field.FieldType);
                                field.SetValue(target, obj);
                                target = obj;
                                created = true;
                                break;
                            }
                        }

                        if (!created)
                            throw new ArgumentException($"Failed to find entry \"{key}\" at {Path.FilePath}:{Path.Line}:{Path.Column}");
                    }
                }

                Populate(target, Data);
            }, (AssetEditPriority)Priority);
        }

        private object FindTarget(object target, Queue<string> parts)
        {
            if (parts.Count == 0)
                return target;

            if ( target is IDictionary dict )
            {
                string key = parts.Peek();
                if (!dict.Contains(key))
                {
                    return dict;
                }
                else
                {
                    parts.Dequeue();
                    return FindTarget(dict[key], parts);
                }
            }
            else if ( target is IList list )
            {
                string key = parts.Peek();
                if (!int.TryParse(key, out int i))
                    throw new ArgumentException($"Path for Entry at {Original.FilePath}:{Original.Line}:{Original.Column}");
                else if ( i >= list.Count )
                {
                    return list;
                }
                else
                {
                    parts.Dequeue();
                    return FindTarget(list[i], parts);
                }
            }
            else
            {
                string key = parts.Peek();
                var members = target.GetType().GetMember(key);
                foreach (var member in members)
                {
                    if (member is PropertyInfo prop && prop.GetValue(target) != null)
                    {
                        parts.Dequeue();
                        return FindTarget(prop.GetValue(target), parts);
                    }
                    else if (member is FieldInfo field && field.GetValue(target) != null)
                    {
                        parts.Dequeue();
                        return FindTarget(field.GetValue(target), parts);
                    }
                }
            }

            return null;
        }
    }

    public class LoadContentEntry : ContentEntry
    {
        private static MethodInfo loadFromModFile;

        public LoadContentEntry(PatchContentEngine engine, Statement statement)
        : base(engine, statement)
        {
            if (Original.FuncCall.Function != "Load")
                throw new ArgumentException("Not a content load?");
        }

        public override void ProcessImpl(AssetRequestedEventArgs e)
        {
            if (loadFromModFile == null)
                loadFromModFile = e.GetType().GetMethod(nameof(AssetRequestedEventArgs.LoadFromModFile)).MakeGenericMethod(e.DataType);

            if ( e.DataType == typeof(Texture2D) )
                loadFromModFile.Invoke(e, new object[] { Path.SimplifyToToken(Engine).Value, (AssetLoadPriority)Priority } );
            else
            {
                e.LoadFrom(() =>
                {
                    return CreateFromSourceElement(e.DataType, Original.Data);
                }, (AssetLoadPriority)Priority);
            }
        }
    }

    public class PatchContentEngine : ContentEngine
    {
        public List<ContentEntry> Entries { get; } = new();
        public Dictionary<string, List<ContentEntry>> EntriesById { get; } = new();
        private Dictionary<string, List<ContentEntry>> EntriesByEditedFile { get; } = new();

        public PatchContentEngine(IManifest manifest, IModHelper helper, string contentRootFile)
        :   base( manifest, helper, contentRootFile )
        {
            Execute();

            Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
        }

        protected override void PostReload()
        {
            base.PostReload();
            Entries.Clear();
            EntriesById.Clear();
            EntriesByEditedFile.Clear();

            Execute();
        }

        private void Execute()
        {
            foreach (var se in Contents.Contents)
            {
                if (se is Statement statement)
                {
                    ContentEntry ce = null;
                    if (statement.FuncCall.Function == "Entry")
                        ce = new EntryContentEntry(this, statement);
                    else if (statement.FuncCall.Function == "Load")
                        ce = new LoadContentEntry(this, statement);
                    else continue;

                    Entries.Add(ce);

                    if (!EntriesByEditedFile.TryGetValue(ce.File, out var entries))
                        EntriesByEditedFile.Add(ce.File, entries = new());
                    entries.Add(ce);

                    if (!EntriesById.TryGetValue(ce.Id, out entries))
                        EntriesById.Add(ce.Id, entries = new());
                    entries.Add(ce);

                    ce.CheckConditions(); // Just to set the "TRUE" ones to true on the main menu
                }
            }

            InvalidateUsedAssets();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!EntriesByEditedFile.TryGetValue(e.Name.Name, out var entries))
                return;

            foreach ( var entry in entries )
            {
                entry.Process(e);
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var entry in Entries)
                entry.CheckConditions();
        }

        private void InvalidateUsedAssets()
        {
            Helper.GameContent.InvalidateCache(a => EntriesByEditedFile.Keys.Contains(a.Name.BaseName.Replace('\\', '/')));
        }
    }
}
