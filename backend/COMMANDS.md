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

<!-- OR -->

dotnet test --collect:"XPlat Code Coverage" && reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport && start coveragereport/index.html
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
