using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SpaceShared.Attributes;
using Stardew3D.DataModels;

namespace Stardew3D.Content.Models;

[DictionaryAssetData<ModelData>("Models", "($/Location)&", OwnedAsset = true)]
internal partial class Locations : SpaceShared.Content.BaseDictionaryAssetData
{
    public LocationModelData Farm => new()
    {
        Portals = new()
        {
            { "Backwoods", new()
            {
                OtherLocation = "Backwoods",
                MatchingPortal = "Farm",
                Position = new( 41, 0, 0 ),
                Facing = Vector3.Backward,
            } },
            { "BusStop", new()
            {
                OtherLocation = "BusStop",
                MatchingPortal = "Farm",
                Position = new( 80, 0, 17 ),
                Facing = Vector3.Left,
            } }
        },
    };

    public LocationModelData BusStop => new()
    {
        Portals = new()
        {
            { "Farm", new()
            {
                OtherLocation = "Farm",
                MatchingPortal = "BusStop",
                Position = new( 0, 0, 24 ),
                Facing = Vector3.Right,
            } }
        },
    };

    public LocationModelData Backwoods => new()
    {
        Portals = new()
        {
            { "Farm", new()
            {
                OtherLocation = "Farm",
                MatchingPortal = "Backwoods",
                Position = new( 14.5f, 0, 40 ),
                Facing = Vector3.Forward,
            } }
        },
    };
}
