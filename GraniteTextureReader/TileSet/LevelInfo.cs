//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/GraniteTextureReader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using Syroot.BinaryData;

namespace GraniteLib.TileSet;

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


    public static uint GetSize(uint version)
    {
        if (version < 4 || version > 6)
            throw new NotSupportedException($"Version {version} not supported");

        return 0x10;
    }
}
