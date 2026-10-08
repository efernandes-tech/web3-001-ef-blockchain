# Dockerfile for Miner with Web Log Viewer
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY *.sln ./

COPY src/EF.Blockchain.Client/*.csproj ./src/EF.Blockchain.Client/

RUN dotnet restore ./src/EF.Blockchain.Client/EF.Blockchain.Client.csproj

# Copy source code
COPY src/ ./src/

WORKDIR /app/src/EF.Blockchain.Client

# Build the application
RUN dotnet publish -c Release -o /app/out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set working directory
WORKDIR /app

# Copy built app from build stage
COPY --from=build /app/out .

# Install nginx
RUN apt-get update && apt-get install -y --no-install-recommends nginx curl && rm -rf /var/lib/apt/lists/*

# Create simple nginx config
RUN echo 'server { \
    listen 3000; \
    root /app; \
    location /miner-logs.txt { \
    add_header Content-Type text/plain; \
    add_header Cache-Control "no-cache"; \
    add_header Access-Control-Allow-Origin "*"; \
    add_header Access-Control-Allow-Methods "GET, OPTIONS"; \
    add_header Access-Control-Allow-Headers "Content-Type"; \
    if ($request_method = OPTIONS) { \
    return 204; \
    } \
    } \
    location / { \
    return 200 "Miner is running. Check /miner-logs.txt for logs."; \
    add_header Content-Type text/plain; \
    add_header Access-Control-Allow-Origin "*"; \
    } \
    }' > /etc/nginx/sites-available/default

# Create start script
# Exit when either process dies so the orchestrator restarts the container
RUN echo '#!/bin/bash\n\
    trap "kill 0" TERM\n\
    nginx -g "daemon off;" &\n\
    dotnet EF.Blockchain.Client.dll miner &\n\
    wait -n\n\
    exit 1\n\
    ' > /app/start.sh && chmod +x /app/start.sh

# Expose port 3000 for CapRover
EXPOSE 3000

# Let the orchestrator replace the container when nginx stops answering
HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
    CMD curl -fsS http://127.0.0.1:3000/ || exit 1

# Start both services
CMD ["/app/start.sh"]
