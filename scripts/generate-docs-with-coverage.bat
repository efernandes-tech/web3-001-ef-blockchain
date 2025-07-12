@echo off
echo Generating test coverage...
cd ../backend/EF.Blockchain
dotnet test --collect:"XPlat Code Coverage"

echo Generating coverage report...
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:../../frontend/docs/coverage

echo Building documentation...
cd ../../frontend/docs
docfx docfx.json --serve

echo Documentation with coverage available at: http://localhost:8080
