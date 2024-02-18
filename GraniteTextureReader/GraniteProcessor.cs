using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Buffers;
using System.Xml.Serialization;

using Microsoft.Toolkit.HighPerformance;

using BCnEncoder.Shared;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using GraniteTextureReader.GDEX;
using GraniteTextureReader.TileSet;
using GraniteTextureReader.Pages;

namespace GraniteTextureReader;

public class GraniteProcessor : IDisposable
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

        Project project = TileSet.GetProject();

        for (int i = 0; i < textures.Count; i++)
        {
            GDEXItem? textureItem = textures[i];

            TextureDescriptor texture = TextureDescriptor.FromGDEXItem(textureItem);
            if (!string.IsNullOrEmpty(textureName) && texture.Name != textureName) 
                continue;

            if (layer > -1)
            {
                string realName = string.Empty;
                ushort w = (ushort)texture.Width, h = (ushort)texture.Height;
                uint tileStepX = 1, tileStepY = 1;
                if (project is not null)
                {
                    ProjectAsset asset = project.ImportedAssets.FirstOrDefault(e => e.Name == texture.Name);
                    ProjectAssetLayer layerAsset = asset.Layers[layer];
                    ProjectAssetLayerTexturesTexture textureLayerAsset = layerAsset.Textures.Texture;
                    realName = Path.GetFileName(textureLayerAsset.Src);


                    // Page file width and height can differ from texture. It's odd.
                    // Files that do this also have tiles with their id value having the upper bit set
                    w = textureLayerAsset.Width;
                    h = textureLayerAsset.Height;

                    // Required when texture is smaller than declared in the page file
                    tileStepX = texture.Width / textureLayerAsset.Width;
                    tileStepY = texture.Height / textureLayerAsset.Height;
                }

                Console.WriteLine($"[{i + 1}/{textures.Count}] Processing {realName} from {texture.Name} ({texture.Width}x{texture.Height}, layer {layer})");
                string outputName = string.IsNullOrEmpty(realName) ? texture.Name + $"_{layer}.tga" : realName;
                
                ExtractTexture(Path.ChangeExtension(outputName, ".tga"), texture.X, texture.Y, w, h, tileStepX, tileStepY, 0, layer); // TODO: Preserve file type? .dds, tga, etc
            }
            else
            {
                for (int layerNum = 0; layerNum < 4; layerNum++)
                {
                    string realName = string.Empty;
                    ushort w = (ushort)texture.Width, h = (ushort)texture.Height;
                    uint tileStepX = 1, tileStepY = 1;
                    if (project is not null)
                    {
                        ProjectAsset asset = project.ImportedAssets[i];
                        if (layerNum >= asset.Layers.Length)
                            continue;

                        ProjectAssetLayer layerAsset = asset.Layers[layerNum];
                        ProjectAssetLayerTexturesTexture textureLayerAsset = layerAsset.Textures.Texture;
                        realName = Path.GetFileName(textureLayerAsset.Src);

                        w = textureLayerAsset.Width;
                        h = textureLayerAsset.Height;
                        tileStepX = texture.Width / textureLayerAsset.Width;
                        tileStepY = texture.Height / textureLayerAsset.Height;
                    }

                    string outputName = string.IsNullOrEmpty(realName) ? texture.Name + $"_{layerNum}" : realName;

                    Console.WriteLine($"[{i+1}/{textures.Count}] Processing {realName} from {texture.Name} ({texture.Width}x{texture.Height}, layer {layerNum})");
                    ExtractTexture(Path.ChangeExtension(outputName, ".tga"), texture.X, texture.Y, w, h, tileStepX, tileStepY, 0, layerNum);
                }
            }
        }
    }

    public void ExtractTexture(string outputPath, uint xOffset, uint yOffset, ushort textureWidth, ushort textureHeight, uint tileStepX, uint tileStepY, int level, int layer)
    {
        uint tileWidthNoBorder = TileSet.TileWidth - (2 * TileSet.TileBorder);
        uint tileHeightNoBorder = TileSet.TileHeight - (2 * TileSet.TileBorder);

        uint numXTiles = (textureWidth / tileWidthNoBorder);
        uint numYTiles = (textureHeight / tileHeightNoBorder);

        uint texTileOfsX = xOffset / tileWidthNoBorder;
        uint texTileOfsY = yOffset / tileHeightNoBorder;

        Rgba32[] texturePixels = ArrayPool<Rgba32>.Shared.Rent(textureWidth * textureHeight);

        // Layers explicitly can set a default image color.
        LayerInfo layerInfo = TileSet.LayerInfos[layer];
        texturePixels.AsSpan().Fill(new Rgba32(layerInfo.DefaultColor));

        for (uint currentTileY = 0; currentTileY < numYTiles; currentTileY++)
        {
            for (uint currentTileX = 0; currentTileX < numXTiles; currentTileX++)
            {
                LevelInfo levelInfo = TileSet.LevelInfos[level];

                uint tX = (currentTileX * tileStepX) + texTileOfsX;
                uint tY = (currentTileY * tileStepY) + texTileOfsY;
                TileInfo tileInfo = levelInfo.TileInfos[layer + TileSet.LayerInfos.Count * (tY * levelInfo.NumTilesX + tX)];
                if ((tileInfo.FlatTileIndex >> 31) != 0)
                {
                    // This bit means the current level has no data to be used and a lower level should be used.
                    // TODO: Actually use lower level rather than use tile stepping.
                }

                int outputX = (int)(currentTileX * tileWidthNoBorder);
                int outputY = (int)(currentTileY * tileHeightNoBorder);

                ColorRgba32[] tilePixels = GetFlatTileData(tileInfo.FlatTileIndex);
                if (layerInfo.DataType == DataType.X8Y8Z0_TANGENT) // Normal maps, b and a is ignored
                {
                    for (int i = 0; i < tilePixels.Length; i++)
                    {
                        tilePixels[i].b = (byte)((layerInfo.DefaultColor >> 16) & 0xFF);
                        tilePixels[i].a = (byte)((layerInfo.DefaultColor >> 24) & 0xFF);
                    }
                }

                Span<Rgba32> tilePixelsRgba = MemoryMarshal.Cast<ColorRgba32, Rgba32>(tilePixels);
                // Copy each row to the output, faster than doing it per-pixel
                for (int yRow = 0; yRow < tileHeightNoBorder; yRow++)
                {
                    Span<Rgba32> rowPixels = tilePixelsRgba.Slice((int)(((yRow + TileSet.TileBorder) * TileSet.TileWidth) + TileSet.TileBorder), (int)tileWidthNoBorder);
                    Span<Rgba32> outputRow = texturePixels.AsSpan((outputY * textureWidth) + (yRow * textureWidth) + outputX, (int)tileWidthNoBorder);
                    rowPixels.CopyTo(outputRow);
                }
            }
        }

        Image<Rgba32> image = Image.LoadPixelData<Rgba32>(texturePixels, textureWidth, textureHeight);
        image.SaveAsTga(_dir + "\\_extracted\\" + outputPath);
        ArrayPool<Rgba32>.Shared.Return(texturePixels);
    }

    private ColorRgba32[] GetFlatTileData(int flatTileIndex)
    {
        FlatTileInfo flatTileInfo = TileSet.FlatTileInfos[flatTileIndex & 0xFFFFFF];
        PageFileInfo pageFileInfo = TileSet.PageFileInfos[flatTileInfo.PageFileIndex];

        PackedTileInfo packedTileInfo = TileSet.PackedTileInfos[flatTileInfo.TileListOffset];

        if (!PageFiles.TryGetValue(flatTileInfo.PageFileIndex, out PageFile pageFile))
        {
            string pageFileName = Path.Combine(_dir, pageFileInfo.FileName);
            Console.WriteLine(pageFileName);

            pageFile = new PageFile(TileSet);
            pageFile.Initialize(pageFileName);
            PageFiles.Add(flatTileInfo.PageFileIndex, pageFile);
        }

        return pageFile.TranscodeTile(flatTileInfo.pageIndex, flatTileInfo.TileIndex);
    }

    public void Dispose()
    {
        foreach (var pageFile in PageFiles.Values)
        {
            pageFile?.Dispose();
        }
    }
}
