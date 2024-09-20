using System.Data;

// Todo: 所有 Rename 放最开头，现在因为各个方法都耦合了 Rename 相关的快成屎山了
class CSharpTemplate
{
    ILogger _logger = null!;
    readonly Dictionary<string, string> _typedefs = new()
    {
        ["int32_t"] = "System.Int32",
        ["int64_t"] = "System.Int64",
        ["uint8_t"] = "System.Byte",
        ["uint64_t"] = "System.UInt64",
    };
    readonly Dictionary<string, string> _customHandleTypedefs = new()
    {
        ["MaaContext*"] = "MaaContextHandle",
        ["MaaController*"] = "MaaControllerHandle",
        ["MaaCustomControllerCallbacks*"] = "MaaCustomControllerCallbacksHandle",
        ["MaaImageBuffer*"] = "MaaImageBufferHandle",
        ["MaaImageListBuffer*"] = "MaaImageListBufferHandle",
        ["MaaRect*"] = "MaaRectHandle",
        ["MaaResource*"] = "MaaResourceHandle",
        ["MaaStringBuffer*"] = "MaaStringBufferHandle",
        ["MaaStringListBuffer*"] = "MaaStringListBufferHandle",
        ["MaaTasker*"] = "MaaTaskerHandle",
        ["MaaToolkitAdbDevice*"] = "MaaToolkitAdbDeviceHandle",
        ["MaaToolkitAdbDeviceList*"] = "MaaToolkitAdbDeviceListHandle",
        ["MaaToolkitDesktopWindow*"] = "MaaToolkitDesktopWindowHandle",
        ["MaaToolkitDesktopWindowList*"] = "MaaToolkitDesktopWindowListHandle",

    };
    readonly Dictionary<string, string> _types = new()
    {
        ["MaaBool*"] = "{0} MaaBool",
        ["MaaNodeId*"] = "{0} MaaNodeId{1}",
        ["MaaOptionValue"] = "byte[]",
        ["MaaRecoId*"] = "{0} MaaRecoId",
        ["MaaSize*"] = "{0} MaaSize",

        ["const char*"] = "string",
        ["char*"] = "string",
        ["void*"] = "nint",

        ["int32_t"] = "int",
        ["uint64_t"] = "ulong",

        ["MaaAdbInputMethod"] = "MaaAdbInputMethod",
        ["MaaAdbScreencapMethod"] = "MaaAdbScreencapMethod",
        ["MaaBool"] = "MaaBool",
        ["MaaCtrlId"] = "MaaCtrlId",
        ["MaaCtrlOption"] = "MaaCtrlOption",
        ["MaaCustomActionCallback"] = "MaaCustomActionCallback",
        ["MaaCustomRecognitionCallback"] = "MaaCustomRecognitionCallback",
        ["MaaDbgControllerType"] = "MaaDbgControllerType",
        ["MaaGlobalOption"] = "MaaGlobalOption",
        ["MaaImageEncodedData"] = "MaaImageEncodedData",
        ["MaaImageRawData"] = "MaaImageRawData",
        ["MaaNodeId"] = "MaaNodeId",
        ["MaaNotificationCallback"] = "MaaNotificationCallback",
        ["MaaOptionValueSize"] = "MaaOptionValueSize",
        ["MaaRecoId"] = "MaaRecoId",
        ["MaaResId"] = "MaaResId",
        ["MaaResOption"] = "MaaResOption",
        ["MaaSize"] = "MaaSize",
        ["MaaStatus"] = "MaaStatus",
        ["MaaTaskerOption"] = "MaaTaskerOption",
        ["MaaTaskId"] = "MaaTaskId",
        ["MaaWin32InputMethod"] = "MaaWin32InputMethod",
        ["MaaWin32ScreencapMethod"] = "MaaWin32ScreencapMethod",
        ["void"] = "void",
    };
    readonly Dictionary<string, string> _enumdefs = new()
    {
        ["MaaCtrlOption"] = "ControllerOption",
        ["MaaGlobalOption"] = "GlobalOption",
        ["MaaResOption"] = "ResourceOption",
        ["MaaTaskerOption"] = "TaskerOption",

        ["MaaLoggingLevel"] = "LoggingLevel",
        ["MaaStatus"] = "MaaJobStatus",

        ["MaaAdbInputMethod"] = "AdbInputMethods",
        ["MaaAdbScreencapMethod"] = "AdbScreencapMethods",
        ["MaaWin32ScreencapMethod"] = "Win32ScreencapMethod",
        ["MaaWin32InputMethod"] = "Win32InputMethod",
        ["MaaDbgControllerType"] = "DbgControllerType",
    };
    private readonly Dictionary<string, string> _enumVariabledefs = new()
    {
        ["MaaAdbControllerType"] = "AdbControllerTypes",
        ["MaaDbgControllerType"] = "DbgControllerType",
        ["MaaThriftControllerType"] = "ThriftControllerType",
        ["MaaWin32ControllerType"] = "Win32ControllerTypes",
    };
    private readonly Dictionary<string, string> _structKeys = new()
    {
        ["MaaCustomControllerCallbacks"] = "Controller",
    };
    private readonly Dictionary<string, string> _unmanagedToManaged = new()
    {
        ["MaaStringBuffer*"] = "new Buffers.MaaStringBuffer({0})",
        ["MaaImageBuffer*"] = "new MaaImageBuffer({0})",
        // ["const char*"] = "{0}.ToStringUTF8()",
        ["void*"] = "",

        ["MaaBool"] = "{0}.ToMaaBool()",
    };

