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
        ["MaaAgentClient*"] = "MaaAgentClientHandle",
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
    readonly Dictionary<string, string> _marshallers = new()
    {
        ["IMaaCustomController"] = "[MarshalUsing(typeof(MaaMarshaller))] ",
        ["bool"] = "[MarshalAs(UnmanagedType.U1)] ",
    };
    readonly Dictionary<string, string> _types = new()
    {
        ["MaaCustomControllerCallbacksHandle"] = "Custom.IMaaCustomController",

        ["MaaBool*"] = "{0} bool",
        ["MaaNodeId*"] = "{0} MaaNodeId{1}",
        ["MaaOptionValue"] = "byte[]",
        ["MaaRecoId*"] = "{0} MaaRecoId",
        ["MaaSize*"] = "{0} MaaSize",
        ["MaaStatus*"] = "{0} MaaStatus",

        ["const char*"] = "string",
        ["char*"] = "string",
        ["void*"] = "nint",

        ["int32_t"] = "int",
        ["int64_t"] = "long",
        ["uint64_t"] = "ulong",

        ["MaaAdbInputMethod"] = "MaaAdbInputMethod",
        ["MaaAdbScreencapMethod"] = "MaaAdbScreencapMethod",
        ["MaaBool"] = "bool",
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
        ["MaaInferenceDevice"] = "InferenceDevice",
        ["MaaInferenceExecutionProvider"] = "InferenceExecutionProvider",
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
        ["MaaStringBuffer*"] = "new MaaStringBuffer({0})",
        ["MaaImageBuffer*"] = "new MaaImageBuffer({0})",
        ["void*"] = "",
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
            var newCompoundName = compoundName switch
            {
                "MaaAgentClientAPI.h" => "MaaAgentClient",
                "MaaAgentServerAPI.h" => "MaaAgentServer",
                "MaaMsg" => "MaaMsg",
                _ when compoundName.EndsWith(".h") => compoundName[..^2],
                _ => throw new NotImplementedException(),
            };

            api.Compounds.Remove(compoundName);
            api.Compounds.Add(newCompoundName, compound);
            if (newCompoundName.Contains("MaaMsg")) continue;

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

                var msgTree = new GenExtension.GenTree<string, DefineDoc>();
                foreach (var (msgName, msg) in compound.Defines)
                {
                    var subNames = msgName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    if (subNames[^1] == "Succeeded")
                    {
                        msgTree[[.. subNames[..^1], "Prefix"]] = new DefineDoc { Description = msg.Description, Value = $"\"{string.Join('.', subNames[1..^1])}\"" };
                    }
                    msgTree[subNames] = msg;
                }

                var writer = context["./src/MaaFramework.Binding/MaaMsg.cs"];
                WriteAutoGenerated(writer);
                writer
                    .WriteLine("﻿namespace MaaFramework.Binding.Notification;")
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
                            (name, doc) => writer.Write(GenDocument(doc.Description))
                                                 .EnsureEmptyLine()
                                                 .WriteLine(GenDefine(name, doc, "public"))
                                                 .WriteLine());

                var notificationHandlerRegistryTree = new GenExtension.GenTree<string, (string Parent, string Detail, string Child)>();
                GenRegistryTree(msgTree, []);
                void GenRegistryTree(GenExtension.GenTree<string, DefineDoc> msgTree, string[] parentList)
                {
                    foreach ((var nodeName, var node) in msgTree)
                    {
                        if (node.Count == 0 && node.Value is not null) // 子节点为叶子节点
                            if (nodeName == "Prefix")
                                continue;
                            else
                                notificationHandlerRegistryTree[[.. parentList[1..], nodeName]] = (string.Join(".", parentList[1..]), string.Concat([.. parentList[1..], "Detail"]), nodeName);
                        else
                            GenRegistryTree(node, [.. parentList, nodeName]);
                    }
                }

                writer = context["./src/MaaFramework.Binding.Extensions/Notification/NotificationHandlerRegistry.cs"];
                WriteAutoGenerated(writer);
                writer.WithCBlock("""
                    #nullable enable

                    using System.Text.Json;

                    namespace MaaFramework.Binding.Notification;

                    /// <summary>
                    ///     A registry that manages and distributes MaaFramework callback notifications,
                    /// acting as a central processor to receive MaaCallback events and route them to
                    /// appropriate handlers.
                    /// </summary>
                    public sealed class NotificationHandlerRegistry
                    """, writer =>
                    {
                        writer.WithCBlock("public void OnCallback(object? sender, MaaCallbackEventArgs e)", writer =>
                        {
                            writer.WithCBlock("switch (e.Message)", writer =>
                            {
                                notificationHandlerRegistryTree.Gen(
                                    (name, action) => action.Invoke(),
                                    (name, doc) => writer.WriteLine($"""
                                        case MaaMsg.{doc.Parent}.{doc.Child}:
                                            {doc.Parent}.On{doc.Child}(sender, e.Details); return;
                                        """));

                                writer.WriteLine("""        
                                        default:
                                            OnUnknown(sender, e); return;
                                        """);
                            });
                        })
                        .WriteLine()
                        .WriteLine("""
                            public event EventHandler<MaaCallbackEventArgs>? Unknown;
                            internal void OnUnknown(object? sender, MaaCallbackEventArgs details) => Unknown?.Invoke(sender, details);
                            """)
                        .WriteLine();

                        notificationHandlerRegistryTree.Gen(
                                (name, action)
                            => writer.WriteLine($$"""public {{name}}Registry {{name}} { get; } = new();""")
                                     .WithCBlock($"public sealed class {name}Registry", action)
                                     .EnsureEmptyLine()
                                     .WriteLine(),
                                (name, doc)
                            => writer.WriteLine($"""
                                                public event EventHandler<{doc.Detail}>? {doc.Child};
                                                internal void On{doc.Child}(object? sender, string details) => {doc.Child}?.Invoke(sender, JsonSerializer.Deserialize(details,
                                                        NotificationDetailContext.Default.{doc.Detail}) ?? throw new InvalidCastException());
                                                """)
                                     .EnsureEmptyLine());
                    });
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
                if (location.Contains("Def")
                    && compound.Functions.Count + compound.Defines.Count + globalDelegates.Count == 0)
                    writer.WriteLine("namespace MaaFramework.Binding.Interop.Native;").WriteLine();

                if (compound.Functions.Count + compound.Defines.Count + globalDelegates.Count > 0)
                {
                    writer
                        .WriteLine("using System.Runtime.InteropServices;")
                        .WriteLine("using System.Runtime.InteropServices.Marshalling;")
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
                        .Replace("MaaInferenceDevice0", "GPU0")
                        .Replace("MaaInferenceDevice1", "GPU1")
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

    #region GlobalUsing
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

    #region Function Delegate
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
        //Dictionary<string, string> custom

        var @return = GenReturn(function.Types, out var returnAttribute);
        var @params = GenParameters(function.Parameters);
        if (specials.TryGetValue(name, out var t))
            @params = FormattableStringFactory.Create(@params.Format, @params.GetArguments().Select(x => (x as string)?.ToString().Replace(t.Old, t.New)
                                                                                                      ?? (x as FormattableString)?.ToString().Replace(t.Old, t.New)
                                                                                                      ?? x).ToArray());
        return (@return, returnAttribute, @params);
    }
    FormattableString GenFunction(string name, FunctionDoc function, string modifiers = "public static partial", FormattableString? statements = null, string? ret = null)
    {
        statements ??= $";";
        var (@return, returnAttribute, @params) = GenFunctionElement(name, function);
        @return = ret ?? @return;
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
        "MAA_AGENT_CLIENT_API" => $"""[LibraryImport("MaaAgentClient", StringMarshalling = StringMarshalling.Utf8)]""",
        "MAA_AGENT_SERVER_API" => $"""[LibraryImport("MaaAgentServer", StringMarshalling = StringMarshalling.Utf8)]""",
        "const char*" or "char*" => $"[return: MarshalUsing(typeof(MaaMarshaller))]",
        "MaaBool" => $"[return: MarshalAs(UnmanagedType.U1)]",

        _ => $"",
    });
    FormattableString GenParameters(Dictionary<string, ParameterDoc> doc) => doc.Gen(separator: ", ", f: (paramName, param) =>
    {
        var type = _types.TryGetValue(param.Type, out var t)
            ? t
            : _customHandleTypedefs.SingleOrDefault(x => param.Type.Contains(x.Key)).Value ?? param.Type;
        type = _types.GetValueOrDefault(type, type);
        var marshaller = _marshallers.SingleOrDefault(x => type.Contains(x.Key)).Value ?? string.Empty;
        type = $"{marshaller}{type}";

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
        returnAttribute = $"";
        var @return = doc.Keys.Single(IsReturnType);

        if (_types.TryGetValue(@return, out var type))
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

        var managedType = $"IMaaCustom{key}";
        var unmanagedType = $"{keyName}Handle";

        writer
            .EnsureEmptyLine()
            .WriteLine("using MaaFramework.Binding.Buffers;")
            .WriteLine("using MaaFramework.Binding.Custom;")
            .WriteLine("using System.Collections.Concurrent;")
            .WriteLine("using System.Runtime.InteropServices;")
            .WriteLine("using System.Runtime.InteropServices.Marshalling;")
            .WriteLine()
            .WriteLine("namespace MaaFramework.Binding.Interop.Native;")
            .WriteLine()
            .WithCBlock($$"""
                        /// <summary>
                        ///     Marshaller for <see cref="{{managedType}}"/>.
                        /// </summary>
                        [CustomMarshaller(typeof({{managedType}}), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
                        public static class MaaCustom{{key}}Marshaller
                        """, () =>
                        {
                            writer
                                .EnsureEmptyLine()
                                .WriteLine($$"""
                            private static readonly ConcurrentDictionary<IMaaCustomController, ManagedToUnmanagedIn> s_instances = [];

                            /// <summary>
                            ///     Releases a <see cref="{{managedType}}"/>.
                            /// </summary>
                            /// <param name="managed">The <see cref="{{managedType}}"/>.</param>
                            public static void Free({{managedType}} managed)
                            {
                                if (s_instances.TryGetValue(managed, out var value))
                                {
                                    ManagedToUnmanagedIn.Free(value);
                                }
                            }
                            """)
                                .EnsureEmptyLine()
                                .WriteLine()
                                .WriteLine($$"""
                            /// <summary>
                            ///     Custom marshaller to marshal a managed <see cref="{{managedType}}"/> as an unmanaged nint.
                            /// </summary>
                            public struct ManagedToUnmanagedIn
                            {
                                private {{managedType}} _managed;
                                private Delegates _delegates;
                                private GCHandle _handle;
                            
                                /// <summary>
                                ///     Initializes the marshaller with a managed <see cref="{{managedType}}"/>.
                                /// </summary>
                                /// <param name="managed">The managed <see cref="{{managedType}}"/> with which to initialize the marshaller.</param>
                                public void FromManaged({{managedType}} managed)
                                {
                                    _managed = managed;
                                    _delegates = new Delegates(managed);
                                }
                           
                                /// <summary>
                                ///     Converts the current managed <see cref="{{managedType}}"/> to an unmanaged nint.
                                /// </summary>
                                /// <returns>An unmanaged nint.</returns>
                                public {{unmanagedType}} ToUnmanaged()
                                {
                                    _handle = GCHandle.Alloc(new Unmanaged(_delegates), GCHandleType.Pinned);
                            
                                    var value = s_instances.GetOrAdd(_managed, this);
                                    Interlocked.Increment(ref value._delegates.Times);
                                    if (value._handle != _handle)
                                        _handle.Free();
                            
                                    return value._handle.AddrOfPinnedObject();
                                }
                            
                                /// <summary>
                                ///     Frees any allocated unmanaged memory.
                                /// </summary>
                                public void Free()
                                {
                                    // Free
                                }
                            
                                internal static void Free(ManagedToUnmanagedIn value)
                                {
                                    if (Interlocked.Decrement(ref value._delegates.Times) == 0 && s_instances.TryRemove(value._managed, out _))
                                    {
                                        value._handle.Free();
                                    }
                                }
                            }
                            """)
                                .EnsureEmptyLine()
                                .WriteLine()
                                .WriteLine($$"""
                            private sealed class Delegates(IMaaCustomController managed)
                            {
                                public int Times = 0;
                                {{GenStructExtensionConvert_LocalMethod(functionPointers)}}
                            };
                            """).EnsureEmptyLine()
                                .WriteLine()
                                .WriteLine(GenDocument(doc.Description))
                                .WriteLine($$"""
                            [StructLayout(LayoutKind.Sequential)]
                            private sealed class Unmanaged(Delegates delegates)
                            {
                                {{functionPointers.Keys.Select(name => $"public nint {name} = Marshal.GetFunctionPointerForDelegate(delegates.{name});")}}
                            }
                            """).EnsureEmptyLine()
                                .WriteLine()
                                .WriteLine(GenStructExtensionField_Delegate(functionPointers));
                        })
            .EnsureEmptyLine();
    }
    FormattableString GenStructExtensionField_Delegate(Dictionary<string, FunctionDoc> doc) => doc.Gen(separator: "\r\n\r\n", f: (name, function) => Join(
        GenDocument(function),
        GenAttribute(function.Types),
        GenDelegate(name + "Delegate", function)));
    FormattableString GenStructExtensionConvert_LocalMethod(Dictionary<string, FunctionDoc> doc) => doc.Gen(separator: "\r\n", f: (name, function) => Join(
        GenFunction($"{name} = ", function, modifiers: "public", ret: $"{name}Delegate",
            statements: GenStatements(name, function.Types.Keys.Single(), function.Parameters))));
    FormattableString GenStatements(string funcName, string funcType, Dictionary<string, ParameterDoc> doc) => FormattableStringFactory.Create("{0};", GenManagedType(funcType,
        $" => managed.{Naming.To.PascalCase(funcName)}(" +
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
