# GraniteTextureReader

Texture Reader/Extractor for [Granite SDK](https://unity.com/products/granite-sdk) files (.gts/.gtp). This tool was primarly made with Granblue Fantasy: Relink in mind, but should also support extracting from Unity games using [Streaming Virtual Texturing](https://docs.unity3d.com/Manual/svt-streaming-virtual-texturing.html).

> [!IMPORTANT]  
> Only version 6 `.gts` files are currently supported. (Version can be viewed at 0x04 in the .gts files)

Layer Numbers
* `-1` 	- Extracts all Texture layers
* `0` 	- Extract Albedo map layer
* `1` 	- Extract Normal map layer
* `2` 	- Extract RGB Mask 1 map layer
* `3` 	- Extract RGB Mask 2 map layer

### Extract Single Texture from .gts
```
GraniteTextureReader.exe extract -t "<path to .gts>" -f "<granite texture name>" -l <layer number>
```

### Extract All Textures from .gts
```
GraniteTextureReader.exe extract-all -t "<path to .gts>" -l <layer number>
```

## File types
* `.gts` - Tile Set
* `.gtp` - Page File
