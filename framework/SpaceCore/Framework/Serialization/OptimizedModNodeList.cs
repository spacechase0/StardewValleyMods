using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SpaceCore.Framework.Serialization
{
    /// <summary>An optimized view of all the XML mod nodes stored in the <see cref="SerializerManager.FarmerFilename"/> or <see cref="SerializerManager.Filename"/> file.</summary>
    internal class OptimizedModNodeList
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The mod nodes in the file.</summary>
        public OptimizedModNode[] ModNodes { get; }

        /// <summary>The <see cref="OptimizedModNode.Path"/> value for every element in the save which contains one or more mod nodes.</summary>
        public ISet<string> AncestorPaths { get; }

        /// <summary>The mod nodes to insert grouped by their immediate parent node's <see cref="OptimizedModNode.Path"/>.</summary>
        public IDictionary<string, OptimizedModNode[]> ModNodesByParent { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modNodes">The mod nodes in the file.</param>
        public OptimizedModNodeList(OptimizedModNode[] modNodes)
        {
            this.ModNodes = modNodes;
            this.AncestorPaths = new HashSet<string>(modNodes.SelectMany(p => p.AncestorPaths));
            this.ModNodesByParent = modNodes
                .GroupBy(p => p.ParentPath)
                .ToDictionary(p => p.Key, p => p.ToArray());
        }

        /// <summary>Load a <see cref="OptimizedModNodeList"/> representation from a raw data file.</summary>
        /// <param name="path">The absolute path to the file to load.</param>
        public static OptimizedModNodeList LoadFromFile(string path)
        {
            // load raw data from file
            var rawNodes = File.Exists(path)
                ? JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(File.ReadAllText(path))
                : null;
            if (rawNodes == null || rawNodes.Length == 0)
                return new(Array.Empty<OptimizedModNode>());

            // parse nodes
            OptimizedModNode[] nodes = rawNodes
                .Select(pair => new OptimizedModNode(xmlNode: pair.Value, path: pair.Key))
                .ToArray();
            return new(nodes);
        }
    }
}
