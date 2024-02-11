using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.TileSet;

public enum DataType
{
    R8G8B8_SRGB,
    R8G8B8A8_SRGB,
    X8Y8Z0_TANGENT,
    R8G8B8_LINEAR,
    R8G8B8A8_LINEAR,
    X8,
    X8Y8,
    X8Y8Z8,
    X8Y8Z8W8,
    X16,
    X16Y16,
    X16Y16Z16,
    X16Y16Z16W16,
    X32,
    X32_FLOAT,
    X32Y32,
    X32Y32_FLOAT,
    X32Y32Z32,
    X32Y32Z32_FLOAT,
    R32G32B32,
    R32G32B32_FLOAT,
    X32Y32Z32W32,
    X32Y32Z32W32_FLOAT,
    R32G32B32A32,
    R32G32B32A32_FLOAT,
    R16G16B16_FLOAT,
    R16G16B16A16_FLOAT,
}
