public class MaaApiDocument : CodegenCS.Models.IJsonInputModel
{
    public string Version { get; set; } = "v0.0.0";
    public Dictionary<string, CompoundDoc> Compounds { get; set; } = [];
}

public class CompoundDoc
{
    public string Location { get; set; } = string.Empty;

    public Dictionary<string, TypedefDoc> Typedefs { get; set; } = [];
    public Dictionary<string, DefineDoc> Defines { get; set; } = [];
    public Dictionary<string, FunctionDoc> Functions { get; set; } = [];
    public Dictionary<string, EnumDoc> Enums { get; set; } = [];
    public Dictionary<string, StructDoc> Structs { get; set; } = [];
}

public class DescriptionDoc
{
    public string Brief { get; set; } = string.Empty;

    // Multiple line string
    public List<string> Details { get; set; } = [];
    public string? Deprecated { get; set; } = null;
}

public class TypedefDoc
{
    public TypeDoc? Type { get; set; } = null;
    public FunctionDoc? FunctionPointer { get; set; } = null;
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
    public Dictionary<string, string> Types { get; set; } = [];
    public Dictionary<string, ParameterDoc> Parameters { get; set; } = [];
}

public class ParameterDoc
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public System.Data.ParameterDirection Direction { get; set; } = 0;
    public bool? IsPointerToArray { get; set; } = null;
}

public class EnumDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public Dictionary<string, DefineDoc> EnumValues { get; set; } = [];
    public bool IsFlags { get; set; } = false;
}

public class StructDoc
{
    public DescriptionDoc Description { get; set; } = new();
    public Dictionary<string, VariableDoc> Variables { get; set; } = [];
}

public class VariableDoc
{
    public FunctionDoc? FunctionPointer { get; set; } = null;
}
