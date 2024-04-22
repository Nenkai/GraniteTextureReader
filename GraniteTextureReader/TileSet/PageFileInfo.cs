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
        // Sometimes there's data beyond the file name, todo investigate?
        long currentPos = bs.Position;
        FileName = bs.ReadString(StringCoding.ZeroTerminated, Encoding.Unicode);
        bs.Position = currentPos + 0x200;

        NumPages = bs.ReadUInt32();
        Checksum = bs.ReadBytes(0x10);
        Type = bs.ReadUInt32();

        if (version >= 6)
        {
            SizeInBytes = bs.ReadUInt32();
            bs.ReadUInt32();
        }
    }

    public void Write(BinaryStream bs, uint version)
    {
        long offs = bs.Position;
        bs.WriteString(FileName, StringCoding.Raw, Encoding.Unicode);
        bs.Position = offs + 0x200;
        bs.WriteUInt32(NumPages);
        bs.WriteBytes(Checksum);
        bs.WriteUInt32(Type);

        if (version >= 6)
        {
            bs.WriteUInt32(SizeInBytes);
            bs.WriteUInt32(0);
        }
    }

    public static uint GetSize(uint version)
    {
        if (version == 5)
            return 0x218;
        else if (version == 6)
            return 0x220;

        throw new NotSupportedException();
    }
}
