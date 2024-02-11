using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Buffers;

using GraniteTextureReader.TileSet;
using GraniteTextureReader.Pages;

using BCnEncoder.Shared;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Toolkit.HighPerformance;

namespace GraniteTextureReader;


public class GraniteProcessor
{
    public TileSetFile TileSet { get; set; }
    public Dictionary<int, PageFile> PageFiles { get; set; } = [];
    private string _dir;

    public void Read(string tileSetFile)
    {
        Configuration.Default.PreferContiguousImageBuffers = true;

        _dir = Path.GetDirectoryName(Path.GetFullPath(tileSetFile));
        // Console.WriteLine($"_dir {_dir}");
        string outputPath = _dir + "\\_extracted";
        // Console.WriteLine($"outputPath {outputPath}");
        Directory.CreateDirectory(outputPath);

        using var fs = File.OpenRead(tileSetFile);
        TileSet = new TileSetFile();
        TileSet.Initialize(fs);
    }

    public void Extract(int layer, string textureName = "")
    {
        var textures = TileSet.GetTextures();

        foreach (var textureItem in textures)
        {
            TextureDescriptor texture = TextureDescriptor.FromGDEXItem(textureItem);
            if (!String.IsNullOrEmpty(textureName) && texture.Name != textureName) continue;
            if (layer > -1)
                ExtractTexture(texture.Name + $"_{layer}.png", texture, 0, layer);
            else
                for (int layerNum = 0; layerNum < 4; layerNum++)
                {
                    ExtractTexture(texture.Name + $"_{layerNum}.png", texture, 0, layerNum);
                }
        }
    }

    public void ExtractTexture(string outputPath, TextureDescriptor texture, int level, int layer)
    {
        Console.WriteLine($"Processing {texture.Name} ({texture.Width}x{texture.Height}, mip level {level}, layer {layer})");
        uint tileWidthNoBorder = TileSet.TileWidth - (2 * TileSet.TileBorder);
        uint tileHeightNoBorder = TileSet.TileHeight - (2 * TileSet.TileBorder);

        uint numXTiles = texture.Width / tileWidthNoBorder;
        uint numYTiles = texture.Height / tileWidthNoBorder;

        uint texTileOfsX = texture.XXXX / tileWidthNoBorder;
        uint texTileOfsY = texture.YYYY / tileHeightNoBorder;

        Rgba32[] texturePixels = ArrayPool<Rgba32>.Shared.Rent(texture.Width * texture.Height);

        for (int currentTileY = 0; currentTileY < numYTiles; currentTileY++)
        {
            for (int currentTileX = 0; currentTileX < numXTiles; currentTileX++)
            {
                LevelInfo levelInfo = TileSet.LevelInfo[level];

                int tX = (int)(texTileOfsX + currentTileX);
                int tY = (int)(texTileOfsY + currentTileY);
                TileInfo tileInfo = levelInfo.TileInfos[layer + TileSet.Layers.Length * (tY * levelInfo.NumTilesX + tX)];

                int outputX = (int)(currentTileX * tileWidthNoBorder);
                int outputY = (int)(currentTileY * tileHeightNoBorder);

                ColorRgba32[] tilePixels = GetFlatTileData(tileInfo.FlatTileIndex);
                Span<Rgba32> tilePixelsRgba = MemoryMarshal.Cast<ColorRgba32, Rgba32>(tilePixels);

                // Copy each row to the output, faster than doing it per-pixel
                for (int yRow = 0; yRow < tileHeightNoBorder; yRow++)
                {
                    Span<Rgba32> rowPixels = tilePixelsRgba.Slice((int)(((yRow + TileSet.TileBorder) * TileSet.TileWidth) + TileSet.TileBorder), (int)tileWidthNoBorder);
                    Span<Rgba32> outputRow = texturePixels.AsSpan((outputY * texture.Width) + (yRow * texture.Width) + outputX, (int)tileWidthNoBorder);
                    rowPixels.CopyTo(outputRow);
                }
            }
        }

        Image<Rgba32> imagee = Image.LoadPixelData<Rgba32>(texturePixels, texture.Width, texture.Height);
        imagee.Save(_dir + "\\_extracted\\" + outputPath);

        ArrayPool<Rgba32>.Shared.Return(texturePixels);
    }

    private ColorRgba32[] GetFlatTileData(int flatTileIndex)
    {
        FlatTileInfo flatTileInfo = TileSet.FlatTileInfos[flatTileIndex & 0xFFFFFF];
        PageFileInfo pageFileInfo = TileSet.PageFileInfos[flatTileInfo.PageFileIndex];

        if (!PageFiles.TryGetValue(flatTileInfo.PageFileIndex, out PageFile pageFile))
        {
            string pageFileName = Path.Combine(_dir, pageFileInfo.FileName);

            pageFile = new PageFile(TileSet);
            pageFile.Initialize(pageFileName);
            PageFiles.Add(flatTileInfo.PageFileIndex, pageFile);
        }

        return pageFile.TranscodeTile(flatTileInfo.pageIndex, flatTileInfo.TileIndex);
    }
}
