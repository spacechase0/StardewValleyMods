using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceShared.APIs
{
    public interface ITrainStationApi
    {
        void OpenTrainMenu();
        void OpenBoatMenu();

        //If the given StopId already exists, update it with the given data. Otherwise, create a new train stop with the given data
        void RegisterTrainStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);

        //If the given StopId already exists, update it with the given data. Otherwise, create a new boat stop with the given data
        void RegisterBoatStation(string stopId, string targetMapName, Dictionary<string, string> localizedDisplayName, int targetX, int targetY, int cost, int facingDirectionAfterWarp, string[] conditions, string translatedName);
    }
}
