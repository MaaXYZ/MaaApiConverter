cat Template.cs > temp/MaaApiConvert.cs
cat GenExtension.cs > temp/MaaApiConvert.cs
cat temp/MaaApiDocument.cs >> temp/MaaApiConvert.cs
dotnet dotnet-codegencs template run temp/MaaApiConvert.cs temp/index.json

echo
