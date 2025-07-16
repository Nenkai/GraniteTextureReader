# GraniteTextureReader

Texture Reader/Extractor for [Granite SDK](https://web.archive.org/web/20240229035708/https://unity.com/products/granite-sdk) files (.gts/.gtp). This tool was primarly made with Granblue Fantasy: Relink in mind, but should also support extracting from Unity games using [Streaming Virtual Texturing](https://docs.unity3d.com/Manual/svt-streaming-virtual-texturing.html).

The Granite SDK has long been acquired by Unity and for the most part shelved (main page on the Unity site is gone), so updates to the format are unlikely.

It is known to be used in:
* Granblue Fantasy: Relink (proprietary, long development cycle, likely acquired Granite licensing before Unity acquired it)
* Baldur's Gate 3 (same as above)
* Conan Exiles (Unreal)
* The very few Unity games that opt-in to use [Streaming Virtual Texturing](https://docs.unity3d.com/Manual/svt-streaming-virtual-texturing.html), which has been marked as experimental for years and at this point abandoned yet still available

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

The official website used to sit at https://graphinesoftware.com/

* [Archived Homepage](https://web.archive.org/web/20210926130802/http://graphinesoftware.com:80/granite-sdk)
* [SDK PDF](https://web.archive.org/web/20240629135242/https://graphinesoftware.com/sites/default/files/shared/whitepaper_granite_sdk5.pdf)
* [More Details](https://web.archive.org/web/20181220200828/https://graphinesoftware.com/texture-streaming)
* [More Details 2](https://web.archive.org/web/20210616105443/http://graphinesoftware.com:80/our-technology/how-it-works)
* [Use Cases](https://web.archive.org/web/20210616111752/http://graphinesoftware.com:80/our-technology/use-cases)

Other libraries that can handle creating such files:
* [BG3VTexSuite](https://github.com/Brucephalus/BG3VTexSuite/tree/a8a96277c6f21db4c598faf3cb1a7541ff94c48e), which quite literally seems to contain the proprietary official SDK tools (V5) along with traces of decompilation attempts. This repo was somehow available prior to BG3's release date
* [LSLib](https://github.com/Norbyte/lslib), for creating tile sets for BG3

## Building

Requires **.NET 9.0** (VS2022).
