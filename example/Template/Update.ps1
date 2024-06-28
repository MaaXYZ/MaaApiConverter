dotnet tool restore
rm -r temp
mkdir temp
curl https://moomiji.github.io/MaaFramework/index.json -o temp/index.json
curl https://raw.githubusercontent.com/MaaXYZ/MaaApiConverter/main/src/Models/MaaApiDocument.cs -o temp/MaaApiDocument.cs
