using Syroot.BinaryData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.TileSet;

public class ParameterBlockInfo
{
    public uint ID { get; set; }
    public uint Codec { get; set; }
    public uint Size { get; set; }
    public byte[] ParameterBlock { get; set; }

    public void Read(BinaryStream bs, uint version)
    {
        ID = bs.ReadUInt32();
        Codec = bs.ReadUInt32();
        Size = bs.ReadUInt32();
        uint offset = bs.ReadUInt32();
        bs.Position = offset;
        ParameterBlock = bs.ReadBytes((int)Size);
    }

    public static uint GetSize(uint version)
    {
        if (version == 6)
            return 0x14;

        throw new NotSupportedException();
    }
}
