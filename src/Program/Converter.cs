using System.Data;
using Doxygen.Compound;

namespace MaaApiConverter;

internal static class Converter
{
    public static string Path { get; set; } = string.Empty;
    public static MaaApiDocument Api { get; } = new();

    public static void Convert(this IEnumerable<doxygen> doxygens)
    {
        var structQuery = from mainDoxygen in doxygens
                          from mainCompound in mainDoxygen.compounddef
                          let key = mainCompound.compoundname
                          from innerclass in mainCompound.innerclass
                          where (innerclass?.refid ?? string.Empty).StartsWith("struct")
                          let doxygen = CompoundExecute.Parse(IndexExecute.GetCompoundPath(innerclass.refid!))
                          from compound in doxygen.compounddef
                          select (key, compound);

        var memberQuery = from doxygen in doxygens
                          from compound in doxygen.compounddef
                          let key = compound.compoundname.Dump(key => Api.Compounds.Add(key, new()
                          {
                              Location = compound.location?.file ?? string.Empty
                          }))
                          from section in compound.sectiondef
                          from member in section.memberdef
                          select (key, member);

        ConvertMember(memberQuery);
        ConvertStruct(structQuery);
    }

    #region ConvertMember
    private static void ConvertMember(IEnumerable<(string, memberdefType)> members)
    {
        foreach ((var key, var member) in members)
        {
            Action<memberdefType, CompoundDoc> action = member.kind switch
            {
                "typedef" => ConvertTypedef,
                "define" => ConvertDefine,
                "function" => ConvertFunction,
                "enum" => ConvertEnum,
                _ => DefaultConvert,
            };
            action.Invoke(member, Api.Compounds[key]);
        }
    }

    private static void DefaultConvert(memberdefType member, CompoundDoc compound)
        => Console.WriteLine($"Unconverted members type: {member.kind}");
    private static void ConvertTypedef(memberdefType member, CompoundDoc compound)
        => (!string.IsNullOrEmpty(member.argsstring?.Untyped.Value)).Dump(isFunctionPointer =>
        compound.Typedefs.Add(member.name.Untyped.Value.Trim(), new()
        {
            Type = isFunctionPointer ? null : new()
            {
                Description = ToDescriptionFrom(member),
                Define = member.type?.Untyped.Value.Replace(" *", "* ").Trim() ?? string.Empty,
            },
            FunctionPointer = !isFunctionPointer ? null : new()
            {
                Description = ToDescriptionFrom(member),
                Types = ToTypesFrom(member, isFunctionPointer),
                Parameters = ToParametersFrom(member),
            },
        }));
    private static void ConvertDefine(memberdefType member, CompoundDoc compound)
        => compound.Defines.Add(member.name.Untyped.Value.Trim(), new()
        {
            Description = ToDescriptionFrom(member),
            Value = member.initializer?.Untyped.Value.TrimStart('=').Trim() ?? string.Empty,
        });
    private static void ConvertFunction(memberdefType member, CompoundDoc compound)
        => compound.Functions.Add(member.name.Untyped.Value.Trim(), new()
        {
            Description = ToDescriptionFrom(member),
            Types = ToTypesFrom(member),
            Parameters = ToParametersFrom(member),
        });
    private static void ConvertEnum(memberdefType member, CompoundDoc compound)
    => compound.Enums.Add(member.name.Untyped.Value.Trim(), new()
    {
        Description = ToDescriptionFrom(member),
        EnumValues = ToEnumValuesFrom(member),
        IsFlags = ToIsFlagsFrom(member),
    });
    #endregion

    #region ConvertStruct
    private static void ConvertStruct(IEnumerable<(string, compounddefType)> compounds)
    {
        foreach ((var key, var compound) in compounds)
        {
            Api.Compounds[key].Structs.Add(compound.compoundname.Trim(), new StructDoc()
            {
                Description = ToDescriptionFrom(compound),
                Variables = ToVariablesFrom(compound),
            });
        }
    }
    private static Dictionary<string, VariableDoc> ToVariablesFrom(compounddefType compound)
    {
        var memberQuery = from section in compound.sectiondef
                          from member in section.memberdef
                          select (member.name.Untyped.Value.Trim(), member);
        var doc = new Dictionary<string, VariableDoc>();
        foreach ((var memberName, var member) in memberQuery)
        {
            doc.Add(memberName, new()
            {
                FunctionPointer = new()
                {
                    Description = ToDescriptionFrom(member),
                    Types = ToTypesFrom(member, isFunctionPointer: true),
                    Parameters = ToParametersFrom(member),
                },
            });
        }
        return doc;
    }
    #endregion

