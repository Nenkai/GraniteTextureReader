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
    public uint Width { get; set; }
    public uint Height { get; set; }
    public uint X { get; set; }
    public uint Y { get; set; }

    public static TextureDescriptor FromGDEXItem(GDEXItem item)
    {
        var texture = new TextureDescriptor();
        texture.Name = item[GDEXTags.Name].GetString();
        texture.Width = (uint)item[GDEXTags.Width].GetInt();
        texture.Height = (uint)item[GDEXTags.Height].GetInt();
        texture.X = (uint)item[GDEXTags.X].GetInt();
        texture.Y = (uint)item[GDEXTags.Y].GetInt();
        return texture;
    }
}
