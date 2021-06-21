using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceCore.Framework.Serialization
{
    /// <summary>An optimized view of an XML mod node stored in the <see cref="SerializerManager.FarmerFilename"/> or <see cref="SerializerManager.Filename"/> file.</summary>
    internal class OptimizedModNode
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw XML of the node to insert.</summary>
        public string XmlNode { get; }

        /// <summary>The raw 'node path' which indicates where to insert the <see cref="XmlNode"/> in the save file. This consists of slash-delimited positions within the XML tree. For example, "/1/3/5" means "insert into the 5th position under the 3rd child of the root".</summary>
        public string Path { get; }

        /// <summary>The <see cref="Path"/> of the parent element into which to insert the <see cref="XmlNode"/>.</summary>
        public string ParentPath { get; }

        /// <summary>A set of every ancestor node's <see cref="Path"/> which contains this XML node. For example, given a <see cref="Path"/> of "/1/3/5", this would contain "/1" and "/1/3".</summary>
        public ISet<string> AncestorPaths { get; }

        /// <summary>The index at which to insert the <see cref="XmlNode"/> within the <see cref="ParentPath"/>.</summary>
        public int Index { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="xmlNode"><inheritdoc cref="XmlNode" path="/summary"/></param>
        /// <param name="path"><inheritdoc cref="Path" path="/summary"/></param>
        public OptimizedModNode(string xmlNode, string path)
        {
            this.ParsePath(path, out var ancestorPaths, out string parentPath, out int index);

            this.XmlNode = xmlNode;
            this.Path = path;
            this.AncestorPaths = ancestorPaths;
            this.ParentPath = parentPath;
            this.Index = index;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Parse an arbitrary path into its component metadata.</summary>
        /// <param name="path"><inheritdoc cref="Path" path="/summary"/></param>
        /// <param name="ancestorPaths"><inheritdoc cref="AncestorPaths" path="/summary"/></param>
        /// <param name="parentPath"><inheritdoc cref="ParentPath" path="/summary"/></param>
        /// <param name="index"><inheritdoc cref="Index" path="/summary"/></param>
        private void ParsePath(string path, out ISet<string> ancestorPaths, out string parentPath, out int index)
        {
            // split path into segments (e.g. "/1/3/5" => 1, 3, 5)
            string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // get parent paths
            ancestorPaths = new HashSet<string>();
            parentPath = "";
            foreach (string segment in segments.Take(segments.Length - 1)) // ignore child path
            {
                parentPath += $"/{segment}";
                ancestorPaths.Add(parentPath);
            }

            // get index
            index = int.Parse(segments[segments.Length - 1]);
        }
    }
}
