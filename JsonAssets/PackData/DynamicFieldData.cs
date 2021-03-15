using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonAssets.PackData
{
    public class DynamicFieldData
    {
        public string[] Conditions;

        public string Field;

        public object Data;

        public bool Check()
        {
            if ( Mod.instance.epu.CheckConditions( Conditions ) )
                return true;
            return false;
        }

        public void Apply( object obj )
        {
            string[] fields = Field.Split( '.' );

            // Find the place to apply it to.
            object lastObj = null;
            PropertyInfo lastProp = null;
            object lastInd = null;
            int fCount = 0;
            foreach ( var field_ in fields )
            {
                string field = field_;

                // Prepare index value
                object ind = null;
                if ( field.Contains( '[' ) )
                {
                    int indStart = field.IndexOf( '[' ) + 1;
                    int indEnd = field.IndexOf( ']' );
                    string indStr = field.Substring( indStart, indEnd - indStart );
                    if ( int.TryParse( indStr, out int result ) )
                        ind = result; // For arrays
                    else
                        ind = indStr; // For dictionaries

                    field = field.Substring( 0, indStart - 1 );
                }

                // Get the property the field refers to
                var prop = obj.GetType().GetProperty( field );
                if ( prop == null )
                    throw new ArgumentException( $"No such property '{field}' on {obj}" );

                // Direct indices to next field
                lastObj = obj;
                obj = prop.GetValue( obj );
                if ( ind is int indI && obj is Array arr )
                {
                    if ( arr.Length <= indI )
                        throw new ArgumentException( $"No such index '{indI}' in array '{field}'" );
                    obj = arr.Cast<object>().ElementAt( indI );
                }
                if ( ind is int indI2 && obj is System.Collections.IList list )
                {
                    if ( list.Count <= indI2 )
                        throw new ArgumentException( $"No such index '{indI2}' in array '{field}'" );
                    obj = list[ indI2 ];
                }
                if ( fCount < fields.Length - 1 )
                {
                    if ( ind != null && obj is System.Collections.IDictionary dict )
                    {
                        if ( !dict.Contains( ind ) )
                            throw new ArgumentException( $"No such key '{ind}' in dictionary '{field}'" );
                        obj = dict[ ind ];
                    }
                }

                lastProp = prop;
                lastInd = ind;
                ++fCount;
            }

            // Apply it
            if ( lastInd == null )
            {
                if ( lastProp.PropertyType == typeof( int ) )
                    lastProp.SetValue( lastObj, ( int ) ( long ) Data );
                else if ( lastProp.PropertyType == typeof( bool ) )
                    lastProp.SetValue( lastObj, ( bool ) Data );
                else if ( lastProp.PropertyType == typeof( float ) )
                    lastProp.SetValue( lastObj, ( float ) ( double ) Data );
                else if ( lastProp.PropertyType == typeof( string ) )
                    lastProp.SetValue( lastObj, ( string ) Data );
                else if ( Nullable.GetUnderlyingType( lastProp.PropertyType ) != null )
                {
                    if ( lastProp.PropertyType == typeof( int? ) )
                        lastProp.SetValue( lastObj, ( int ) ( long ) Data );
                    else if ( lastProp.PropertyType == typeof( float? ) )
                        lastProp.SetValue( lastObj, ( float ) ( double ) Data );
                    else
                        lastProp.SetValue( lastObj, Data );
                }
                else if ( !lastProp.PropertyType.IsPrimitive && Data == null )
                    lastProp.SetValue( lastObj, null );
                else
                    throw new ArgumentException( $"Unsupported type {lastProp.PropertyType} {Data.GetType()}" );
                // TODO: JObject
            }
            else
            {
                // TODO
                throw new NotImplementedException( $"Not implemented: Setting index {lastProp} {lastInd} {lastObj} {obj} {Data} {Data.GetType()}" );
            }
        }
    }
}
