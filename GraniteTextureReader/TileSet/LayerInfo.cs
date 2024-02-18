using Syroot.BinaryData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.TileSet;

public class LayerInfo
{
    public DataType DataType { get; set; }

    /// <summary>
    /// RGBA
    /// </summary>
    public uint DefaultColor { get; set; }

    public void Read(BinaryStream bs)
    {
        DataType = (DataType)bs.ReadUInt32();
        DefaultColor = bs.ReadUInt32();
    }

    public void Write(BinaryStream bs)
    {
        bs.WriteUInt32((uint)DataType);
        bs.WriteUInt32(DefaultColor);
    }

    public static uint GetSize(uint version)
    {
        if (version < 4 || version > 6)
            throw new NotSupportedException($"Version {version} not supported");

        return 0x08;
    }
}
