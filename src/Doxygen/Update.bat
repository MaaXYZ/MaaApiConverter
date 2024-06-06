dotnet tool restore
dotnet LinqToXsd gen -o ./ -c .config/compound ../MaaFramework/xml/compound.xsd
dotnet LinqToXsd gen -o ./ -c .config/index ../MaaFramework/xml/index.xsd
