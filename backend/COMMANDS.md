# web3-001-ef-blockchain

## Commands:

```cmd
dotnet tool install --global dotnet-reportgenerator-globaltool
```

```cmd
cd ./backend/EF.Blockchain

dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport

start coveragereport/index.html

dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport && start coveragereport/index.html
```

```cmd
<!-- install -->
dotnet tool update -g docfx

<!-- start -->
cd ./backend/EF.Blockchain
mkdir docs
cd docs
docfx init

<!-- update -->
cd ./backend/EF.Blockchain
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
