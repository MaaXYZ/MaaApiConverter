using Doxygen.Index;

internal static class IndexExecute
{
    public static doxygenindex Parse(this string path)
        => doxygenindex.Load(Path.Combine(path, "index.xml").Dump(Console.WriteLine));

    public static string GetCompoundPath(string refid)
        => Path.Combine(Converter.Path, $"{refid}.xml");

    public static IEnumerable<string> GetCompoundPaths(this doxygenindex index, params string[] kinds)
        => from compound in index.compound
           where kinds.Contains(compound.kind)
           select GetCompoundPath(compound.refid);
}
