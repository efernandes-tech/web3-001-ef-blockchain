# -------- Build stage --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy only necessary files for restore
COPY *.sln ./

COPY src/EF.Blockchain.Server/*.csproj ./src/EF.Blockchain.Server/

# Restore dependencies
RUN dotnet restore ./src/EF.Blockchain.Server/EF.Blockchain.Server.csproj

# Copy source code
COPY src/ ./src/

# Publish the app
WORKDIR /app/src/EF.Blockchain.Server

RUN dotnet publish -c Release -o /app/out

# -------- Runtime stage --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy the build output
COPY --from=build /app/out ./

# Configure port
ENV ASPNETCORE_URLS=http://+:5000

EXPOSE 5000

# Start the app
ENTRYPOINT ["dotnet", "EF.Blockchain.Server.dll"]
