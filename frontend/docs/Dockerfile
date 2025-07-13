# Multi-stage build for DocFX documentation
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder

# Set working directory
WORKDIR /app

# Install DocFX globally
RUN dotnet tool install -g docfx

# Add dotnet tools to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy documentation files
COPY . .

# Build the documentation
RUN docfx docfx.json

# Production stage
FROM nginx:alpine

# Copy built docs from builder stage
COPY --from=builder /app/_site /usr/share/nginx/html

# Create nginx config for CapRover
RUN echo 'server { listen 3000; root /usr/share/nginx/html; index index.html; location / { try_files $uri $uri/ /index.html; } }' > /etc/nginx/conf.d/default.conf

# Expose port 3000 for CapRover
EXPOSE 3000

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
