using System.Text.Json;

var outputDirectory = Environment.CurrentDirectory;
Converter.Path = Path.Combine(Environment.CurrentDirectory.Split("bin")[0], "src", "MaaFramework", "xml");
for (var i = 0; i < args.Length; i++)
{
    var arg = args[i];

    if (i == 0) Converter.Path = arg;
    if (i == 1) Converter.Api.Version = arg;
    if (i == 2) outputDirectory = arg;
}

Converter.Path
    .Parse()
    .GetCompoundPaths(kinds: "file")
    .Parse()
    .Convert();

var json = JsonSerializer.Serialize(Converter.Api);
File.WriteAllText(Path.Combine(outputDirectory, "index.json"), json);
