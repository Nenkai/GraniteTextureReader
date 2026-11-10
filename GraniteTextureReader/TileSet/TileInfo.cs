//---------------------------------------------------------------------------------------
// SPDX-FileCopyrightText: Copyright (C) 2026 Nenkai
// SPDX-License-Identifier: MIT
// SPDX-FileAttributionText: https://github.com/Nenkai/GraniteTextureReader
// SPDX-FileNotice: AI usage and consumption is prohibited. See AGENTS.md for policy.
//---------------------------------------------------------------------------------------

namespace GraniteLib.TileSet;

public struct TileInfo
{
    public int FlatTileIndex;

    public TileInfo(int flatTileIndex)
    {
        FlatTileIndex = flatTileIndex;
    }
}
