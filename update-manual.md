# Step to update manual

## .NET

1. Update version in ```lib/dotnet/{Memento.Core|Memento.React}/{Memento.Core|Memento.React}.csproj```

2. Update version in [README.md](./README.md)

3. Add release notes in [Notes](./release-notes.dotnet.md)

Package packing command is following

```
dotnet pack
```

Publish command is following

Memento.Core
```
dotnet nuget push .\lib\dotnet\Memento.Core\bin\Debug\Memento.Core.x.x.x.nupkg -k [APIKEY] -s https://www.nuget.org/
```

Memento.Blazor
```
dotnet nuget push .\lib\dotnet\Memento.Blazor\bin\Debug\Memento.Blazor.x.x.x.nupkg -k [APIKEY] -s https://www.nuget.org/
```

##  Node.js

1. Update version in ```lib/node/{react|core}/package.json```

2. Update version in [README.md](./README.md)

3. Add release notes in [Notes](./release-notes.node.md)
