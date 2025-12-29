# Dockerfile - Para producción (multi-stage)

# Stage 1: Build
FROM node:20-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

# Build para producción
RUN npm run build -- --configuration production

# Stage 2: Serve with nginx
FROM nginx:alpine

# Copiar configuración de nginx personalizada
COPY nginx.conf /etc/nginx/nginx.conf

# Copiar assets compilados desde el build
COPY --from=builder /app/dist/quickmeet-frontend /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:80/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
