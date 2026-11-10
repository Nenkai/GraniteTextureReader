//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/GraniteTextureReader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

using Syroot.BinaryData;

namespace GraniteLib.TileSet;

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
