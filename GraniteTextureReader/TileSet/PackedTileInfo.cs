using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.TileSet;

public struct PackedTileInfo
{
    public PackedTileInfo(uint value)
    {
        PackedValue = value;
    }

    public uint PackedValue { get; set; }

    public readonly byte Layer => (byte)(PackedValue & 0b1111);
    public readonly byte Mipmap => (byte)((PackedValue >> 4) & 0b1111);
    public readonly ushort TileX => (byte)((PackedValue >> 20) & 0b111111111111);
    public readonly ushort TileY => (byte)((PackedValue >> 12) & 0b111111111111);
}
