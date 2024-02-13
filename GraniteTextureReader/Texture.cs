using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraniteTextureReader.GDEX;

namespace GraniteTextureReader;

public class TextureDescriptor
{
    public string Name { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public static TextureDescriptor FromGDEXItem(GDEXItem item)
    {
        var texture = new TextureDescriptor();
        texture.Name = item[GDEXTags.Name].GetString();
        texture.Width = (ushort)item[GDEXTags.Width].GetShort();
        texture.Height = (ushort)item[GDEXTags.Height].GetShort();
        texture.X = (ushort)item[GDEXTags.X].GetShort();
        texture.Y = (ushort)item[GDEXTags.Y].GetShort();
        return texture;
    }
}
