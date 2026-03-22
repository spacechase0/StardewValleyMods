using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Stardew3D.DataModels;

public class Portal
{
    public string OtherLocation { get; set; }
    public string MatchingPortal { get; set; }

    public Vector3 Position { get; set; }
    public Vector3 Facing { get; set; }

    public override string ToString()
    {
        return $"{OtherLocation} {MatchingPortal} {Position.X} {Position.Y} {Position.Z} {Facing.X} {Facing.Y} {Facing.Z}";
    }

    public static Dictionary<string, Portal> From(string mapProp)
    {
        Dictionary<string, Portal> ret = new();

        string[] args = ArgUtility.SplitBySpaceQuoteAware(mapProp);
        for (int i = 0; i + 6 < args.Length; i += 7)
        {
            if (!ArgUtility.TryGet(args, i + 0, out string portalName, out string err) ||
                !ArgUtility.TryGet(args, i + 1, out string otherLocName, out err) ||
                !ArgUtility.TryGet(args, i + 2, out string matchingPortalName, out err) ||
                !ArgUtility.TryGetFloat(args, i + 3, out float positionX, out err) ||
                !ArgUtility.TryGetFloat(args, i + 4, out float positionY, out err) ||
                !ArgUtility.TryGetFloat(args, i + 5, out float positionZ, out err) ||
                !ArgUtility.TryGet(args, i + 6, out string dir, out err))
                continue;

            Vector3 facing;
            if (dir.ToLowerInvariant() is not "north" and not "south" and not "west" and not "east")
            {
                if (!ArgUtility.TryGetFloat(args, i + 6, out float facingX, out err) ||
                    !ArgUtility.TryGetFloat(args, i + 7, out float facingY, out err) ||
                    !ArgUtility.TryGetFloat(args, i + 8, out float facingZ, out err))
                    continue;

                facing = new(facingX, facingY, facingZ);
                i += 2;
            }
            else
            {
                facing = dir.ToLowerInvariant() switch
                {
                    "north" => Vector3.Forward,
                    "south" => Vector3.Backward,
                    "west" => Vector3.Left,
                    "east" => Vector3.Right,
                };
            }

            ret.Add(portalName, new()
            {
                OtherLocation = otherLocName,
                MatchingPortal = matchingPortalName,
                Position = new Vector3(positionX, positionY, positionZ),
                Facing = facing,
            });
    }

        return ret;
    }
}
