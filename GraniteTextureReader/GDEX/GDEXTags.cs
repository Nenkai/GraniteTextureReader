using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraniteTextureReader.GDEX;

public class GDEXTags
{
    public const uint MetaData = 0x4154454D;
    public const uint Atlas = 0x534C5441;
    public const uint TextureSet = 0x53545854;
    public const uint Texture = 0x52545854;
    public const uint Name = 0x454D414E;
    public const uint Width = 0x48544457;
    public const uint Height = 0x54484748;
    public const uint XXXX = 0x58585858;
    public const uint YYYY = 0x59595959;
    public const uint TilingMethod = 0x52444441;
    public const uint SRGB = 0x42475253;
    public const uint Thumbnail = 0x424D4854;
    public const uint Project = 0x4A4F5250;
    public const uint LayerInfo = 0x464E494C;
    public const uint Layer = 0x5259414C;
    public const uint Index = 0x58444E49;
    public const uint FormatType = 0x45505954;
    public const uint Information = 0x4F464E49;
    public const uint Component = 0x4F464E49;
    public const uint CMPW = 0x57504D43;
    public const uint VersionMajor = 0x524A414D;
    public const uint VersionMinor = 0x524E494D;
    public const uint BuildVersion = 0x56444C42;
    public const uint BuildInformation = 0x464E4942;
    public const uint DateTime = 0x45544144;
    public const uint Blocks = 0x534B4C42;
    public const uint TilingMode = 0x454C4954;
    public const uint BuildProfile = 0x52504442;
    public const uint LTMP = 0x504D544C;
}
