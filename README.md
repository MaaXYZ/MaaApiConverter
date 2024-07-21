# MaaApiConverter

MaaApiConverter 是一个用于助力 Binding 开发的小工具。

脱胎于 MaaFramework.Binding.CSharp 的同名工具，使用 C# 语法编写模版生成 Binding 代码。

## 开发

开发分为 2 部分，具体请看 `src` 和 `gen` 目录：

1. [doc -> model](src)

    在每次[文档部署](https://github.com/MaaXYZ/MaaFramework/actions/workflows/doc.yml)期间，将 doc 中的所需信息转为 [model](src/Models/MaaApiDocument.cs) data，并保存至索引文件 `index.json`。
    
    可通过 [github.io](https://maaxyz.github.io/MaaFramework/index.json) 访问下载。

2. [model -> code](gen)

    使用 C# 语法编写模版，读取索引文件 `index.json` 生成 Binding 代码。
    
    理论上可用于生成各种语言的 Binding 代码，使用的 C# 语法也非常基础。

## 鸣谢

### 开源库

- [CodegenCS](https://github.com/Drizin/CodegenCS)

  C# Toolkit for Code Generation (T4 alternative!)

- [LinqToXsdCore](https://github.com/mamift/LinqToXsdCore)

  LinqToXsd ported to .NET Core (targets .NET Standard 2 for generated code and .NET Core 3.1, .NET 5+ for the code generator CLI tool).