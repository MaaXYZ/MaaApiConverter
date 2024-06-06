using CodegenCS.Models;

public class MaaApiDocument : IJsonInputModel
{
    public string Version { get; set; } = "v0.0.0";
    public Dictionary<string, CompoundDoc> Compounds { get; set; } = [];    // <name, compound>
}

public class CompoundDoc
{
    public string Location { get; set; } = string.Empty;

    public Dictionary<string, TypedefDoc> Typedefs { get; set; } = [];      // <name, typedef>
    public Dictionary<string, DefineDoc> Defines { get; set; } = [];        // <name, define>
    public Dictionary<string, FunctionDoc> Functions { get; set; } = [];    // <name, function>
    public Dictionary<string, EnumDoc> Enums { get; set; } = [];            // <name, enum>
    public Dictionary<string, StructDoc> Structs { get; set; } = [];        // <name, struct>
}

public class DescriptionDoc
{
    public string Brief { get; set; } = string.Empty;
    public List<string> Details { get; set; } = [];                         // Multiple line string
    public string InBody { get; set; } = string.Empty;
}

public class TypedefDoc
{
    public TypeDoc? Type { get; set; }
    public FunctionDoc? FunctionPointer { get; set; }
}
public class TypeDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public string Define { get; set; } = string.Empty;
}

public class DefineDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public string Value { get; set; } = string.Empty;
}

public class FunctionDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public Dictionary<string, string> Types { get; set; } = [];             // <name, description>
    public Dictionary<string, ParameterDoc> Parameters { get; set; } = [];  // <name, parameter>
}

public class ParameterDoc
{
    // TODO: 参数方向
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class EnumDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public Dictionary<string, DefineDoc> EnumValues { get; set; } = [];     // <name, enumValue>
}

public class StructDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public Dictionary<string, VariableDoc> Variables { get; set; } = [];    // <name, variable>
}

public class VariableDoc
{
    public FunctionDoc? FunctionPointer { get; set; }
}
