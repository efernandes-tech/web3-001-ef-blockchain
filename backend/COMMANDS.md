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
docker-compose up -d
docker-compose up -d --build --force-recreate
```

```cmd
cd ./backend/EF.Blockchain

tar --exclude='*/bin*' \
    --exclude='*/obj*' \
    --exclude='*/.vs*' \
    --exclude='*/tests*' \
    --exclude='*/docs*' \
    --exclude='*/coveragereport*' \
    -cvf build-server-ef-blockchain.tar .

caprover deploy \
  --caproverUrl https://caprover.edersonfernandes.tec.br \
  --appName server-ef-blockchain \
  --tarFile ./build-server-ef-blockchain.tar
```

```cmd

```

```cmd

```

```cmd

```

```cmd

```
