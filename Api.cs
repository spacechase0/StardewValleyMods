using GenericModConfigMenu.ModOption;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericModConfigMenu
{
    public interface IApi
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);
        
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func< bool > optionGet, Action< bool > optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func< int > optionGet, Action< int > optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func< float > optionGet, Action< float > optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func< string > optionGet, Action< string > optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func< SButton > optionGet, Action< SButton > optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func< int > optionGet, Action< int > optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func< float > optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func< string > optionGet, Action< string > optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func< Vector2, object, object > widgetUpdate,
                                   Func< SpriteBatch, Vector2, object, object > widgetDraw,
                                   Action< object > onSave);
    }

    public class Api : IApi
    {
        public void RegisterModConfig( IManifest mod, Action revertToDefault, Action saveToFile )
        {
            if ( Mod.instance.configs.ContainsKey( mod ) )
                throw new ArgumentException( "Mod already registered" );
            Mod.instance.configs.Add( mod, new ModConfig( mod, revertToDefault, saveToFile ) );
        }

        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet) => RegisterSimpleOption<bool>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet) => RegisterSimpleOption<int>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet) => RegisterSimpleOption<float>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet) => RegisterSimpleOption<string>(mod, optionName, optionDesc, optionGet, optionSet);
        public void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet) => RegisterSimpleOption<SButton>(mod, optionName, optionDesc, optionGet, optionSet);

        public void RegisterSimpleOption< T >( IManifest mod, string optionName, string optionDesc, Func< T > optionGet, Action< T > optionSet )
        {
            if ( !Mod.instance.configs.ContainsKey( mod ) )
                throw new ArgumentException( "Mod not registered" );

            Type[] valid = new Type[] { typeof( bool ), typeof( int ), typeof( float ), typeof( string ), typeof( SButton ) };
            if ( !valid.Contains( typeof( T ) ) )
            {
                throw new ArgumentException( "Invalid config option type." );
            }
            Mod.instance.configs[ mod ].Options.Add( new SimpleModOption< T >( optionName, optionDesc, typeof( T ), optionGet, optionSet ) );
        }

        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max) => RegisterClampedOption<int>(mod, optionName, optionDesc, optionGet, optionSet, min, max);
        public void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max) => RegisterClampedOption<float>(mod, optionName, optionDesc, optionGet, optionSet, min, max);

        public void RegisterClampedOption< T >( IManifest mod, string optionName, string optionDesc, Func< T > optionGet, Action< T > optionSet, T min, T max )
        {
            if ( !Mod.instance.configs.ContainsKey( mod ) )
                throw new ArgumentException( "Mod not registered" );

            Type[] valid = new Type[] { typeof( int ), typeof( float ) };
            if ( !valid.Contains( typeof( T ) ) )
            {
                throw new ArgumentException( "Invalid config option type." );
            }
            Mod.instance.configs[ mod ].Options.Add( new ClampedModOption< T >( optionName, optionDesc, typeof( T ), optionGet, optionSet, min, max ) );
        }

        public void RegisterChoiceOption( IManifest mod, string optionName, string optionDesc, Func< string > optionGet, Action< string > optionSet, string[] choices )
        {
            if ( !Mod.instance.configs.ContainsKey( mod ) )
                throw new ArgumentException( "Mod not registered" );

            Mod.instance.configs[ mod ].Options.Add( new ChoiceModOption<string>( optionName, optionDesc, typeof( string ), optionGet, optionSet, choices ) );
        }
        
        public void RegisterComplexOption( IManifest mod, string optionName, string optionDesc,
                                           Func< Vector2, object, object> widgetUpdate,
                                           Func< SpriteBatch, Vector2, object, object > widgetDraw,
                                           Action< object > onSave )
            => RegisterComplexOption<object>(mod, optionName, optionDesc, widgetUpdate, widgetDraw, onSave);

        public void RegisterComplexOption< T >( IManifest mod, string optionName, string optionDesc,
                                                Func< Vector2, T, T > widgetUpdate,
                                                Func< SpriteBatch, Vector2, T, T > widgetDraw,
                                                Action< T > onSave )
        {
            if ( !Mod.instance.configs.ContainsKey( mod ) )
                throw new ArgumentException( "Mod not registered" );

            Func<Vector2, object, object> update = ( Vector2 v2, object o ) => widgetUpdate.Invoke(v2, (T) o);
            Func<SpriteBatch, Vector2, object, object> draw = (SpriteBatch b, Vector2 v2, object o) => widgetDraw.Invoke(b, v2, (T)o);
            Action<object> save = (object o) => onSave.Invoke((T)o);

            Mod.instance.configs[ mod ].Options.Add( new ComplexModOption( optionName, optionDesc, update, draw, save ) );
        }
    }
}
