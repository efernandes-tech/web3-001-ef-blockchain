# .NET - Commands:

```cmd
dotnet tool install --global dotnet-reportgenerator-globaltool
```

```cmd
cd ./dotnet-version/EF.Blockchain

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport

start coveragereport/index.html
```
