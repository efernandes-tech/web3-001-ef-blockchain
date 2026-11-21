# Dockerfile for CapRover deployment
FROM node:18-alpine

# Set working directory
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# API URL baked into the Vite build (passed via --build-arg by scripts/deploy.sh).
# Actual environment variables take precedence over .env files in Vite.
ARG VITE_API_URL=http://localhost:5242
ENV VITE_API_URL=$VITE_API_URL

# Build the app
RUN npm run build

# Install serve to serve static files
RUN npm install -g serve

# Expose port (CapRover uses PORT environment variable)
EXPOSE 3000

# Let the orchestrator replace the container when the app stops answering
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -q --spider http://127.0.0.1:3000/ || exit 1

# Start the app
CMD ["serve", "-s", "dist", "-l", "3000"]