    FormattableString Join(params FormattableString[] formattableStrings) => GenExtension.Join(Environment.NewLine, formattableStrings);

    void Rename(MaaApiDocument api)
    {
        static void Name<T>(bool? isPascalCase, Dictionary<string, T> docs, Action<T>? action = null)
        {
            foreach ((var name, var doc) in docs.ToArray())
            {
                if (isPascalCase is not null)
                {
                    docs.Remove(name);
                    if (isPascalCase is true)
                        docs.Add(Naming.To.PascalCase(name), doc);
                    else
                        docs.Add(Naming.To.camelCase(name), doc);
                }
                action?.Invoke(doc);
            }
        }

        foreach (var (compoundName, compound) in api.Compounds.ToArray())
        {
            api.Compounds.Remove(compoundName);
            api.Compounds.Add(compoundName.EndsWith(".h") ? compoundName[..^2] : compoundName, compound);
            if (compoundName.Contains("MaaMsg")) continue;

            Name(null, compound.Typedefs, typedef
                => Name(false, typedef.FunctionPointer?.Parameters ?? []));
            Name(true, compound.Defines);
            Name(true, compound.Functions, func
                => Name(false, func.Parameters));
            Name(true, compound.Enums, @enum
                => Name(true, @enum.EnumValues, enumValue
                    => enumValue.Value = Naming.To.PascalCase(enumValue.Value)));
            Name(true, compound.Structs, @struct
                => Name(true, @struct.Variables, variable
                    => Name(false, variable.FunctionPointer?.Parameters ?? [])));
        }
    }

