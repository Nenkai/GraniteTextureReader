using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public class FlatTileInfo
{
    public ushort PageFileIndex;
    public ushort pageIndex;
    public ushort TileIndex;
    public ushort NumTiles;
    public uint TileListOffset;

    public void Read(BinaryStream bs)
    {
        PageFileIndex = bs.ReadUInt16();
        pageIndex = bs.ReadUInt16();
        TileIndex = bs.ReadUInt16();
        NumTiles = bs.ReadUInt16();
        TileListOffset = bs.ReadUInt32();
    }

    public static uint GetSize()
    {
        return 0x0C;
    }
}
