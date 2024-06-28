using Doxygen.Index;

namespace MaaApiConverter;

internal static class IndexExecute
{
    public static doxygenindex Parse(this string path)
        => doxygenindex.Load(Path.Combine(path, "index.xml").Dump(Console.WriteLine));

    public static string GetCompoundPath(string refid)
        => Path.Combine(Converter.Path, $"{refid}.xml");

    public static IEnumerable<string> GetCompoundPaths(this doxygenindex index, string[] kinds, string[] exceptNames)
        => from compound in index.compound
           where kinds.Contains(compound.kind)
           where !exceptNames.Contains(compound.name)
           select GetCompoundPath(compound.refid);
}
