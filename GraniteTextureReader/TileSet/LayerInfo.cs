using Syroot.BinaryData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.TileSet;

public class LayerInfo
{
    public uint Unk { get; set; }
    public uint DefaultRGBAMaybe { get; set; }


    public void Read(BinaryStream bs)
    {
        Unk = bs.ReadUInt32();
        DefaultRGBAMaybe = bs.ReadUInt32();
    }

    public static uint GetSize(uint version)
    {
        if (version != 6)
            throw new NotSupportedException($"Version {version} not supported");

        return 0x08;
    }
}
