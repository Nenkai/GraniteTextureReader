using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using GraniteTextureReader.GDEX;
using Syroot.BinaryData;

namespace GraniteTextureReader.TileSet;

public class TileSetFile
{
    public const uint TILESET_FILE_MAGIC = 0x47505247;

    public uint Version { get; set; }
    public byte[] Guid { get; set; }
    public uint TileWidth { get; set; }
    public uint TileHeight { get; set; }
    public uint TileBorder { get; set; }
    public uint MaxTileSize { get; set; }
    public uint CustomPageSize { get; set; }
    public uint NumPageFiles { get; set; }

    public GDEXItem Metadata { get; set; } = new GDEXItem();
    public List<FlatTileInfo> FlatTileInfos { get; set; } = [];
    public List<PageFileInfo> PageFileInfos { get; set; } = [];
    public List<LayerInfo> LayerInfos { get; set; } = [];
    public List<LevelInfo> LevelInfos { get; set; } = [];
    public Dictionary<uint, ParameterBlockInfo> ParameterBlockInfos { get; set; } = [];
    public PackedTileInfo[] PackedTileInfos { get; set; }

    // Graphine::Granite::Internal::TiledFile::Initialize
    public void Initialize(string file)
    {
        using var fs = File.OpenRead(file);
        Initialize(fs);
    }

    public void Initialize(Stream stream)
    {
        using var bs = new BinaryStream(stream);
        uint magic = bs.ReadUInt32();
        if (magic != TILESET_FILE_MAGIC)
            throw new IOException("Invalid magic.");

        Version = bs.ReadUInt32();
        if (Version != 5 && Version != 6)
            throw new NotSupportedException("Only version 6 is supported.");

        if (Version >= 5)
            InitializeV5orV6(bs);
    }

    // Graphine::Granite::Internal::TiledFile::InitializeV5orV6
    private void InitializeV5orV6(BinaryStream bs)
    {
        bs.ReadUInt32();
        Guid = bs.ReadBytes(16); // Not a checksum
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
        ReadPackedTileInfos(bs, numReverseTiles, reverseTilesOffset);
    }

    public string GetProjectFile()
    {
        var proj = Metadata[GDEXTags.Project];
        if (proj is null)
            return string.Empty;

        string projFile = proj.GetString();
        return projFile;
    }

