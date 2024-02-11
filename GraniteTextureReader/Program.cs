using GraniteTextureReader.TileSet;

using CommandLine;

namespace GraniteTextureReader;

internal class Program
{
    public const string Version = "1.0.0";
    
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

        var p = Parser.Default.ParseArguments<ExtractVerbs, ExtractAllVerbs, ExtractProjectFileVerbs>(args);
    
        p.WithParsed<ExtractVerbs>(ExtractSpecific)
        .WithParsed<ExtractAllVerbs>(ExtractAll)
        .WithParsed<ExtractProjectFileVerbs>(ExtractProjectFile)
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
}
