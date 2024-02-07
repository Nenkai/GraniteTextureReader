
using SixLabors.ImageSharp.Textures;
using SixLabors.ImageSharp.Textures.TextureFormats;
using SixLabors.ImageSharp.Textures.TextureFormats.Decoding;

using BCnEncoder.Decoder;
using BCnEncoder.Shared;

using GraniteTextureReader.TileSet;
using GraniteTextureReader.Pages;

namespace GraniteTextureReader;

internal class Program
{
    static void Main(string[] args)
    {
        var graniteProcessor = new GraniteProcessor();
        graniteProcessor.Read(args[0]);
        graniteProcessor.ExtractAll();
    }
}