    public Project GetProject()
    {
        string projFile = GetProjectFile();
        if (!string.IsNullOrEmpty(projFile))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Project));
            Project project;
            using (TextReader reader = new StringReader(projFile))
                return (Project)serializer.Deserialize(reader);
        }

        return null;
    }

    public List<GDEXItem> GetTextures()
    {
        GDEXItem atlas = Metadata[GDEXTags.Atlas];
        GDEXItem textures = atlas[GDEXTags.TextureSet];

        return textures.GetObjectList();
    }

    public void Write(Stream stream, uint version)
    {
        var bs = new BinaryStream(stream);
        bs.Position = 0xC0;

        WriteLayers(bs, version);
        WriteLevelAndTileInfos(bs, version);
        WriteParameterBlocksAndInfos(bs, version);
        WritePageFileInfos(bs, version);
        WriteMetadata(bs, version);
        // TODO thumbnail data
        WritePackedTileList(bs, version);
        WriteFlatTileInfos(bs, version);

        // Write header
        bs.Position = 0;
        bs.WriteString("GRPG", StringCoding.Raw);
        bs.WriteUInt32(version);
        bs.WriteUInt32(0);
        bs.WriteBytes(Guid);
        bs.Position = 0x34;
        bs.WriteUInt32(TileWidth);
        bs.WriteUInt32(TileHeight);
        bs.WriteUInt32(TileBorder);
        bs.WriteUInt32(MaxTileSize);

        bs.Position = 0x80;
        bs.WriteUInt32(CustomPageSize);
    }


    #region Reading
    private void ReadLayers(BinaryStream bs, uint count, ulong offset)
    {
        for (int i = 0; i < count; i++)
        {
            bs.Position = (long)offset + (i * LayerInfo.GetSize(Version));
            var layer = new LayerInfo();
            layer.Read(bs);
            LayerInfos.Add(layer);
        }
    }

    private void ReadLevels(BinaryStream bs, uint count, ulong offset)
    {
        for (int i = 0; i < count; i++)
        {
            bs.Position = (long)offset + (i * LevelInfo.GetSize(Version));
            var levelInfo = new LevelInfo();
            levelInfo.Read(bs, LayerInfos.Count);
            LevelInfos.Add(levelInfo);
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

    private void ReadPackedTileInfos(BinaryStream bs, uint count, ulong offset)
    {
        PackedTileInfos = new PackedTileInfo[count];
        bs.Position = (long)offset;
        for (int i = 0; i < count; i++)
        {
            PackedTileInfos[i] = new PackedTileInfo(bs.ReadUInt32());
        }
    }
    #endregion

    private void WriteLayers(BinaryStream bs, uint version)
    {
        long layersOffset = bs.Position;
        for (int i = 0; i < LayerInfos.Count; i++)
            LayerInfos[i].Write(bs);

        long lastPos = bs.Position;

        bs.Position = 0x1C;
        bs.WriteUInt32((uint)LayerInfos.Count);
        bs.WriteInt64(layersOffset);

        bs.Position = lastPos;
    }

    private void WriteLevelAndTileInfos(BinaryStream bs, uint version)
    {

        List<long> tileInfosOffset = new List<long>(LevelInfos.Count);
        for (int i = 0; i < LevelInfos.Count; i++)
        {
            tileInfosOffset.Add(bs.Position);

            LevelInfo levelInfo = LevelInfos[i];
            for (int j = 0 ; j < levelInfo.TileInfos.Length; j++)
                bs.WriteInt32(levelInfo.TileInfos[j].FlatTileIndex);
        }

        long levelsOffset = bs.Position;
        long lastPos = bs.Position;
        for (int i = 0; i < LevelInfos.Count; i++)
        {
            LevelInfo levelInfo = LevelInfos[i];
            bs.WriteUInt32(levelInfo.NumTilesX);
            bs.WriteUInt32(levelInfo.NumTilesY);
            bs.WriteUInt32((uint)tileInfosOffset[i]);
            bs.WriteUInt32(0); // Pad

            lastPos = bs.Position;
        }

        bs.Position = 0x28;
        bs.WriteUInt32((uint)LevelInfos.Count);
        bs.WriteInt64(levelsOffset);

        bs.Position = lastPos;
    }

    private void WriteParameterBlocksAndInfos(BinaryStream bs, uint version)
    {
        long parameterBlockInfosOffset = bs.Position;

        long infoOffset = bs.Position;
        long lastBlockOffset = bs.Position + (ParameterBlockInfos.Count * ParameterBlockInfo.GetSize(version));
        int i = 0;
        foreach (ParameterBlockInfo paramBlock in ParameterBlockInfos.Values)
        {
            bs.Position = infoOffset + (i * ParameterBlockInfo.GetSize(version));
            bs.WriteUInt32(paramBlock.ID);
            bs.WriteUInt32(paramBlock.Codec);
            bs.WriteUInt32((uint)paramBlock.ParameterBlock.Length);
            bs.WriteUInt32((uint)lastBlockOffset);
            bs.WriteUInt32(0); // Pad

            bs.Position = lastBlockOffset;
            bs.WriteBytes(paramBlock.ParameterBlock);
            lastBlockOffset = bs.Position;
            i++;
        }

        bs.Position = 0x9C;
        bs.WriteUInt32((uint)ParameterBlockInfos.Count);
        bs.WriteInt64(parameterBlockInfosOffset);

        bs.Position = lastBlockOffset;
    }

    private void WritePageFileInfos(BinaryStream bs, uint version)
    {
        long pageFileInfosOffset = bs.Position;
        for (int i = 0; i < PageFileInfos.Count; i++)
        {
            PageFileInfos[i].Write(bs, version);
        }

        long lastPos = bs.Position;

        bs.Position = 0x84;
        bs.WriteUInt32((uint)PageFileInfos.Count);
        bs.WriteInt64(pageFileInfosOffset);

        bs.Position = lastPos;
    }

    private void WriteMetadata(BinaryStream bs, uint version)
    {
        long metadataOffset = bs.Position;
        Metadata.Write(bs);
        long endPos = bs.Position;
        long metadataSize = endPos - metadataOffset;

        bs.Position = 0x90;
        bs.WriteUInt32((uint)metadataSize);
        bs.WriteInt64(metadataOffset);

        bs.Position = endPos;
    }

    private void WritePackedTileList(BinaryStream bs, uint version)
    {
        long packedTileList = bs.Position;
        for (int i = 0; i < PackedTileInfos.Length; i++)
            bs.WriteUInt32(PackedTileInfos[i].PackedValue);
        long endPos = bs.Position;

        bs.Position = 0x58;
        bs.WriteUInt32((uint)PackedTileInfos.Length);
        bs.WriteInt64(packedTileList);

        bs.Position = endPos;
    }

    private void WriteFlatTileInfos(BinaryStream bs, uint version)
    {
        long flatTileInfosOffset = bs.Position;
        for (int i = 0; i < FlatTileInfos.Count; i++)
            FlatTileInfos[i].Write(bs);
        long endPos = bs.Position;

        bs.Position = 0x44;
        bs.WriteUInt32((uint)FlatTileInfos.Count);
        bs.WriteInt64(flatTileInfosOffset);

        bs.Position = endPos;
    }

}
