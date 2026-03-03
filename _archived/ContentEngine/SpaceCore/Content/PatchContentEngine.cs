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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceCore.Content.Functions;
using SpaceCore.Content.StardewFunctions;
using SpaceShared;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Minigames;

namespace SpaceCore.Content
{
    public abstract class ContentEntry
    {
        private static Token AlwaysTrue = new Token() { Value = "TRUE", IsString = true };

        public PatchContentEngine Engine { get; }
        public Statement Original { get; }

        public string Id { get; set; } = "";
        public string File { get; set; }
        public SourceElement Path { get; set; }
        public SourceElement Condition { get; set; } = AlwaysTrue;
        public int Priority { get; set; } = (int)AssetLoadPriority.Medium;
        public RefreshTime Refresh { get; set; } = RefreshTime.Daily;

        protected internal bool ConditionsMet { get; private set; }
        protected internal bool NeedsRefresh { get; set; } = false;

        public enum RefreshTime
        {
            Daily, // Or title screen
        };

        private static Token ConditionKey = new() { Value = "Condition", IsString = true };
        private static Token PriorityKey = new() { Value = "Priority", IsString = true };
        private static Token IdKey = new() { Value = "Id", IsString = true };
        private static Token DefaultId = new() { Value = "", IsString = true };
        private static Token RefreshKey = new() { Value = "Refresh", IsString = true };
        private static Token DefaultRefresh = new() { Value = "Daily", IsString = true };

        public ContentEntry(PatchContentEngine engine, Statement statement)
        {
            Engine = engine;
            Original = statement;

            if (Original.FuncCall.Parameters.Count < 1)
                throw new ArgumentException($"Not enough parameters to content entry function at {Original.FilePath}:{Original.Line}:{Original.Column}");
            if (Original.FuncCall.Parameters[0] is not Token)
                throw new ArgumentException($"Content entry file parameter must be a string at {Original.FilePath}:{Original.Line}:{Original.Column}");
            if (Original.FuncCall.Parameters.Count >= 2 && Original.FuncCall.Parameters[1] is not Token and not Statement)
                throw new ArgumentException($"Content entry function path parameter, if provided, must be a string at {Original.FuncCall.Parameters[1].FilePath}:{Original.FuncCall.Parameters[1].Line}:{Original.FuncCall.Parameters[1].Column}");
            /*
            if (Original.FuncCall.Parameters.Count >= 3 && Original.FuncCall.Parameters[2] is not Token and not Statement)
                throw new ArgumentException($"Entry function condition parameter, if provided, must be a string at {Original.FuncCall.Parameters[2].FilePath}:{Original.FuncCall.Parameters[2].Line}:{Original.FuncCall.Parameters[2].Column}");
            if (Original.FuncCall.Parameters.Count >= 4 && Original.FuncCall.Parameters[3] is not Token)
                throw new ArgumentException($"Entry function priority parameter, if provided, must be 'Low', 'Medium', 'High', 'Exclusive', or an integer, at {Original.FuncCall.Parameters[3].FilePath}:{Original.FuncCall.Parameters[3].Line}:{Original.FuncCall.Parameters[3].Column}");
            if (Original.FuncCall.Parameters.Count >= 5 && Original.FuncCall.Parameters[4] is not Token)
                throw new ArgumentException($"Entry function ID parameter, if provided, must be a string, at {Original.FuncCall.Parameters[4].FilePath}:{Original.FuncCall.Parameters[4].Line}:{Original.FuncCall.Parameters[4].Column}");
            */
            Block extras = new();
            if (Original.FuncCall.Parameters.Count >= 3 && Original.FuncCall.Parameters[2] is not Block block)
                throw new ArgumentException($"Third parameter of a content entry must be a block containing the extra arguments if provided, at {Original.FuncCall.Parameters[2].FilePath}:{Original.FuncCall.Parameters[2].Line}:{Original.FuncCall.Parameters[2].Column}");
            else if (Original.FuncCall.Parameters.Count >= 3)
                extras = Original.FuncCall.Parameters[2] as Block;

            File = (Original.FuncCall.Parameters[0] as Token).Value.Replace('\\', '/');
            if (Original.FuncCall.Parameters.Count >= 2)
                Path = Original.FuncCall.Parameters[1];
            Condition = extras.Contents.GetOrDefault(ConditionKey, AlwaysTrue);
            if (extras.Contents.TryGetValue(PriorityKey, out var priorityElem))
            {
                switch ((priorityElem as Token).Value)
                {
                    case "Low": Priority = (int)AssetLoadPriority.Low; break;
                    case "Early": Priority = (int)AssetEditPriority.Early; break;
                    case "Medium": Priority = (int)AssetLoadPriority.Medium; break;
                    case "Default": Priority = (int)AssetEditPriority.Default; break;
                    case "High": Priority = (int)AssetLoadPriority.High; break;
                    case "Late": Priority = (int)AssetEditPriority.Late; break;
                    case "Exclusive": Priority = (int)AssetLoadPriority.Exclusive; break;
                    default:
                        if (!int.TryParse((priorityElem as Token).Value, out int priority))
                            throw new ArgumentException($"Entry function priority parameter, if provided, be 'Low'/'Early', 'Medium'/'Default', 'High'/'Late', 'Exclusive', or an integer, at {priorityElem.FilePath}:{priorityElem.Line}:{priorityElem.Column}");
                        Priority = priority;
                        break;
                }
            }
            Id = (extras.Contents.GetOrDefault(IdKey, DefaultId) as Token).Value;
            Refresh = Enum.Parse<RefreshTime>((extras.Contents.GetOrDefault(RefreshKey, DefaultRefresh) as Token).Value);
        }

