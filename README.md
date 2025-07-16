# GraniteTextureReader

Texture Reader/Extractor for [Granite SDK](https://unity.com/products/granite-sdk) files (.gts/.gtp). This tool was primarly made with Granblue Fantasy: Relink in mind, but should also support extracting from Unity games using [Streaming Virtual Texturing](https://docs.unity3d.com/Manual/svt-streaming-virtual-texturing.html).

### Features

* Extraction of all textures within a Granite SDK tile set (.gts)
* Extracting specific textures within a tile set
* Downgrading a tile set from version 6 to 5 for use within the Granite Tile Set viewer
* Extracting project files out of tile set files (if the tile set was not built as release/unstripped)

> [!IMPORTANT]  
> Only version 6 `.gts` files are currently supported. (Version can be viewed at 0x04 in the .gts files)

> [!WARNING]
> This project is currently in stand-by and will not currently receive updates unless contributions are made (which are welcome).

## Usage

Get the latest version in [**Releases**](https://github.com/Nenkai/GraniteTextureReader/releases).

To extract a tile set in its entirety, run the command with:

```
GraniteTextureReader extract-all --tileset <path_to_gts_file> [--layer <layer>]
```

* Replace `<path_to_gts_file>` with the path to the GTS file
* **Optionally** also provide `--layer` to extract a specific tile set layer. **By default all are extracted.** Different layers are intended for different types of textures, so a simple example would be texture layer 0 being Albedo map, 1 being Normal map. Up to 3. 

More commands are available by just running the command and reading the command/verb listing.

## Documentation

File format documentation/010 Editor Template is available [here](https://github.com/Nenkai/010GameTemplates/blob/main/Graphine/Granite%20SDK/TileSet_GTS.bt).

## Building

Requires **.NET 9.0** (VS2022).
