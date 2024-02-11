using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel;

namespace GraniteTextureReader;

[Serializable]
[XmlType(AnonymousType = true)]
[XmlRoot(Namespace = "", IsNullable = false)]
public partial class Project
{
    public ProjectBuildConfig BuildConfig { get; set; }

    [XmlArrayItem("LayerDescription", IsNullable = false)]
    public ProjectLayerDescription[] LayerConfig { get; set; }

    [XmlArrayItem("Asset", IsNullable = false)]
    public ProjectAsset[] ImportedAssets { get; set; }

    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Guid { get; set; }

    [XmlAttribute]
    public decimal GrBuildVersion { get; set; }
    
    [XmlAttribute]
    public string BuildProfile { get; set; }
}


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectBuildConfig
{
    public string OutputDirectory { get; set; }
    public string SoupOutputDirectory { get; set; }
    public string OutputType { get; set; }
    public string OutputName { get; set; }
    public byte WarningLevel { get; set; }
    public string LogFile { get; set; }
    public string TilingMode { get; set; }
    public byte MaximumAnisotropy { get; set; }
    public uint CustomPageSize { get; set; }
    public string CustomTargetDisk { get; set; }
    public ushort CustomBlockSize { get; set; }
    public byte CustomTileWidth { get; set; }
    public byte CustomTileHeight { get; set; }
    public string PagingStrategy { get; set; }
}


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectLayerDescription
{
    
    [XmlAttribute]
    public string CompressionFormat { get; set; }

    [XmlAttribute]
    public string QualityProfile { get; set; }

    [XmlAttribute]
    public string DataType { get; set; }

    [XmlAttribute]
    public string DefaultColor { get; set; }
}


[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectAsset
{
    
    [XmlArrayItemAttribute("Layer", IsNullable = false)]
    public ProjectAssetLayer[] Layers { get; set; }

    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string GUID { get; set; }

    [XmlAttribute]
    public ushort Width { get; set; }

    [XmlAttribute]
    public ushort Height { get; set; }

    [XmlAttribute]
    public byte TargetWidth { get; set; }

    [XmlAttribute]
    public byte TargetHeight { get; set; }

    [XmlAttribute]
    public string AutoScalingMode { get; set; }

    [XmlAttribute]
    public string TilingMethod { get; set; }
    
    [XmlAttribute]
    public string Type { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectAssetLayer
{
    public ProjectAssetLayerTextures Textures { get; set; }

    [XmlAttribute]
    public string QualityProfile { get; set; }

    [XmlAttribute]
    public string Flip { get; set; }

    [XmlAttribute]
    public byte TargetWidth { get; set; }

    [XmlAttribute]
    public byte TargetHeight { get; set; }

    [XmlAttribute]
    public string ResizeMode { get; set; }

    [XmlAttribute]
    public string MipSource { get; set; }

    [XmlAttribute]
    public string TextureType { get; set; }

    [XmlAttribute]
    public string AssetPackingMode { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectAssetLayerTextures
{
    public ProjectAssetLayerTexturesTexture Texture { get; set; }
}

[Serializable]
[XmlType(AnonymousType = true)]
public partial class ProjectAssetLayerTexturesTexture
{
    [XmlAttribute]
    public string Src { get; set; }

    [XmlAttribute]
    public byte SubIndex { get; set; }

    [XmlAttribute]
    public ushort Width { get; set; }

    [XmlAttribute]
    public ushort Height { get; set; }

    [XmlAttribute]
    public byte ArrayIndex { get; set; }

    [XmlAttribute]
    public string LastChangeDate { get; set; }

    [XmlAttribute]
    public byte NumChannels { get; set; }
}
