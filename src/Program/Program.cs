namespace MaaApiConverter;

public static class Program
{
    private static void Main(string[] args) => Generate(args);

    public static string Generate(string[] args)
    {
        var outputDirectory = Environment.CurrentDirectory;
        Converter.Path = Path.GetFullPath(Path.Join(Environment.CurrentDirectory.Split("bin")[0], "..", "src", "MaaFramework", "xml"));
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (i == 0) Converter.Path = arg;
            if (i == 1) Converter.Api.Version = arg;
            if (i == 2) outputDirectory = arg;
        }

        Converter.Path
            .Parse()
            .GetCompoundPaths(kinds: ["file"], exceptNames: ["MaaPort.h"])
            .Parse()
            .Convert();

        var outputPath = Path.Combine(outputDirectory, "index.json");
        var json = System.Text.Json.JsonSerializer.Serialize(Converter.Api);
        File.WriteAllText(outputPath, json);
        Console.WriteLine($"""Generated index.json => {outputPath}""");

        return outputPath;
    }
}

