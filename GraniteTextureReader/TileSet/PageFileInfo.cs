using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public class PageFileInfo
{
    public uint PageIndexStart { get; set; }

    public string FileName { get; set; }
    public uint NumPages;
    public byte[] Checksum { get; set; }
    public uint Type { get; set; }
    public uint SizeInBytes { get; set; }

    public void Read(BinaryStream bs, uint version)
    {
        byte[] fileName = bs.ReadBytes(0x200);
        FileName = Encoding.Unicode.GetString(fileName).TrimEnd('\0');
        NumPages = bs.ReadUInt32();
        Checksum = bs.ReadBytes(0x10);
        Type = bs.ReadUInt32();
        SizeInBytes = bs.ReadUInt32();
        bs.ReadUInt32();
    }

    public static uint GetSize(uint version)
    {
        if (version == 6)
            return 0x220;

        throw new NotSupportedException();
    }
}
