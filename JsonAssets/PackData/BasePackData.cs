using SpaceShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public abstract class BasePackData : ICloneable
    {
        internal ContentPack parent;

        internal BasePackData original;

        /// <summary>
        /// Conditions for if an item is enabled.
        /// If not met, they will be removed from the game.
        /// This is checked at the beginning of each day.
        /// These are ExpandedPreconditionsUtility conditions.
        /// </summary>
        public Dictionary<string, string> EnableConditions { get; set; }

        internal ContentPatcher.IManagedConditions EnableConditionsObject;

        /// <summary>
        /// If the current pack data is currently enabled or not.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// At the beginning of each day, these are checked and applied to each pack data.
        /// </summary>
        public DynamicFieldData[] DynamicFields { get; set; }

        public void ApplyDynamicFields()
        {
            if ( DynamicFields == null )
                return;

            foreach ( var dynField in DynamicFields )
            {
                try
                {
                    if ( dynField.Check( this ) )
                        dynField.Apply( this );
                }
                catch ( Exception e )
                {
                    Log.error( "Error applying dynamic field: " + e );
                }
            }
        }

        public virtual object Clone()
        {
            // This should do a deep copy of the object, EXCEPT `original`
            // That needs to remain a reference to the "original" object values.
            // This is because objects are cloned from that to get the original values
            // before applying dynamic fields.

            var ret = ( BasePackData ) this.MemberwiseClone();

            // In theory should do deep clone of dynamic fields,
            // but I'm not planning on supporting dynamic field editing of dynamic fields.

            return ret;
        }
    }
}