    public void Main(ILogger logger, ICodegenContext context, MaaApiDocument api)
    {
        if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "./src/MaaFramework.Binding/")))
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "./src/MaaFramework.Binding/"), true);
        if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "./src/MaaFramework.Binding.Native/")))
            Directory.Delete(Path.Combine(Environment.CurrentDirectory, "./src/MaaFramework.Binding.Native/"), true);

        Rename(api);
        _logger = logger;
        _logger.WriteLineAsync($"MaaApiDocument Version: {api.Version}");
        // var writer = context.DefaultOutputFile;

        foreach (var (compoundName, compound) in api.Compounds)
        {
            if (compoundName == "MaaMsg")
            {
                if (compound.Enums.Count + compound.Functions.Count + compound.Structs.Count + compound.Typedefs.Count > 0)
                    throw new InvalidOperationException();

                var writer = context["./src/MaaFramework.Binding/MaaMsg.cs"];
                WriteAutoGenerated(writer);

                var msgTree = new GenExtension.GenTree<string, DefineDoc>();
                foreach (var (msgName, msg) in compound.Defines)
                {
                    var subNames = msgName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    msgTree[subNames] = msg;
                }

                writer
                    .WriteLine("﻿namespace MaaFramework.Binding.Messages;")
                    .WriteLine()
                    .WriteLine($"//MaaApiDocument Version: {api.Version}")
                    .WriteLine("""
                                /// <summary>
                                ///  A callback consists of a message and a payload.
                                ///  The message is a string that indicates the type of the message.
                                ///  The payload is a JSON object that contains the details of the message.
                                /// </summary>
                                """);
                msgTree.Gen((name, action) => writer.WithCBlock($"public static class {name}", action),
                            (name, doc) => writer.Write(GenDocument(doc.Description)).EnsureEmptyLine()
                                                 .WriteLine(GenDefine(name, doc, "public"))
                                                 .WriteLine());
                continue;
            }

            if (compound.Typedefs.Count + compound.Functions.Count + compound.Defines.Count + compound.Structs.Count > 0)
            {
                var location = compound.Location.Replace("include/Maa", "./src/MaaFramework.Binding.Native/Interop/").Replace(".h", ".cs");
                var writer = context[location];
                WriteAutoGenerated(writer);

                var globalDelegates = compound.Typedefs.Where(x => x.Value.FunctionPointer is not null).ToDictionary(x => x.Key.Replace("API", "Api"), x => x.Value.FunctionPointer!);
                var globalUsings = compound.Typedefs.Where(x => x.Value.Type is not null).ToDictionary(x => x.Key, x => x.Value.Type!);

                if (globalUsings.Count > 0)
                    WriteGlobalUsings(writer, globalUsings);
                if (location.Contains("MaaToolkitDef"))
                    writer.WriteLine("namespace MaaFramework.Binding.Interop.Native;").WriteLine();

                if (compound.Functions.Count + compound.Defines.Count + globalDelegates.Count > 0)
                {
                    writer
                        .WriteLine("using System.Runtime.InteropServices;")
                        .WriteLine()
                        .WriteLine("namespace MaaFramework.Binding.Interop.Native;")
                        .WriteLine();

                    if (globalDelegates.Count > 0)
                        WriteGlobalDelegates(writer, globalDelegates);

                    var className = compoundName.Contains("MaaToolkit") ? "MaaToolkit" : compoundName;
                    if (compound.Functions.Count + compound.Defines.Count > 0)
                        writer
                            .WithCBlock(
                                $"public static partial class {className}", () =>
                                {
                                    writer.WriteLine(Join(
                                        GenDefines(compound.Defines.Where(x => !x.Key.EndsWith("Mask")).ToDictionary()),
                                        GenFunctions(compound.Functions)));
                                })
                            .WriteLine();
                }

                if (compound.Structs.Count > 0)
                {
                    (var structName, var @struct) = compound.Structs.Single();
                    HashSet<string> specials = ["MaaRect"];
                    if (!specials.Contains(structName))
                    {
                        if (compound.Typedefs.Count + compound.Functions.Count + compound.Defines.Count > 0)
                            throw new InvalidOperationException();
                        WriteStruct(writer, structName, @struct);
                    }
                }
            }

            compound.Enums = compound.Enums.ToDictionary(x => x.Key.Replace("Enum", string.Empty), x => x.Value);
            foreach ((var enumName, var @enum) in compound.Enums)
            {
                string ReplaceName(string name) => name
                        .Replace(enumName, string.Empty)
                        .Replace(enumName.Replace("Type", string.Empty), string.Empty)
                        .Replace("0ULL", "0")
                        .Replace("1ULL", "1");

                var newEnumName = _enumdefs[enumName];
                var subDir = newEnumName.Contains("ControllerType") ? "Controllers/"
                    : newEnumName.Contains("Method") ? "Controllers/"
                    : newEnumName.Contains("Option") ? "Options/"
                    : string.Empty;
                var location = "./src/MaaFramework.Binding/Enums/" + subDir + newEnumName + ".cs";
                var writer = context[location];
                WriteAutoGenerated(writer);

                @enum.EnumValues = compound.Defines
                    .Where(pair => pair.Key.Contains(enumName))
                    .Concat(@enum.EnumValues)
                    .ToDictionary(
                        x => ReplaceName(x.Key),
                        x =>
                        {
                            x.Value.Value = ReplaceName(x.Value.Value);
                            return x.Value;
                        });
                @enum.UnderlyingType = _typedefs[@enum.UnderlyingType];
                writer
                    .WriteLine("namespace MaaFramework.Binding;")
                    .WriteLine();
                WriteEnum(writer, newEnumName, @enum);
            }
        }

        if (UnreplacedTypes.Count > 0)
            logger.WriteLineAsync("UnreplacedTypes:");
        foreach (var t in UnreplacedTypes)
            logger.WriteLineAsync($"""["{t}"] = "{t}",""");
    }

    void WriteAutoGenerated(ICodegenTextWriter writer) => writer.WriteLine("""
        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by a tool.
        //
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------
        
        #pragma warning disable CS1573 // 参数在 XML 注释中没有匹配的 param 标记
        #pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

        """);

    # region GlobalUsing
    void WriteGlobalUsings(ICodegenTextWriter writer, Dictionary<string, TypeDoc> types)
    {
        foreach (var (name, type) in types)
        {
            if (_customHandleTypedefs.TryGetValue($"{name}*", out var custom))
            {
                writer.EnsureEmptyLine().Write(GenComment(type.Description))
                    .EnsureEmptyLine().Write($"global using {custom} = nint;");
                continue;
            }

            if (type.Define.EndsWith('*')) type.Define = "nint";
            type.Define = _typedefs.GetValueOrDefault(type.Define, type.Define);
            type.Define = types.GetValueOrDefault(type.Define, type).Define;
            writer.EnsureEmptyLine().Write(GenComment(type.Description))
                .EnsureEmptyLine().Write($"global using {name} = {type.Define};");
        }

        writer.EnsureEmptyLine().WriteLine();
    }
    #endregion

    # region Function Delegate
    void WriteGlobalDelegates(ICodegenTextWriter writer, Dictionary<string, FunctionDoc> functions) => writer.WriteLine(functions.Gen(separator: "\r\n\r\n", f: (name, function) => Join(
            GenDocument(function),
            GenAttribute(function.Types),
            GenDelegate(name, function))
        )).EnsureEmptyLine().WriteLine();
    FormattableString GenFunctions(Dictionary<string, FunctionDoc> functions) => functions.Gen(separator: "\r\n\r\n", f: (name, function) => Join(
        GenDocument(function),
        GenAttribute(function.Types),
        GenFunction(name, function)));
    (string @return, FormattableString returnAttribute, FormattableString @params) GenFunctionElement(string name, FunctionDoc function)
    {
        Dictionary<string, (string Old, string New)> specials = new()
        {
            ["MaaStringBufferSetEx"] = ("string", "byte[]"),
        };

        var @return = GenReturn(function.Types, out var returnAttribute);
        var @params = GenParameters(function.Parameters);
        if (specials.TryGetValue(name, out var t))
            @params = FormattableStringFactory.Create(@params.Format, @params.GetArguments().Select(x => (x as string)?.ToString().Replace(t.Old, t.New)
                                                                                                      ?? (x as FormattableString)?.ToString().Replace(t.Old, t.New)
                                                                                                      ?? x).ToArray());
        return (@return, returnAttribute, @params);
    }
    FormattableString GenFunction(string name, FunctionDoc function, string modifiers = "public static partial", FormattableString? statements = null)
    {
        statements ??= $";";
        var (@return, returnAttribute, @params) = GenFunctionElement(name, function);
        return string.IsNullOrWhiteSpace(modifiers)
            ? Join(returnAttribute, $"{@return} {name}({@params}){statements}")
            : Join(returnAttribute, $"{modifiers} {@return} {name}({@params}){statements}");

    }
    FormattableString GenDelegate(string name, FunctionDoc function)
    {
        (var @return, var returnAttribute, var @params) = GenFunctionElement(name, function);
        return Join(
            $"[UnmanagedFunctionPointer(CallingConvention.Cdecl)]",
            returnAttribute,
            $"public delegate {@return} {name}({@params.ToString().Replace("string ", "[MarshalAs(UnmanagedType.LPUTF8Str)] string ")});");
    }
    FormattableString GenAttribute(Dictionary<string, string> doc) => doc.Gen((type, description) => type switch
    {
        "MAA_FRAMEWORK_API" => $"""[LibraryImport("MaaFramework", StringMarshalling = StringMarshalling.Utf8)]""",
        "MAA_TOOLKIT_API" => $"""[LibraryImport("MaaToolkit", StringMarshalling = StringMarshalling.Utf8)]""",
        _ => $"",
    });
    FormattableString GenParameters(Dictionary<string, ParameterDoc> doc) => doc.Gen(separator: ", ", f: (paramName, param) =>
    {
        var type = _types.TryGetValue(param.Type, out var t) ? t
            : _customHandleTypedefs.SingleOrDefault(x => param.Type.Contains(x.Key)).Value ?? param.Type;

        if (type == param.Type && !_types.ContainsKey(type))
            UnreplacedTypes.Add(type);

        var modifier = (param.IsPointerToArray is true, param.Direction) switch
        {
            // https://learn.microsoft.com/zh-cn/dotnet/standard/native-interop/best-practices
            // nmmd 又说务必对数组参数使用 [In] 和 [Out] 属性，又在这报错
            // 这就是我们的 M$ 啊，真是 MM 又 $$ 啊，你们有没有这样的 M$ 啊
            // error SYSLIB1051: 此参数上提供的 “[In]” 和 “[Out]” 属性在此参数上不受支持。
            // error SYSLIB1051: 不支持 “[In]” 属性，除非同时使用 “[Out]” 属性。没有 “[Out]” 属性的情况下，“[In]” 属性的行为与默认行为相同。

            // (true, ParameterDirection.InputOutput) => "[In, Out]",
            // (true, ParameterDirection.Output) => "[Out]",
            // (true, ParameterDirection.Input) => "[In]",
            (false, ParameterDirection.InputOutput) => "ref",
            (false, ParameterDirection.Output) => "out",
            (false, ParameterDirection.Input) => "in",
            _ => string.Empty,
        };
        type = (type.Contains("{0}"), !string.IsNullOrEmpty(modifier), type.Contains("{1}"), param.IsPointerToArray) switch
        {
            // {0} modifier {1} array
            (false, false, false, _) => type,
            (true, true, false, _) => string.Format(type, modifier),
            (true, true, true, false) => string.Format(type, modifier, string.Empty),
            (true, false, true, true) => string.Format(type, string.Empty, "[]"),
            _ => throw new InvalidOperationException(),
        };
        return $"{type.Trim()} {paramName}";
    });
    string GenReturn(Dictionary<string, string> doc, out FormattableString returnAttribute)
    {
        var specials = new Dictionary<string, string>
        {
            ["const char*"] = "nint", // 封送会释放掉 Maa 返回的 const char*
            ["char*"] = "nint",
        };

        returnAttribute = $"";
        var @return = doc.Keys.Single(IsReturnType);
        if (specials.TryGetValue(@return, out var type))
            return type;

        if (_types.TryGetValue(@return, out type))
            return type;

        if (_customHandleTypedefs.TryGetValue(@return, out type))
            return type;

        UnreplacedTypes.Add(@return);
        return @return;
    }
    SortedSet<string> UnreplacedTypes = [];
    #endregion

    #region Const
    FormattableString GenDefines(Dictionary<string, DefineDoc> defines) => defines.Gen((name, define) => Join(
        GenDocument(define.Description),
        GenDefine(name, define, "internal")));
    FormattableString GenDefine(string name, DefineDoc define, string modifiers)
    {
        var type = "string"; // MaaMsg
        var value = define.Value;
        if (value.StartsWith('(') && value.EndsWith(')'))
            value = value[1..^1];
        var match = Regex.Matches(value, @"\((.+)\)(.+)");
        if (match.Count == 1)
        {
            type = match[0].Groups[1].Value;
            value = match[0].Groups[2].Value;
        }

        Dictionary<string, (string Type, string Value)> specials = new()
        {
            ["MaaNullSize"] = ("MaaSize", "MaaSize.MaxValue"),
        };
        if (specials.TryGetValue(name, out var t))
        {
            type = t.Type;
            value = t.Value;
        }

        return $"{modifiers} const {type} {name} = {value};";
    }
    #endregion

    #region Enum
    void WriteEnum(ICodegenTextWriter writer, string name, EnumDoc doc)
    {
        writer
            .EnsureEmptyLine().Write(GenDocument(doc.Description))
            .EnsureEmptyLine().Write(doc.IsFlags ? "[Flags]" : string.Empty)
            .EnsureEmptyLine().WithCBlock(
                $"public enum {name} : {doc.UnderlyingType}", () =>
                {
                    writer.WriteLine(GenEnumValues(doc.EnumValues));
                })
            .EnsureEmptyLine().WriteLine();
    }
    FormattableString GenEnumValues(Dictionary<string, DefineDoc> doc) => doc.Gen((name, value) => Join(
        GenDocument(value.Description),
        $"{name} = {value.Value},"));
    #endregion

    #region Struct
    void WriteStruct(ICodegenTextWriter writer, string keyName, StructDoc doc)
    {
        Dictionary<string, string> specials = new()
        {
            ["Stop"] = "Abort",
        };

        var key = _structKeys[keyName];
        var functionPointers = doc.Variables.Where(x => x.Value.FunctionPointer is not null).ToDictionary(x => specials.GetValueOrDefault(x.Key, x.Key), x => x.Value.FunctionPointer!);

        writer
            .WithIndent($"""
                        global using Maa{key}ApiTuple = (
                            System.Runtime.InteropServices.GCHandle Handle,
                            MaaFramework.Binding.Custom.IMaaCustom{key} Managed,
                        """, ");", () =>
                        {
                            writer.WriteLine(GenStructTupleField_Delegate(key, functionPointers));
                        })
            .EnsureEmptyLine()
            .WriteLine()
            .WriteLine("using MaaFramework.Binding.Buffers;")
            .WriteLine("using MaaFramework.Binding.Custom;")
            .WriteLine("using System.Runtime.InteropServices;")
            .WriteLine()
            .WriteLine("namespace MaaFramework.Binding.Interop.Native;")
            .WriteLine()
            .WriteLine(GenDocument(doc.Description))
            .WriteLine($$"""
                        [StructLayout(LayoutKind.Sequential)]
                        public class {{keyName}}
                        {
                            {{from name in functionPointers.Keys select $"public nint {name}FunctionPointer;"}}
                        }
                        """)
            .EnsureEmptyLine()
            .WriteLine()
            .WithCBlock($"""
                        /// <summary>
                        ///     A static class providing extension methods for the converter of <see cref="IMaaCustom{key}"/>.
                        /// </summary>
                        public static class IMaaCustom{key}Extension
                        """, () =>
                        {
                            writer.WriteLine(GenStructExtensionField_Delegate(functionPointers));
                            writer.WithCBlock(
                                $"public static {keyName}Handle Convert(this IMaaCustom{key} task, out Maa{key}ApiTuple tuple)", () =>
                                {
                                    writer.WriteLine(GenStructExtensionConvert_LocalMethod(functionPointers));
                                    writer.WriteLine($$"""
                                        
                                        {{functionPointers.Keys.Select(name => $"{name}Delegate {name}Method = {name}LocalMethod;")}}
                                        
                                        var handle = GCHandle.Alloc(new {{keyName}}()
                                        {
                                            {{functionPointers.Keys.Select(name => $"{name}FunctionPointer = Marshal.GetFunctionPointerForDelegate<{name}Delegate>({name}Method),")}}
                                        }, GCHandleType.Pinned);
                                        
                                        tuple = (
                                            handle,
                                            task,
                                            {{string.Join(",\r\n", functionPointers.Keys.Select(x => x + "Method"))}}
                                        );
                                        return handle.AddrOfPinnedObject();
                                        """);
                                });
                        })
            .WriteLine();
    }
    FormattableString GenStructTupleField_Delegate(string key, Dictionary<string, FunctionDoc> doc) => doc.Gen((name, function) =>
        $"MaaFramework.Binding.Interop.Native.IMaaCustom{key}Extension.{name}Delegate {name}Method",
        separator: ",\r\n");
    FormattableString GenStructExtensionField_Delegate(Dictionary<string, FunctionDoc> doc) => doc.Gen(separator: "\r\n\r\n", f: (name, function) => Join(
        GenDocument(function),
        GenAttribute(function.Types),
        GenDelegate(name + "Delegate", function)));
    FormattableString GenStructExtensionConvert_LocalMethod(Dictionary<string, FunctionDoc> doc) => doc.Gen(separator: "\r\n\r\n", f: (name, function) => Join(
        GenFunction(name + "LocalMethod", function, modifiers: string.Empty, statements: GenStatements(name, function.Types.Keys.Single(), function.Parameters))));
    FormattableString GenStatements(string funcName, string funcType, Dictionary<string, ParameterDoc> doc) => FormattableStringFactory.Create("{0};", GenManagedType(funcType,
        $" => task.{Naming.To.PascalCase(funcName)}(" +
        $"{doc.Gen(separator: ", ", f: (paramName, param) => GenManagedType(param.Type, paramName))}" +
        $")"));
    FormattableString GenManagedType(string unmanagedType, string value)
        => FormattableStringFactory.Create(_unmanagedToManaged.GetValueOrDefault(unmanagedType, "{0}"), value);

    #endregion

    #region Generic
    FormattableString GenComment(DescriptionDoc doc) => Join(
        doc.Brief.Gen(x => $"// {x}"),
        doc.Details.Gen(x => $"// {x}"),
        GenObsoleteAttribute(doc));
    FormattableString GenDocument(DescriptionDoc doc) => Join(
        GenDocumentSummary(doc),
        GenDocumentRemarks(doc),
        GenObsoleteAttribute(doc));
    FormattableString GenDocument(FunctionDoc doc) => Join(
        GenDocumentSummary(doc.Description),
        GenDocumentParam(doc.Parameters),
        GenDocumentReturns(doc.Types),
        GenDocumentRemarks(doc.Description),
        GenObsoleteAttribute(doc.Description));
    FormattableString GenDocumentSummary(DescriptionDoc doc) => doc.Brief.Gen(brief => $"""
        /// <summary>
        ///     {brief}
        /// </summary>
        """);
    FormattableString GenDocumentRemarks(DescriptionDoc doc) => doc.Details.Gen(details => $"""
        /// <remarks>
        ///     {details.Select(detail => $"<para>{detail}</para>")}
        /// </remarks>
        """);
    FormattableString GenDocumentParam(Dictionary<string, ParameterDoc> doc) => doc.Gen((name, param) => param.Description.Gen(description => $"""
        /// <param name="{name}">{description}</param>
        """));
    FormattableString GenDocumentReturns(Dictionary<string, string> doc) => doc.Single(type => IsReturnType(type.Key)).Value.Gen(description => $"""
        /// <returns>{description}</returns>
        """);
    FormattableString GenObsoleteAttribute(DescriptionDoc doc) => doc.Deprecated is null ? $""
        : string.IsNullOrWhiteSpace(doc.Deprecated)
        ? (FormattableString)$"""[Obsolete]"""
        : $"""[Obsolete("{doc.Deprecated}")]""";

    bool IsReturnType(string type)
        => !type.StartsWith("MAA_") && !type.Equals("const");

    #endregion
}