        public bool CheckConditionsOrRefresh(RefreshTime time)
        {
            bool oldConditionsMet = ConditionsMet;

            Token condition = Condition.SimplifyToToken(Engine, allowLateResolve: true);
            Token path = Path?.SimplifyToToken(Engine, allowLateResolve: true);

            if (condition == null || (Path != null && path == null))
                ConditionsMet = false;
            else
                ConditionsMet = Engine.CheckCondition(condition);

            if (ConditionsMet && !ValidateData())
                ConditionsMet = false;

            if (ConditionsMet != oldConditionsMet)
            {
                //Log.Debug("Condition status changed: " + File + " " + path?.Value);
            }

            return ConditionsMet != oldConditionsMet || WantsRefresh(time);
        }

        protected virtual bool ValidateData()
        {
            return true;
        }

        protected virtual bool WantsRefresh(RefreshTime time)
        {
            return Refresh == time && NeedsRefresh;
        }

        public void Process(AssetRequestedEventArgs e)
        {
            if (ConditionsMet)
                ProcessImpl(e);
            NeedsRefresh = false;
        }

        public abstract void ProcessImpl(AssetRequestedEventArgs e);

        protected void Populate(object obj, Block block)
        {
            var type = obj.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                IDictionary dict = (IDictionary) obj;
                foreach (var kvp in block.Contents)
                {
                    dict.Add(CreateFromSourceElement(type.GetGenericArguments()[0], kvp.Key),
                             CreateFromSourceElement(type.GetGenericArguments()[1], kvp.Value));
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList list = (IList)obj;
                list.Add(CreateFromSourceElement(type.GetGenericArguments()[0], block));
            }
            else
            {
                Dictionary<MemberInfo, Tuple<Token, object>> data = new();
                foreach (var entry in block.Contents.Keys)
                {
                    string key = entry.Value;

                    var members = obj.GetType().GetMember(key);
                    foreach (var member in members)
                    {
                        if (member is PropertyInfo prop)
                        {
                            data.Add(member, new(entry, CreateFromSourceElement(prop.PropertyType, block.Contents[entry])));
                            break;
                        }
                        else if (member is FieldInfo field)
                        {
                            data.Add(member, new(entry, CreateFromSourceElement(field.FieldType, block.Contents[entry])));
                            break;
                        }
                    }
                }
                foreach (var kvp in data)
                {
                    MemberInfo member = kvp.Key;
                    object val = kvp.Value.Item2;
                    if (member is PropertyInfo prop)
                    {
                        prop.SetValue(obj, val);
                    }
                    else if (member is FieldInfo field)
                    {
                        field.SetValue(obj, val);
                    }
                }
            }
        }

