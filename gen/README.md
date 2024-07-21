## 最佳实践

### 开发环境

即 main 分支

自行编写 `Template.cs` 放于文件夹 `Templates` 下，通过项目 `TemplateDebug` 进行调试。
（可能根据语言分有不同分支，也可能是一个被忽略的文件用于选择不同语言的模版（先咕了））

```
这里应该放个生成 C++ include/ 头文件的最小模版用于演示（先咕了）
```

### 生产环境

即 v1 分支，具体如何整合到 Binding 仓库，并一键生成代码，请参考 [MaaFramework.Binding.CSharp](https://github.com/MaaXYZ/MaaFramework.Binding.CSharp/tree/main/tools/MaaApiConverter)。

## 扩展工具

### GenExtension

[MaaApiDocument](../src/Models/MaaApiDocument.cs) 中存在这三种类型，分别是 `string?` `Dictionary<string, object>` `List<object>`。

这三种类型均有扩展方法 `Gen()` 用于简化生成。

```
这里应该放个例子用于演示（先咕了）
```

### NamingConverter

用于将 MaaFramework 中的命名转为 `PascalCase` `camelCase` `Snake_Case_Upper` `snake_case_lower`。
