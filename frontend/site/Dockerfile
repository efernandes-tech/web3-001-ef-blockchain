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

# Build the app (Vite will automatically use .env.production)
RUN npm run build

# Install serve to serve static files
RUN npm install -g serve

# Expose port (CapRover uses PORT environment variable)
EXPOSE 3000

# Start the app
CMD ["serve", "-s", "dist", "-l", "3000"]
