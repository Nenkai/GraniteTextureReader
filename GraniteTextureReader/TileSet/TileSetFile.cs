using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public class TileSetFile
{
    public const uint TILESET_FILE_MAGIC = 0x47505247;

    public uint Version { get; set; }
    public uint TileWidth { get; set; }
    public uint TileHeight { get; set; }
    public uint TileBorder { get; set; }
    public uint MaxTileSize { get; set; }
    public uint CustomPageSize { get; set; }
    public uint NumPageFiles { get; set; }

    public int[] Layers { get; set; }

    public GDEXItem Metadata { get; set; } = new GDEXItem();
    public List<FlatTileInfo> FlatTileInfos { get; set; } = [];
    public List<PageFileInfo> PageFileInfos { get; set; } = [];
    public List<LevelInfo> LevelInfo { get; set; } = [];
    public Dictionary<uint, ParameterBlockInfo> ParameterBlockInfos { get; set; } = [];

    // Graphine::Granite::Internal::TiledFile::Initialize
    public void Initialize(Stream stream)
    {
        using var bs = new BinaryStream(stream);
        uint magic = bs.ReadUInt32();
        if (magic != TILESET_FILE_MAGIC)
            throw new IOException("Invalid magic.");

        Version = bs.ReadUInt32();
        if (Version != 6)
            throw new NotSupportedException("Only version 6 is supported.");

        if (Version >= 5)
            InitializeV5orV6(bs); // 
    }

    // Graphine::Granite::Internal::TiledFile::InitializeV5orV6
    private void InitializeV5orV6(BinaryStream bs)
    {
        bs.ReadUInt32();
        byte[] guid = bs.ReadBytes(16);
        uint numLayers = bs.ReadUInt32();
        ulong layersOffset = bs.ReadUInt64();
        uint numLevels = bs.ReadUInt32();
        ulong levelsOffset = bs.ReadUInt64();
        TileWidth = bs.ReadUInt32();
        TileHeight = bs.ReadUInt32();
        TileBorder = bs.ReadUInt32();
        MaxTileSize = bs.ReadUInt32();
        uint numFlatTiles = bs.ReadUInt32();
        ulong flatTilesOffset = bs.ReadUInt64();
        bs.ReadUInt32();
        bs.ReadUInt32();
        uint numReverseTiles = bs.ReadUInt32();
        ulong reverseTilesOffset = bs.ReadUInt64();
        bs.Position += 7 * sizeof(uint);
        CustomPageSize = bs.ReadUInt32();
        NumPageFiles = bs.ReadUInt32();
        ulong pagesFilesOffset = bs.ReadUInt64();
        uint metaSize = bs.ReadUInt32();
        ulong metaOffset = bs.ReadUInt64();
        uint numParameterBlock = bs.ReadUInt32();
        ulong parameterBlocksOffset = bs.ReadUInt64();

        bs.Position = (long)metaOffset;
        Metadata.Read(bs);

        ReadLayers(bs, numLayers, layersOffset);
        ReadLevels(bs, numLevels, levelsOffset);
        ReadFlatTileInfo(bs, numFlatTiles, flatTilesOffset);
        ReadPageInfos(bs, pagesFilesOffset);
        ReadParameterBlock(bs, numParameterBlock, parameterBlocksOffset);
    }

    public List<GDEXItem> GetTextures()
    {
        GDEXItem atlas = Metadata[GDEXTags.Atlas];
        GDEXItem textures = atlas[GDEXTags.TextureSet];

        return textures.GetObjectList();
    }

    private void ReadLayers(BinaryStream bs, uint count, ulong offset)
    {
        Layers = new int[count];
        for (int i = 0; i < count; i++)
        {
            Layers[i] = bs.ReadInt32();
            bs.ReadInt32();
        }
    }

    private void ReadLevels(BinaryStream bs, uint count, ulong offset)
    {
        for (int i = 0; i < count; i++)
        {
            bs.Position = (long)offset + (i * 0x10);
            var levelInfo = new LevelInfo();
            levelInfo.Read(bs, Layers.Length);
            LevelInfo.Add(levelInfo);
        }
    }
    private void ReadPageInfos(BinaryStream bs, ulong offset)
    {
        uint pageCounter = 0;

        for (int i = 0; i < NumPageFiles; i++)
        {
            bs.Position = (long)(offset + (ulong)(i * PageFileInfo.GetSize(Version)));

            var pageFile = new PageFileInfo();
            pageFile.Read(bs, Version);
            PageFileInfos.Add(pageFile);

            pageFile.PageIndexStart = pageCounter;
            pageCounter += pageFile.NumPages;
        }
    }

    private void ReadFlatTileInfo(BinaryStream bs, uint count, ulong offset)
    {
        uint counter = 0;

        for (int i = 0; i < count; i++)
        {
            bs.Position = (long)(offset + (ulong)(i * FlatTileInfo.GetSize()));

            var flatTileInfo = new FlatTileInfo();
            flatTileInfo.Read(bs);
            FlatTileInfos.Add(flatTileInfo);

            if (flatTileInfo.pageIndex > counter)
                counter = flatTileInfo.pageIndex;
        }
    }

    private void ReadParameterBlock(BinaryStream bs, uint count, ulong offset)
    {
        for (int i = 0; i < count; i++)
        {
            bs.Position = (long)(offset + (ulong)(i * ParameterBlockInfo.GetSize(Version)));

            var parameterBlockInfo = new ParameterBlockInfo();
            parameterBlockInfo.Read(bs, Version);
            ParameterBlockInfos.Add(parameterBlockInfo.ID, parameterBlockInfo);
        }
    }
}
