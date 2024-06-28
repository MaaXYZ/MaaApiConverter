using Doxygen.Compound;

namespace MaaApiConverter;

internal static class CompoundExecute
{
    public static doxygen Parse(string path)
        => doxygen.Load(path.Dump(Console.WriteLine));

    public static IEnumerable<doxygen> Parse(this IEnumerable<string> paths)
        => from path in paths
           select Parse(path);
}
