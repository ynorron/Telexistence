# Telexistence Robot Control API

This project implements a scalable backend to control and monitor robots remotely using Orleans and MongoDB.

## Features

- Strongly-typed command system using enums
- MongoDB for persistence
- Orleans for scalable robot state handling
- JWT Authentication
- Swagger support
- Docker + GitHub Actions CI/CD
- Azure Container Apps–ready deployment
- Global exception handling via Middleware
- Unit test implementation
- `appsettings.Development.json` for local config

## Run Locally

```bash
docker-compose up --build
```
- If you encounter this error: 'no matching manifest for windows(10.0.22631)/amd64 in the manifest list entries'
- you have to switch your docker to Linux Containers

Visit `http://localhost:5000/swagger` for the Swagger UI.

## JWT Login

```json
POST /login
{
  "username": "admin",
  "password": "password"
}
```

Use the returned token for authorized endpoints.

## Azure Deployment (Container Apps)

Azure Container Apps were chosen for:
- Fully managed runtime
- Simple scale-out via KEDA
- Built-in support for Docker images and Dapr (future extensibility)

To deploy:
1. Create a Container App Environment
2. Push the built image to Azure Container Registry
3. Deploy using `az containerapp create ...` with your image and settings

##  CI/CD Pipeline

Configured via GitHub Actions:
- Restores and builds on push to `main`
- Runs unit tests
- Future: extend with `az` CLI steps for auto-deployment