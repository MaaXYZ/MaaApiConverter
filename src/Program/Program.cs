using System.Text.Json;

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
            .GetCompoundPaths(kinds: ["file", "group"], exceptNames: ["MaaPort.h", "MaaMsg.h"])
            .Parse()
            .Convert();

        var outputPath = Path.Combine(outputDirectory, "index.json");
        var json = JsonSerializer.Serialize(Converter.Api, new JsonSerializerOptions
        {
            WriteIndented = true,
            IndentCharacter = '\t',
            NewLine = "\n",
            IndentSize = 1,
        });
        File.WriteAllText(outputPath, json);
        Console.WriteLine($"""Generated index.json => {outputPath}""");

        return outputPath;
    }
}

