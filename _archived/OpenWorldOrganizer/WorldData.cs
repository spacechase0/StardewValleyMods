using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace OpenWorldOrganizer
{
    public class WorldData
    {
        public string Id { get; set; }

        // todo - map menu data? or hijack 1.6 map stuff?

        public bool Outdoors { get; set; }
        public bool Seamless { get; set; }

        public Vector2 WorldSize { get; set; }
        // section size is always 100x100
        public class SectionData
        {
            public string MapPath { get; set; }
        }
    }
}
