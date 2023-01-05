# Step to update manual

## .NET

1. Update the version in ```Directory.Build.props```

2. Update the version in [README.md](./README.md)

3. Add the release notes in [Notes](./release-notes.dotnet.md)

Package packing command is following

```
dotnet pack -c Release --include-symbols
```

Publish command is following

```
dotnet nuget push .\lib\dotnet\Memento.{ProjectName}\bin\Release\Memento.{ProjectName}.x.x.x.nupkg -k [APIKEY] -s https://www.nuget.org/
```

##  Node.js

1. Update the version contains in ```lib/node/{react|core}/package.json```

2. Update the version in [README.md](./README.md)

3. Add the release notes in [Notes](./release-notes.node.md)
