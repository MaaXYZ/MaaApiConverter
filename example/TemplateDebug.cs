// Template Debug 命令行程序

using CodegenCS.IO;

// 先生成并运行一遍 MaaApiConverter, 以获取 index.json
var model = new ModelFactory([]).LoadModelFromFile<MaaApiDocument>(MaaApiConverter.Program.Generate([]));

// 当前目录设置为 Convert.ps1 脚本的所在目录
Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory.Split("bin")[0], "example", "Template");
var ctx = new CodegenContext();
new CSharpTemplate().Main(new ColoredConsoleLogger(), ctx, model);

if (ctx.OutputFiles.Count > 0 && ctx.Errors.Count == 0)
{
    // Before saving the generated files we delete the previously generated files (to avoid conflicts)
    if (Directory.Exists(Environment.CurrentDirectory))
        Directory.GetFiles(Environment.CurrentDirectory, "*.generated.cs", SearchOption.AllDirectories).ToList().ForEach(File.Delete);

    ctx.DefaultOutputFile.RelativePath = "MaaApiConvert.generated.cs";
    ctx.SaveToFolder(Environment.CurrentDirectory);
}
