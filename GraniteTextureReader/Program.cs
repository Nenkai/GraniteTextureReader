using GraniteTextureReader.TileSet;
using GraniteTextureReader.GDEX;

using CommandLine;

namespace GraniteTextureReader;

internal class Program
{
    public const string Version = "1.1.1";
    
    //======================
    //Main Program
    //======================
    static void Main(string[] args)
    {
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine($"- GraniteTextureReader {Version} by Nenkai/WistfulHopes/AlphaSatanOmega");
        Console.WriteLine("---------------------------------------------");
        Console.WriteLine("- https://github.com/Nenkai");
        Console.WriteLine("- https://github.com/WistfulHopes");
        Console.WriteLine("- https://github.com/AlphaSatanOmega");
        Console.WriteLine("---------------------------------------------");

        var p = Parser.Default.ParseArguments<ExtractVerbs, ExtractAllVerbs, ExtractProjectFileVerbs, DowngradeVerbs>(args);
    
        p.WithParsed<ExtractVerbs>(ExtractSpecific)
        .WithParsed<ExtractAllVerbs>(ExtractAll)
        .WithParsed<ExtractProjectFileVerbs>(ExtractProjectFile)
        .WithParsed<DowngradeVerbs>(Downgrade)
        .WithNotParsed(HandleNotParsedArgs);
    }

    //======================
    //Methods
    //======================

    public static void Extract(string tileSetPath, int layer, string textureName = "")
    {
        if (!File.Exists(tileSetPath))
        {
            Console.WriteLine($"ERROR: Tile set file '{tileSetPath}' does not exist.");
            return;
        }

        var graniteProcessor = new GraniteProcessor();
        try
        {
            graniteProcessor.Read(tileSetPath);
            graniteProcessor.Extract(layer, textureName);

            Console.WriteLine($"Done. Files can be found in the \"_extracted\" folder next to {tileSetPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to extract from {tileSetPath} - {ex.Message}");
            return;
        }
    }

    public static void ExtractSpecific(ExtractVerbs verbs)
    {
        Extract(verbs.TileSetPath, verbs.LayerToExtract, verbs.FileToExtract);
    }

    public static void ExtractAll(ExtractAllVerbs verbs)
    {
        Console.Write("!!!Warning!!!" +
                      "\nExtracting all the textures of a .gts takes up a lot of disk space." +
                      "\nMake sure you have enough extra space reserved on the same drive." +
                      "\nAre you sure you want to continue?. [y/n]");
        if (Console.ReadKey().Key != ConsoleKey.Y)
        {
            Console.WriteLine();
            Console.WriteLine("Aborted.");
            return;
        }
        Extract(verbs.TileSetPath, verbs.LayerToExtract);
    }

    public static void ExtractProjectFile(ExtractProjectFileVerbs verbs)
    {
        if (!File.Exists(verbs.TileSetPath))
        {
            Console.WriteLine($"ERROR: Tile set file '{verbs.TileSetPath}' does not exist.");
            return;
        }

        var tileSetFile = new TileSetFile();
        try
        {
            tileSetFile.Initialize(verbs.TileSetPath);
            var project = tileSetFile.GetProjectFile();
            if (string.IsNullOrEmpty(project))
            {
                Console.WriteLine($"No project file in tile set.");
                return;
            }

            string projPath = Path.GetDirectoryName(Path.GetFullPath(verbs.TileSetPath));
            string outputPath = Path.Combine(projPath, "proj.xml");
            File.WriteAllText(outputPath, project);

            Console.WriteLine($"Project file extracted to {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to extract from {verbs.TileSetPath} - {ex.Message}");
            return;
        }
    }

