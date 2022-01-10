using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace SpaceCore
{
    internal class CustomLocationContext
    {
        public string Name { get; set; }
        public Func< Random, LocationWeather > GetLocationWeatherForTomorrow { get; set; }
        //public Func< Farmer, string > PassoutWakeupLocation { get; set; }
        //public Func< Farmer, Point? > PassoutWakeupPoint { get; set; }
    }
}
