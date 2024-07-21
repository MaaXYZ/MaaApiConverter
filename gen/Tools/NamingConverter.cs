public class Naming
{
    public static Naming To { get; } = new();

    public string PascalCase(string name) => ConvertName(name, isLower: false, isSnake: false);
    public string camelCase(string name) => ConvertName(name, isLower: true, isSnake: false);
    public string Snake_Case_Upper(string name) => ConvertName(name, isLower: false, isSnake: true);
    public string snake_case_lower(string name) => ConvertName(name, isLower: true, isSnake: true);

    private static string ConvertName(string name, bool isLower, bool isSnake)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }
        // PascalCase -> snake_case_lower
        if (!name.Contains('_') && isSnake)
            name = System.Text.Json.JsonNamingPolicy.SnakeCaseLower.ConvertName(name);
        if (isLower && isSnake)
            return name;

        // PascalCase -> PascalCase camelCase
        // PascalCase -> snake_case_lower -> Snake_Case_Upper
        // snake_case -> PascalCase camelCase Snake_Case_Upper snake_case_lower
        var strs = name.Split('_');
        var newStr = string.Join(isSnake ? "_" : string.Empty, strs);
        return string.Create(newStr.Length, newStr, (chars, newStr) =>
        {
            newStr.CopyTo(chars);
            chars[0] = isLower ? char.ToLowerInvariant(chars[0]) : char.ToUpperInvariant(chars[0]);

            var i = 0;
            foreach (var str in strs[..^1])
            {
                i += isSnake ? str.Length + 1 : str.Length;
                chars[i] = isLower && isSnake ? char.ToLowerInvariant(chars[i]) : char.ToUpperInvariant(chars[i]);
            }
        });
    }

    public static void Test()
    {
        var a = "d_d__dDxD_Dx";
        var b = "dDxxDDx";
        Console.WriteLine(To.PascalCase(a));        // DDDDxDDx
        Console.WriteLine(To.camelCase(a));         // dDDDxDDx
        Console.WriteLine(To.snake_case_lower(a));  // d_d__dDxD_dx
        Console.WriteLine(To.Snake_Case_Upper(a));  // D_D__DDxD_Dx
        Console.WriteLine(To.PascalCase(b));        // DDxxDDx
        Console.WriteLine(To.camelCase(b));         // dDxxDDx
        Console.WriteLine(To.snake_case_lower(b));  // d_dxx_d_dx
        Console.WriteLine(To.Snake_Case_Upper(b));  // D_Dxx_D_Dx
    }
}
