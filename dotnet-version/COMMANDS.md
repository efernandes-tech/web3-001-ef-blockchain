# web3-001-ef-blockchain

## Commands:

```cmd
dotnet tool install --global dotnet-reportgenerator-globaltool
```

```cmd
cd ./dotnet-version/EF.Blockchain

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport

start coveragereport/index.html

dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport && start coveragereport/index.html
```

```cmd
dotnet tool update -g docfx

mkdir docs
cd docs
docfx init
cd ..
docfx docs/docfx.json
docfx serve docs/_site
```

```cmd

```

```cmd

```

```cmd

```

```cmd

```

```cmd

```

```cmd

```