        protected object CreateFromSourceElement(Type type, SourceElement se)
        {
            if (type.IsPrimitive)
            {
                string str = se.SimplifyToToken(Engine, allowLateResolve: true).Value;
                return Convert.ChangeType(str, type);
            }
            else if (type == typeof(string))
            {
                return se.SimplifyToToken(Engine, allowLateResolve: true).Value;
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
                        var t = statement.SimplifyToToken(Engine, allowLateResolve: true);
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
                return Enum.Parse(type, se.SimplifyToToken(Engine, allowLateResolve: true).Value);
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

        protected override bool ValidateData()
        {
            return ValidateData(Data);
        }
        private bool ValidateData(SourceElement se)
        {
            bool ret = true;
            if (se is Block block)
            {
                foreach (var val in block.Contents.Values)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Array array)
            {
                foreach (var val in array.Contents)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Statement statement)
            {
                if (Engine.SimplifyFunctions.TryGetValue(statement.FuncCall.Function, out var func) &&
                    func is IRefreshingFunction rfunc && rfunc.WouldChangeFromRefresh(statement.FuncCall, Engine))
                    NeedsRefresh = true;
                if (statement.SimplifyToToken(Engine, allowLateResolve: true) == null)
                    ret = false;
            }
            return ret;
        }

        public override void ProcessImpl(AssetRequestedEventArgs e)
        {
            e.Edit((asset) =>
            {
                Queue<string> parts = Path == null ? new Queue<string>() : new Queue<string>(Path.SimplifyToToken(Engine, allowLateResolve: true).Value.Split('/'));

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
                    return FindTarget(list[i < 0 ? (list.Count + i) : i], parts);
                }
            }
            else
            {
                string key = parts.Peek();
                var members = target.GetType().GetMember(key);
                foreach (var member in members)
                {
                    if (member is PropertyInfo prop)
                    {
                        parts.Dequeue();
                        object val = prop.GetValue(target);
                        if (val == null)
                        {
                            val = prop.PropertyType.GetConstructor([]).Invoke([]);
                            prop.SetValue(target, val);
                        }
                        return FindTarget(val, parts);
                    }
                    else if (member is FieldInfo field)
                    {
                        parts.Dequeue();
                        object val = field.GetValue(target);
                        if (val == null)
                        {
                            val = field.FieldType.GetConstructor([]).Invoke([]);
                            field.SetValue(target, val);
                        }
                        return FindTarget(val, parts);
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
        protected override bool ValidateData()
        {
            return ValidateData(Original.Data);
        }
        private bool ValidateData(SourceElement se)
        {
            bool ret = true;
            if (se is Block block)
            {
                foreach (var val in block.Contents.Values)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Array array)
            {
                foreach (var val in array.Contents)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Statement statement)
            {
                if (Engine.SimplifyFunctions.TryGetValue(statement.FuncCall.Function, out var func) &&
                    func is IRefreshingFunction rfunc && rfunc.WouldChangeFromRefresh(statement.FuncCall, Engine))
                    NeedsRefresh = true;
                if (statement.SimplifyToToken(Engine, allowLateResolve: true) == null)
                    ret = false;
            }
            return ret;
        }

        public override void ProcessImpl(AssetRequestedEventArgs e)
        {
            if (loadFromModFile == null)
                loadFromModFile = e.GetType().GetMethod(nameof(AssetRequestedEventArgs.LoadFromModFile)).MakeGenericMethod(e.DataType);

            if ( e.DataType == typeof(Texture2D) )
                loadFromModFile.Invoke(e, new object[] { Path.SimplifyToToken(Engine, allowLateResolve: true).Value, (AssetLoadPriority)Priority } );
            else
            {
                e.LoadFrom(() =>
                {
                    return CreateFromSourceElement(e.DataType, Original.Data);
                }, (AssetLoadPriority)Priority);
            }
        }
    }

    public class EditMapContentEntry : ContentEntry
    {
        private static MethodInfo loadFromModFile;

        private SourceElement Source;
        private Rectangle From;
        private Point To;
        private PatchMapMode Mode = PatchMapMode.Overlay;

        private static readonly Token SourceKey = new Token() { Value = "Source", IsString = true };
        private static readonly Token FromKey = new Token() { Value = "From", IsString = true };
        private static readonly Token ToKey = new Token() { Value = "To", IsString = true };
        private static readonly Token ModeKey = new Token() { Value = "Mode", IsString = true };
        private static readonly Token ValueKey = new Token() { Value = "Value", IsString = true };

        public EditMapContentEntry(PatchContentEngine engine, Statement statement)
        : base(engine, statement)
        {
            if (Original.FuncCall.Function != "MapPatch")
                throw new ArgumentException("Not a map edit?");
            if (Original.Data is not Block block)
                throw new ArgumentException($"MapPatch entry data must be a block containing the map patch data, at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");

            if ( !block.Contents.TryGetValue( SourceKey, out Source ) )
                throw new ArgumentException($"MapPatch entry data must contain \"Source\", the map to patch from, at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");

            if (block.Contents.TryGetValue(FromKey, out var from))
            {
                if (from is not Block fromBlock)
                    throw new ArgumentException($"MapPatch entry data \"From\", if specified, must contain the area of the map to use for the patch (use Rectangle()), at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");

                Holder<Rectangle> holder = new();
                Block tmpBlock = new();
                tmpBlock.Contents[ValueKey] = fromBlock;
                Populate(holder, tmpBlock);
                From = holder.Value;
            }
            if (block.Contents.TryGetValue(ToKey, out var to))
            {
                if (to is not Block toBlock)
                    throw new ArgumentException($"MapPatch entry data \"To\", if specified, must contain the position to patch to (use Vector2()), at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");

                Holder<Point> holder = new();
                Block tmpBlock = new();
                tmpBlock.Contents[ValueKey] = toBlock;
                Populate(holder, tmpBlock);
                To = holder.Value;
            }
            if (block.Contents.TryGetValue(ModeKey, out var mode))
            {
                string modeStr = mode.SimplifyToToken(Engine).Value;
                if ( !Enum.TryParse( modeStr, out Mode ) )
                    throw new ArgumentException($"MapPatch entry data \"Mode\" is invalid, at {Original.Data.FilePath}:{Original.Data.Line}:{Original.Data.Column}");
            }
        }
        protected override bool ValidateData()
        {
            return ValidateData(Source);
        }

        private bool ValidateData(SourceElement se)
        {
            bool ret = true;
            if (se is Block block)
            {
                foreach (var val in block.Contents.Values)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Array array)
            {
                foreach (var val in array.Contents)
                {
                    if (!ValidateData(val))
                        ret = false;
                }
            }
            else if (se is Statement statement)
            {
                if (Engine.SimplifyFunctions.TryGetValue(statement.FuncCall.Function, out var func) &&
                    func is IRefreshingFunction rfunc && rfunc.WouldChangeFromRefresh(statement.FuncCall, Engine))
                if (statement.SimplifyToToken(Engine, allowLateResolve: true) == null)
                    ret = false;
            }
            return ret;
        }

        public override void ProcessImpl(AssetRequestedEventArgs e)
        {
            e.Edit((asset) =>
            {
                var source = Engine.Helper.ModContent.Load<xTile.Map>(Source.SimplifyToToken(Engine, allowLateResolve: true).Value);

                Rectangle from = From;
                if (from == Rectangle.Empty)
                    from = new Rectangle(0, 0, source.Layers[0].LayerWidth, source.Layers[0].LayerHeight);

                var mapAsset = asset.AsMap();
                mapAsset.PatchMap(source, from, new(To.X, To.Y, from.Width, from.Height), Mode);
            }, (AssetEditPriority)Priority);
        }
    }

    public class PatchContentEngine : ContentEngine
    {
        public IManifest Manifest { get; }
        public IModHelper Helper { get; }

        internal IContentPatcherApi cp;
        internal readonly ISemanticVersion cpVersion;

        public List<ContentEntry> Entries { get; } = new();
        public Dictionary<string, List<ContentEntry>> EntriesById { get; } = new();
        private Dictionary<string, List<ContentEntry>> EntriesByEditedFile { get; } = new();

        public PatchContentEngine(IManifest manifest, IModHelper helper, string contentRootFile)
        :   base( manifest.UniqueID, helper.DirectoryPath, contentRootFile,
                (path) => helper.ModContent.GetInternalAssetName(path).Name,
                (seed, isStatic) => isStatic ? Utility.CreateRandom(seed) : Utility.CreateDaySaveRandom(seed) )
        {
            Manifest = manifest;
            Helper = helper;

            AddSimplifyFunction(new ActorFunction());
            AddSimplifyFunction(new ContentPatcherTokenFunction());
            AddSimplifyFunction(new FacingFunction());
            AddSimplifyFunction(new QuickQuestionFunction());
            AddSimplifyFunction(new HasModFunction());
            AddSimplifyFunction(new LocalizationFunction());

            cpVersion = manifest.Dependencies.FirstOrDefault(md => md.UniqueID == "Pathoschild.ContentPatcher")?.MinimumVersion;
            cp = Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");

            Helper.Events.Content.AssetRequested += this.Content_AssetRequested;
            Helper.Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            Helper.Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
        }

        public override bool CheckCondition(Token condition)
        {
            if (condition.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                return true;
            else if (condition.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return false;
            else
                return GameStateQuery.CheckConditions(condition.Value);
        }

        protected override void PostReload()
        {
            foreach (var error in LastErrors)
            {
                Log.Error($"Content error: {error}");
            }

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
                    else if (statement.FuncCall.Function == "MapPatch")
                        ce = new EditMapContentEntry(this, statement);
                    else continue;

                    Entries.Add(ce);

                    if (!EntriesByEditedFile.TryGetValue(ce.File.ToLower(), out var entries))
                        EntriesByEditedFile.Add(ce.File.ToLower(), entries = new());
                    entries.Add(ce);

                    if (!EntriesById.TryGetValue(ce.Id, out entries))
                        EntriesById.Add(ce.Id, entries = new());
                    entries.Add(ce);

                    ce.CheckConditionsOrRefresh( ContentEntry.RefreshTime.Daily ); // Just to set the "TRUE" ones to true on the main menu
                }
            }

            InvalidateUsedAssets();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!EntriesByEditedFile.TryGetValue(e.Name.Name.ToLower(), out var entries))
                return;

            foreach ( var entry in entries )
            {
                entry.Process(e);
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            RecheckPatches(ContentEntry.RefreshTime.Daily);
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            RecheckPatches(ContentEntry.RefreshTime.Daily);
        }

        private void RecheckPatches(ContentEntry.RefreshTime time)
        {
            HashSet<string> changedFiles = new();
            foreach (var entry in Entries)
            {
                if (entry.CheckConditionsOrRefresh(time))
                {
                    changedFiles.Add(entry.File.ToLower());
                }
            }

            Helper.GameContent.InvalidateCache(a => changedFiles.Contains(a.Name.BaseName.ToLower().Replace('\\', '/')));
        }

        private void InvalidateUsedAssets()
        {
            Helper.GameContent.InvalidateCache(a => EntriesByEditedFile.Keys.Contains(a.Name.BaseName.ToLower().Replace('\\', '/')));
        }
    }
}
