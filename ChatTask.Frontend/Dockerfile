# Build stage
FROM node:18-alpine AS builder

WORKDIR /app

# Copy package files
COPY ChatTask.Frontend/package*.json ./

# Install dependencies
RUN npm install

# Copy source code
COPY ChatTask.Frontend/ .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy built assets from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY ChatTask.Frontend/nginx.conf /etc/nginx/nginx.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
