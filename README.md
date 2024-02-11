# GraniteTextureReader

Texture Reader Tool for Granblue Fantasy: Relink Granite Tile Sheets (.gts)

## Usage
Make sure you have extracted all the `.gts`, .`gtp` files under `Granblue Fantasy Relink\data\granite\<2/4>k\gts\<0,1,2>` using [GBFRDataTools](https://github.com/Nenkai/GBFRDataTools/releases).

Layer Numbers
* `-1` 	- Extracts all Texture layers
* `0` 	- Extract Albedo map layer
* `1` 	- Extract Normal map layer
* `2` 	- Extract RGB Mask 1 map layer
* `3` 	- Extract RGB Mask 2 map layer

Extract Single Texture from .gts
```
GraniteTextureReader.exe extract -t "<path to .gts>" -f "<granite texture name>" -l <layer number>
```

Extract All Textures from .gts
```
GraniteTextureReader.exe extract-all -t "<path to .gts>" -l <layer number>
```

File types
* `.gts` - Tile Sheet
* `.gtp` - Tile Pack

## Finding a model's textures
If you need to find a certain model's textures you'll need to run download and run flatc with the MMat_ModelMaterial.fbs file.

1. Download Windows.flatc.binary.zip from [here](https://github.com/google/flatbuffers/releases/tag/v23.5.26), and extract it anywhere.

2. Download MMat_ModelMaterial.fbs from [here](https://github.com/Nenkai/010GameTemplates/blob/main/Cygames/Granblue%20Fantasy%20-%20Relink/MMat_ModelMaterial.fbs).

3. Run `flatc.exe --json "<path_to_MMat_ModelMaterial.fbs>" -- "<path_to_mmat_file>" --raw-binary`, and a JSON file will be spat out into the flatc directory.

4. Open the JSON file and search for the texture group you wish to edit (ex: pl0001_hair). A little below it should be a section called `Granite`. 

5. Copy the very long `PageFile` string, and use it as the `"<granite texture name>"` for the Extract Single Texture from .gts command.