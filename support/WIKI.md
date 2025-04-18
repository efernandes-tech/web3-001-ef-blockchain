# .NET - Commands:

```cmd
dotnet tool install --global dotnet-reportgenerator-globaltool
```

```cmd
cd ./dotnet-version/EF.Blockchain

dotnet test ./tests/EF.Blockchain.UnitTest/EF.Blockchain.UnitTest.csproj --collect:"XPlat Code Coverage"

reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coveragereport

"/c/Program Files/Google/Chrome/Application/chrome.exe" "file:///C:/dev/@web3/web3-001-ef-blockchain/dotnet-version/EF.Blockchain/coveragereport/index.html"
```
