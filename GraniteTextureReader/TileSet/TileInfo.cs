using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public struct TileInfo
{
    public int FlatTileIndex;

    public TileInfo(int flatTileIndex)
    {
        FlatTileIndex = flatTileIndex;
    }
}
