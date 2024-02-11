
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;

using Syroot.BinaryData;
using Syroot.BinaryData.Memory;

using DotFastLZ.Compression;

using GraniteTextureReader.TileSet;
using SixLabors.ImageSharp.PixelFormats;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;

namespace GraniteTextureReader.Pages;

public class PageFile : IDisposable
{
    public const uint PAGE_FILE_MAGIC = 0x50415247;

    public uint Version { get; set; }

    private BinaryStream _stream;

    private TileSetFile _tileSet;

    public PageFile(TileSetFile tileSetFile)
    {
        _tileSet = tileSetFile;
    }

    public void Initialize(string file)
    {
        var fs = new FileStream(file, FileMode.Open);
        _stream = new BinaryStream(fs);
        ReadHeader();
    }

    private void ReadHeader()
    {
        uint magic = _stream.ReadUInt32();
        if (magic != PAGE_FILE_MAGIC)
            throw new IOException("Invalid magic.");

        Version = _stream.ReadUInt32();
        if (Version != 4)
            throw new NotSupportedException("Only version 4 is supported.");
    }

    public ColorRgba32[] TranscodeTile(int pageIndex, int tileIndex)
    {
        long startPageOffset = pageIndex * _tileSet.CustomPageSize;
        long pageOffset = pageIndex == 0 ? 0x18 : startPageOffset;

        _stream.Position = pageOffset + (tileIndex * sizeof(uint)) + 4;
        uint tileOffet = (uint)startPageOffset + _stream.ReadUInt32();
        _stream.Position = tileOffet;

        // Tile header
        uint codec = _stream.ReadUInt32();
        uint parameterId = _stream.ReadUInt32();
        uint size = _stream.ReadUInt32();

        if (!_tileSet.ParameterBlockInfos.TryGetValue(parameterId, out ParameterBlockInfo info))
            throw new InvalidDataException($"Parameter block info missing? Page: {pageIndex}, Tile: {tileIndex}");

        byte[] compressed = _stream.ReadBytes((int)size);
        byte[] output = ArrayPool<byte>.Shared.Rent((int)_tileSet.MaxTileSize);

        ColorRgba32[] colorBuffer = new ColorRgba32[_tileSet.TileWidth * _tileSet.TileHeight];
        long read = 0;
        if (codec == 0)
        {
            if (size == 0x02)
                colorBuffer[0] = new ColorRgba32(compressed[0], compressed[1], 0, 0xFF);
            else if (size == 0x04)
                colorBuffer[0] = new ColorRgba32(compressed[0], compressed[1], compressed[2], compressed[3]);
            else
                throw new NotSupportedException();
        }
        else
        {
            if (codec == 4)
            {
                // "QuartzFast?" aka "raw"/"lz4"/"lz40.1.0"?
                throw new NotSupportedException("Quartzfast codec not supported");
            }
            else if (codec == 9)
            {
                read = FastLZ.Decompress(compressed, size, output, output.Length);
            }
            else
                throw new NotSupportedException($"Codec {codec} not supported");

            SpanReader sr = new SpanReader(info.ParameterBlock);
            sr.Position = 44;
            string textureType = sr.ReadString0();

            CompressionFormat format = textureType switch
            {
                "BC7 " => CompressionFormat.Bc7,
                // bc6?
                "BC5 " => CompressionFormat.Bc5,
                "BC4 " => CompressionFormat.Bc4,
                "BC3 " => CompressionFormat.Bc3,
                "BC1 " => CompressionFormat.Bc1,
            };

            var colors = new BcDecoder();

            // We always decode a full tile with borders
            colorBuffer = colors.DecodeRaw(output, (int)_tileSet.TileWidth, (int)_tileSet.TileHeight, format);

            ArrayPool<byte>.Shared.Return(output);
        }

        return colorBuffer;
    }

    public void Dispose()
    {
        _stream?.Dispose();
    }
}
