//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/GraniteTextureReader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using Syroot.BinaryData;

namespace GraniteLib.TileSet;

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

    public void Write(BinaryStream bs)
    {
        bs.WriteUInt16(PageFileIndex);
        bs.WriteUInt16(pageIndex);
        bs.WriteUInt16(TileIndex);
        bs.WriteUInt16(NumTiles);
        bs.WriteUInt32(TileListOffset);
    }

    public static uint GetSize()
    {
        return 0x0C;
    }
}
