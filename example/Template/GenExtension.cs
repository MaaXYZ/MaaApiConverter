public static class GenExtension
{
    public static FormattableString Gen(this string? check, Func<string, FormattableString> f)
        => string.IsNullOrWhiteSpace(check) ? $"" : f.Invoke(check);

    public static FormattableString Gen<T>(this IEnumerable<T> check, Func<IEnumerable<T>, FormattableString> f)
        => check.Any() ? f.Invoke(check) : $"";

    public static FormattableString Gen<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> check, Func<TKey, TValue, FormattableString> f, string? separator = "\r\n")
        => check.Any() ? check.Select(x => f.Invoke(x.Key, x.Value)).ToSingle(separator) : $"";

    public static FormattableString ToSingle(this IEnumerable<FormattableString> formattableStrings, string? separator)
    {
        var query = (formattableStrings ?? []).Where(f => f.Format != string.Empty);
        if (!query.Any()) return $"";
        var format = string.Join(separator, Enumerable.Range(0, query.Count()).Select(i => $"{{{i}}}"));
        return FormattableStringFactory.Create(format, query.ToArray());
    }
    public static FormattableString Join(string? separator, params FormattableString[] formattableStrings)
        => formattableStrings.ToSingle(separator);

    public class GenTree<TKey, TValue> : Dictionary<TKey, GenTree<TKey, TValue>> where TKey : notnull
    {
        public TValue? Value { get; set; }
        public TValue? this[IEnumerable<TKey> keys]
        {
            get => keys.Aggregate(this, (tree, key) => tree[key]).Value;
            set => keys.Aggregate(this, (tree, key) => tree.TryGetValue(key, out var value) ? value : tree[key] = []).Value = value;
        }
    }

    /// <remarks>
    ///     Action 中使用 ICodegenTextWriter
    /// </remarks>
    public static void Gen<TKey, TValue>(this GenTree<TKey, TValue> tree, Action<TKey, Action> ak, Action<TKey, TValue> akv) where TKey : notnull
    {
        foreach ((var nodeName, var node) in tree)
        {
            if (node.Count == 0 && node.Value is not null) // 子节点为叶子节点
                akv.Invoke(nodeName, node.Value);
            else
                ak.Invoke(nodeName, () =>
                {
                    node.Gen(ak, akv);
                });
        }
    }
}
