using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CombatOverhaulMod.Buffs
{
    [XmlType( "Mods_spacechase0_COM_CustomBuffInstance" )]
    public class CustomBuffInstance : INetObject< NetFields >
    {
        public NetFields NetFields => new NetFields( "CustomBuffInstance");

        private readonly NetString buffId = new();
        private readonly NetFloat duration = new();
        private readonly NetFloat durationUsed = new();

        public string BuffId => buffId.Value;
        public float Duration => duration.Value;
        public float DurationUsed => durationUsed.Value;

        public CustomBuffInstance()
        {
            NetFields.AddField(buffId, nameof(this.buffId) );
            NetFields.AddField(duration, nameof(this.duration));
            NetFields.AddField(durationUsed, nameof(this.durationUsed));
        }

        public CustomBuffInstance( string buffId, float duration )
        :   this()
        {
            this.buffId.Value = buffId;
            this.duration.Value = duration;
        }

        public CustomBuffData GetData()
        {
            var buffs = Game1.content.Load< Dictionary< string, CustomBuffData > >( "spacechase0.CombatOverhaulMod\\Buffs" );
            if ( !buffs.ContainsKey( BuffId ) )
                return null;
            return buffs[ BuffId ];
        }

        public void Apply( Character character )
        {
            var buff = GetData();
            foreach ( var effectData in buff.Effects )
            {
                var effect = EffectRegistry.Get( effectData.EffectId );
                effect.Apply( character, effectData.Modifier );
            }
        }
        public void Tick( Character character, GameTime time )
        {
            var buff = GetData();
            foreach ( var effectData in buff.Effects )
            {
                var effect = EffectRegistry.Get( effectData.EffectId );
                effect.Tick( character, ( float ) time.ElapsedGameTime.TotalSeconds, DurationUsed, Duration, effectData.Modifier );
            }
            durationUsed.Value += ( float ) time.ElapsedGameTime.TotalSeconds;
        }

        public void Unapply( Character character )
        {
            var buff = GetData();
            foreach ( var effectData in buff.Effects )
            {
                var effect = EffectRegistry.Get( effectData.EffectId );
                effect.Unapply( character, effectData.Modifier );
            }
        }
    }
}