    private static readonly StringSplitOptions s_split_Trim_RemoveEmpty = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;
    private static IEnumerable<string> ToDetailQuery(this descriptionType? detail)
        => from para in detail?.para ?? []
           where para.parameterlist.Count is 0  // 参数
           where para.xrefsect.Count is 0       // Deprecated
           where para.simplesect.Count is 0     // return
           from str in para.Untyped.Value.Split('\n', s_split_Trim_RemoveEmpty)
           select str;
    private static IEnumerable<string> ToDeprecatedQuery(this descriptionType? detail)
        => from para in detail?.para ?? []
           where para.xrefsect.Count is not 0
           from xref in para.xrefsect
               //  where string.Concat(xref.xreftitle) == "Deprecated"
           select xref.xrefdescription.Untyped.Value.Trim();
    private static DescriptionDoc ToDescriptionFrom(memberdefType value) => new DescriptionDoc
    {
        Brief = value.briefdescription?.Untyped.Value.Trim() ?? string.Empty,
        Details = value.detaileddescription.ToDetailQuery().ToList().Dump(list =>
        {
            var inBody = value.inbodydescription?.Untyped.Value.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(inBody))
            {
                list.Add(inBody);
                Console.WriteLine($"InBody: {inBody}");
            }
        }),
    }.CheckDeprecated(value.detaileddescription, value);
    private static DescriptionDoc ToDescriptionFrom(compounddefType value) => new DescriptionDoc
    {
        Brief = value.briefdescription?.Untyped.Value.Trim() ?? string.Empty,
        Details = value.detaileddescription.ToDetailQuery().ToList(),
    }.CheckDeprecated(value.detaileddescription, null);
    private static DescriptionDoc ToDescriptionFrom(enumvalueType value)
    {
        // 不用 ToDetailQuery 是因为处理了换行 Count > 1
        var detailQuery = from para in value.detaileddescription?.para ?? []
                          where para.xrefsect.Count is 0
                          select para.Untyped.Value.Trim();
        var doc = new DescriptionDoc()
        {
            Brief = value.briefdescription?.Untyped.Value.Trim() ?? string.Empty,
            Details = detailQuery.ToList(),
        };
        if (!string.IsNullOrWhiteSpace(doc.Brief) || doc.Details.Count < 1)
            goto ret;

        if (doc.Details.Count > 1)
        {
            doc.Brief = doc.Details[0];
            doc.Details.RemoveAt(0);
            goto ret;
        }

        doc.Brief = doc.Details.Single();
        var verbatimQuery = from para in value.detaileddescription?.para ?? []
                            from verbatim in para.verbatim
                            select verbatim.Trim();
        doc.Details = verbatimQuery.ToList();
        if (doc.Details.Count < 1)
            goto ret;

        foreach (var detail in doc.Details)
            doc.Brief = doc.Brief.Replace(detail, string.Empty);
        doc.Brief = doc.Brief.Trim();
ret:
        var splitQuery = from detail in doc.Details
                         from str in detail.Split('\n', s_split_Trim_RemoveEmpty)
                         select str;
        doc.Details = splitQuery.ToList();
        return doc.CheckDeprecated(value.detaileddescription, null);
    }
    private static DescriptionDoc CheckDeprecated(this DescriptionDoc description, descriptionType? detail, memberdefType? member)
    {
        if (member?.definition?.Untyped.Value.Contains("MAA_DEPRECATED") is true)
            description.Deprecated = string.Empty;

        var deprecatedQuery = ToDeprecatedQuery(detail);
        if (deprecatedQuery.Any())
            description.Deprecated = deprecatedQuery.Single();
        return description;
    }
    private static Dictionary<string, DefineDoc> ToEnumValuesFrom(memberdefType member)
    {
        var doc = new Dictionary<string, DefineDoc>();
        foreach (var enumValue in member.enumvalue)
            doc.Add(enumValue.name.Untyped.Value.Trim(), new()
            {
                Description = ToDescriptionFrom(enumValue),
                Value = enumValue.initializer?.Untyped.Value.TrimStart('=').Trim() ?? string.Empty,
            });
        return doc;
    }
    private static bool ToIsFlagsFrom(memberdefType member)
        => member.enumvalue.Select(x => x.initializer?.Untyped.Value ?? string.Empty).Any(x => x.Contains("<<") || x.Contains('|'));
    private static Dictionary<string, string> ToTypesFrom(memberdefType member, bool isFunctionPointer = false)
    {
        var doc = new Dictionary<string, string>();

        var deprecatedQuery = ToDeprecatedQuery(member.detaileddescription);
        var typeQuery = isFunctionPointer
            ? member.type is null ? [] : [member.type.Untyped.Value.Trim('(', ')', ' ', '*')]
            : member.definition?.Untyped.Value.Replace(" *", "* ").Split(' ', s_split_Trim_RemoveEmpty)[..^1] ?? [];
        var returnQuery = from description in member.detaileddescription?.para ?? []
                          from @return in description.simplesect
                          from para in @return.para
                          select para.Untyped.Value.Trim();

        if (deprecatedQuery.Any())
            doc["MAA_DEPRECATED"] = string.Empty; // deprecatedQuery.Single();
        foreach (var type in typeQuery)
            doc.TryAdd(type, string.Empty);

        if (returnQuery.Any())
        {
            var desc = returnQuery.Single().Split(' ');
            var type = desc[0];
            if (!doc.ContainsKey(type))
            {
                type = doc.Keys.Where(x => !x.Contains("MAA_")).Single();
                doc[type] = string.Join(' ', desc);
            }
            else
            {
                doc[type] = string.Join(' ', desc[1..]);
            }
        }
        return doc;
    }
    private static Dictionary<string, ParameterDoc> ToParametersFrom(memberdefType member)
    {
        var doc = new Dictionary<string, ParameterDoc>();
        var paramDescr = from description in member.detaileddescription?.para ?? []
                         from param in description.parameterlist
                         from item in param.parameteritem
                         select item;
        string GetItemName(docParamListItem item)
            => item.parameternamelist.Single().parametername.Single().Untyped.Value.Trim();
        ParameterDirection GetItemDirection(docParamListItem? item, out bool isRef)
        {
            var ret = (ParameterDirection)0;
            var dir = item?.parameternamelist.Single().parametername.Single().direction;
            isRef = !string.IsNullOrEmpty(dir);
            if (!isRef)
                return ret;

            if (dir!.Contains("in", StringComparison.OrdinalIgnoreCase))
                ret |= ParameterDirection.Input;
            if (dir.Contains("out", StringComparison.OrdinalIgnoreCase))
                ret |= ParameterDirection.Output;
            return ret;
        }
        string GetItemDescription(docParamListItem? item)
            => item?.parameterdescription.para.Single()?.Untyped.Value.Trim() ?? string.Empty;

        var paramQuery = from param in member.argsstring?.Untyped.Value.Replace(" *", "* ").Split((char[])[',', '(', ')'], s_split_Trim_RemoveEmpty)
                         let paramTypeName = param.Split(' ', s_split_Trim_RemoveEmpty)
                         let name = paramTypeName[^1]
                         join item in paramDescr on name equals GetItemName(item) into items
                         from item in items.DefaultIfEmpty()
                         select (name, new ParameterDoc
                         {
                             Type = string.Join(' ', paramTypeName[..^1]),
                             Direction = GetItemDirection(item, out var isRef),
                             Description = GetItemDescription(item),
                             IsPointerToArray = member.param.Where(x => x.declname?.Untyped.Value == name).SingleOrDefault()?.briefdescription?.Untyped.Value
                                    .Contains("array", StringComparison.OrdinalIgnoreCase)
                                    ?? isRef ? false : null,
                         });
        foreach ((var paramName, var paramDoc) in paramQuery)
            doc.Add(paramName, paramDoc);
        return doc;
    }
}
