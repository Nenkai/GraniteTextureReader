using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.GDEX;

public class GDEXItem
{
    public uint Tag { get; set; }
    public GDEXItemType Type { get; set; }
    public GDEXItemFlags Flags { get; set; }

    private object _value { get; set; }

    public GDEXItem this[string tagStr]
    {
        get
        {
            foreach (var val in (List<GDEXItem>)_value)
            {
                if (val.Tag == StringTagToTag(tagStr))
                    return val;
            }

            return null;
        }
    }

    public GDEXItem this[uint tag]
    {
        get
        {
            foreach (var val in (List<GDEXItem>)_value)
            {
                if (val.Tag == tag)
                    return val;
            }

            return null;
        }
    }

    public static uint StringTagToTag(string c)
    {
        return c.Length switch
        {
            1 => 0x20202000u | c[0],
            2 => 0x20200000 | ((uint)c[1] << 8) | c[0],
            3 => 0x20000000 | ((uint)c[2] << 16) | ((uint)c[1] << 8) | c[0],
            4 => ((uint)c[3] << 24) | ((uint)c[2] << 16) | ((uint)c[1] << 8) | c[0],
            _ => throw new InvalidDataException("Tag must 4 characters or less, and non empty"),
        };
    }

    public void Read(BinaryStream bs)
    {
        Tag = bs.ReadUInt32();
        Type = (GDEXItemType)bs.Read1Byte();
        Flags = (GDEXItemFlags)bs.Read1Byte();

        ulong itemSize;
        if (Flags.HasFlag(GDEXItemFlags.ExtendedHeader))
            itemSize = bs.ReadUInt16() | bs.ReadUInt32() << 16;
        else
            itemSize = bs.ReadUInt16();

        long basePos = bs.Position;
        switch (Type)
        {
            case GDEXItemType.Struct:
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
            case GDEXItemType.Int32:
                _value = bs.ReadInt32();
                break;
            case GDEXItemType.IntArray:
                _value = bs.ReadInt32s((int)(itemSize / 4));
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
                throw new NotImplementedException($"Unsupported GDEX type {Type}");
        }

        bs.Position = basePos + (long)itemSize;
        bs.Align(0x04, grow: true);
    }

    public void Write(BinaryStream bs)
    {
        bs.WriteUInt32(Tag);
        bs.WriteByte((byte)Type);
        bs.WriteByte((byte)Flags);

        long offsetToDataSize = bs.Position;
        if (Flags.HasFlag(GDEXItemFlags.ExtendedHeader))
            bs.Position += 0x06;
        else
            bs.Position += 0x02;

        long baseDataPos = bs.Position;
        switch (Type)
        {
            case GDEXItemType.Struct:
                {
                    var items = _value as List<GDEXItem>;
                    foreach (var item in items)
                        item.Write(bs);
                }
                break;
            case GDEXItemType.String:
                bs.WriteString(_value as string, StringCoding.ZeroTerminated, Encoding.Unicode);
                break;
            case GDEXItemType.Int32:
                bs.WriteInt32((int)_value);
                break;
            case GDEXItemType.IntArray:
                {
                    var items = _value as int[];
                    foreach (var item in items)
                        bs.WriteInt32(item);
                }
                break;
            case GDEXItemType.GUIDArray:
                {
                    var items = _value as List<Guid>;
                    foreach (var item in items)
                        bs.WriteBytes(item.ToByteArray());
                }
                break;
            default:
                throw new NotSupportedException();
        }
        long endDataPos = bs.Position;

        bs.Position = offsetToDataSize;
        long itemSize = endDataPos - baseDataPos;
        if (Flags.HasFlag(GDEXItemFlags.ExtendedHeader))
        {
            bs.WriteUInt32((uint)(itemSize & 0xFFFFFFFF));
            bs.WriteUInt16((ushort)(itemSize >> 32));
        }
        else
            bs.WriteUInt16((ushort)itemSize);

        bs.Position = endDataPos;
        bs.Align(0x04, grow: true);
    }

    public int GetInt()
    {
        if (Type != GDEXItemType.Int32)
            throw new Exception("Item is not int type.");

        return (int)_value;
    }

    public void SetInt(int value)
    {
        if (Type != GDEXItemType.Int32)
            throw new Exception("Item is not int type.");

        _value = value;
    }

    public string GetString()
    {
        if (Type != GDEXItemType.String)
            throw new Exception("Item is not string type.");

        return _value as string;
    }

    public List<GDEXItem> GetObjectList()
    {
        if (Type != GDEXItemType.Struct)
            throw new Exception("Item is not object type.");

        return _value as List<GDEXItem>;
    }
}

public enum GDEXItemType : byte
{
    Raw = 0,
    Struct = 1,
    String = 2,
    Int32 = 3,
    Int64 = 4,
    Float = 5,
    Double = 6,
    Date = 7,
    IntArray = 8,
    FloatArray = 9,
    Int64Array = 10,
    DoubleArray = 11,
    GUID = 12,
    GUIDArray = 13,
}

[Flags]
public enum GDEXItemFlags
{
    None,
    ExtendedHeader,
}
