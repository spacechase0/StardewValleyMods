using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatCaveOffensive.Data;

public class TileReference
{
    public string Tilesheet;
    public int TileIndex;

    public TileReference() { }
    public TileReference(int tileIndex)
    {
        TileIndex = tileIndex;
    }
}