    public static void Downgrade(DowngradeVerbs verbs)
    {
        if (!File.Exists(verbs.TileSetPath))
        {
            Console.WriteLine($"ERROR: Tile set file '{verbs.TileSetPath}' does not exist.");
            return;
        }

        var tileSetFile = new TileSetFile();
        try
        {
            // Downgrade compatibility & build version. These are checked.
            tileSetFile.Initialize(verbs.TileSetPath);
            if (tileSetFile.Version == 5)
            {
                Console.WriteLine("ERROR: Tile set file is already version 5.");
                return;
            }

            if (tileSetFile.Version < 5)
            {
                Console.WriteLine("ERROR: Tile set under version 5 is not supported.");
                return;
            }

            GDEXItem info = tileSetFile.Metadata[GDEXTags.Information];
            GDEXItem comp = info[GDEXTags.Compatibility];

            GDEXItem compWith = comp[GDEXTags.CompatibleWith];
            compWith[GDEXTags.VersionMajor].SetInt(5);
            compWith[GDEXTags.VersionMinor].SetInt(0);

            GDEXItem buildVersion = comp[GDEXTags.BuildVersion];
            buildVersion[GDEXTags.VersionMajor].SetInt(5);
            buildVersion[GDEXTags.VersionMinor].SetInt(0);

            string dir = Path.GetDirectoryName(Path.GetFullPath(verbs.TileSetPath));

            Console.Write("GTP files needs to be overwritten and aligned to page size. Proceed? [y/n]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine();
                Console.WriteLine("Aborted.");
                return;
            }

            // Align all GTP files to page size. Version 6 seems to have removed this requirement
            foreach (var file in Directory.GetFiles(dir, "*.gtp", SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine($"Padding {Path.GetFileName(file)} to page size, required for V5 (0x{tileSetFile.CustomPageSize:X8})");
                using var fs = File.OpenWrite(file);
                fs.Position = fs.Length;
                Syroot.BinaryData.StreamExtensions.Align(fs, tileSetFile.CustomPageSize, grow: true);
                fs.Position -= 1;
                fs.WriteByte(0); // Just incase it doesn't actually add the bytes at the end of the file
            }

            // Output new .gts file with the appropriate version.
            string outputFile = Path.Combine(dir, $"{Path.GetFileNameWithoutExtension(verbs.TileSetPath)}_v5.gts");
            using (var output = new FileStream(outputFile, FileMode.Create))
                tileSetFile.Write(output, 5);

            Console.WriteLine($"Done, saved as '{outputFile}'.");
            Console.WriteLine($"NOTE: Thumbnail data omitted as it is not yet supported, but the official tile set viewer should read the file just fine.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to extract from {verbs.TileSetPath} - {ex.Message}");
            return;
        }
    }

    public static void HandleNotParsedArgs(IEnumerable<Error> errors)
    {

    }

    //======================
    //Command Line Arguments
    //======================

    [Verb("extract", HelpText = "Extract a specific texture file from a .gts Tile Set file.")]
    public class ExtractVerbs
    {
        [Option(
            't', "tileset", 
            Required = true, 
            HelpText = "Input .gts file. NOTE: Only version 6 is supported.")]
        public string TileSetPath { get; set; }

        [Option(
            'f', "file", 
            Required = true, 
            HelpText = ".gtp texture file name from the tile set to extract.")]
        public string FileToExtract { get; set; }

        [Option(
            'l', "layer(s)", 
            Required = false, 
            HelpText = "Which texture layers to extract (-1 = All, 0 = Albedo, 1 = Normal, 2 = RGB Mask 1, 3 = RGB Mask 2). Defaults to 0.", 
            Default = (int)0)]
        public int LayerToExtract { get; set; }
    }

    [Verb("extract-all", HelpText = "Extract all texture files from a .gts Tile Set file.")]
    public class ExtractAllVerbs
    {
        [Option(
            't', "tileset", 
            Required = true, 
            HelpText = "Input .gts file. NOTE: Only version 6 is supported.")]
        public string TileSetPath { get; set; }

        [Option(
            'l', "layer(s)", 
            Required = false, 
            HelpText = "Which texture layers to extract (-1 = All, 0 = Albedo, 1 = Normal, 2 = RGB Mask 1, 3 = RGB Mask 2). Defaults to 0.", 
            Default = (int)0)]
        public int LayerToExtract { get; set; }

        // [Option('f', "filter", Required = false, HelpText = "Filter. Only paths starting with the specified filter will be extracted.")]
        // public string Filter { get; set; }

        // [Option("overwrite", Required = false, HelpText = "Whether to overwrite if files have already been extracted.")]
        // public bool Overwrite { get; set; }
    }

    [Verb("extract-project-file", HelpText = "Extract the projects file out of the .gts tile set file (if one exists).")]
    public class ExtractProjectFileVerbs
    {
        [Option(
            't', "tileset",
            Required = true,
            HelpText = "Input .gts file.")]
        public string TileSetPath { get; set; }
    }

    [Verb("downgrade", HelpText = "Downgrades a tile set file from version 6 to 5 for use in the granite tile set viewer tool.")]
    public class DowngradeVerbs
    {
        [Option(
            't', "tileset",
            Required = true,
            HelpText = "Input .gts file.")]
        public string TileSetPath { get; set; }
    }
}
