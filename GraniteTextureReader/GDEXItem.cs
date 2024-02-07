using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Syroot.BinaryData;

namespace GraniteTextureReader;

public class GDEXItem
{
    public uint Tag { get; set; }
    public GDEXItemType Type { get; set; }
    public byte Flags { get; set; }

    private object _value { get; set; }

    public GDEXItem this[uint tag]
    {
        get 
        {
            foreach (var val in ((List<GDEXItem>)_value))
            {
                if (val.Tag == tag)
                    return val;
            }

            return null;
        }
    }

    public void Read(BinaryStream bs)
    {
        Tag = bs.ReadUInt32();
        Type = (GDEXItemType)bs.Read1Byte();
        Flags = bs.Read1Byte();

        ulong itemSize;
        if ((Flags & 0x01) == 1)
            itemSize = bs.ReadUInt16() | bs.ReadUInt32() << 16;
        else
            itemSize = bs.ReadUInt16();

        long basePos = bs.Position;
        switch (Type)
        {
            case GDEXItemType.Object:
                {
                    var list = new List<GDEXItem>();
                    while (bs.Position < basePos + (long)itemSize)
                    {
                        var item = new GDEXItem();
                        item.Read(bs);
                        list.Add(item);
                    }
                    _value = list;
                }
                break;
            case GDEXItemType.String:
                _value = bs.ReadString(StringCoding.ZeroTerminated, encoding: Encoding.Unicode);
                break;
            case GDEXItemType.Short:
                _value = bs.ReadInt16();
                break;
            case GDEXItemType.IntArray:
                _value = bs.ReadInt16s((int)(itemSize / 4));
                break;
            case GDEXItemType.GUIDArray:
                {
                    var list = new List<Guid>();
                    for (int i = 0; i < (int)(itemSize / 0x10); i++)
                        list.Add(new Guid(bs.ReadBytes(0x10)));
                    _value = list;
                }
                break;
            default:
                throw new NotImplementedException();
        }

        bs.Position = basePos + (long)itemSize;
        bs.Align(0x04, grow: true);
    }

    public short GetShort()
    {
        if (Type != GDEXItemType.Short)
            throw new Exception("Item is not short type.");

        return (short)_value;
    }

    public string GetString()
    {
        if (Type != GDEXItemType.String)
            throw new Exception("Item is not string type.");

        return _value as string;
    }

    public List<GDEXItem> GetObjectList()
    {
        if (Type != GDEXItemType.Object)
            throw new Exception("Item is not object type.");

        return _value as List<GDEXItem>;
    }
}

public enum GDEXItemType : byte
{
    Object = 1,
    String = 2,
    Short = 3,
    IntArray = 8,
    GUIDArray = 13,
}
