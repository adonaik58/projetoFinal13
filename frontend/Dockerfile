# syntax=docker/dockerfile:1.7

# ---- Build: install exact dependencies and produce the static bundle ----
FROM node:24-alpine AS build
WORKDIR /app
ENV CI=true
COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci --no-audit --no-fund
COPY . .
RUN npm run build

# ---- Runtime: unprivileged Nginx serving the SPA on :8080 ----
FROM nginxinc/nginx-unprivileged:1.28-alpine AS runtime
LABEL org.opencontainers.image.title="xyami-park-web" \
      org.opencontainers.image.description="Xyami.Park SPA (React + TypeScript) served by Nginx"
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
USER nginx
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget -qO- http://127.0.0.1:8080/healthz || exit 1
