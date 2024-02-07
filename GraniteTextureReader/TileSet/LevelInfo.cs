using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public class LevelInfo
{
    public uint NumTilesX { get; set; }
    public uint NumTilesY { get; set; }
    public TileInfo[] TileInfos { get; set; }

    public void Read(BinaryStream bs, int numLayers)
    {
        NumTilesX = bs.ReadUInt32();
        NumTilesY = bs.ReadUInt32();
        uint tilesOffset = bs.ReadUInt32();
        TileInfos = new TileInfo[numLayers * (int)NumTilesX * (int)NumTilesY];

        bs.Position = tilesOffset;
        for (int i = 0; i < TileInfos.Length; i++)
            TileInfos[i] = new TileInfo(bs.ReadInt32());
        
    }
}
