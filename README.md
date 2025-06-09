# Telexistence Robot Control API

This project implements a scalable backend for remote robot control and monitoring, leveraging [Orleans](https://dotnet.github.io/orleans/) for distributed actor-based state, MongoDB for persistence, and SignalR for real-time command streaming.

---

## Features

- **Strongly-typed robot command system** using enums
- **MongoDB** for durable command and state storage
- **Orleans** for scalable, distributed robot state and event processing
- **SignalR** for real-time, low-latency command streaming from clients
- **JWT Authentication** for secure access
- **Swagger** for API documentation and exploration
- **Docker + GitHub Actions** for CI/CD and containerized dev/test
- **Azure Container Apps–ready** deployment
- **Global exception handling** via middleware
- **Unit tests** for core logic
- **`appsettings.Development.json`** for local configuration

---

## Run Locally
- cd into \Telexistence\OrleansAPI>
```bash
docker-compose up --build
```

> **Note:**  
> If you encounter the error:  
> `no matching manifest for windows(10.0.22631)/amd64 in the manifest list entries`  
> you must switch Docker to **Linux Containers**.

Visit [http://localhost:5000/swagger](http://localhost:5000/swagger) for the Swagger UI.

---

## Authentication (JWT Login)

Obtain a JWT by posting credentials:

```http
POST /login
Content-Type: application/json

{
  "username": "admin",
  "password": "password"
}
```

Use the returned token as a **Bearer token** in all subsequent API and SignalR connections.

---

## Real-time Command Streaming (SignalR)

Clients (VR, browser, desktop, Unity, etc.) should use SignalR for low-latency, bi-directional command streaming.

## How the Client Authenticates and Connects to SignalR - Implementation of Streaming client is not included in the project.

1. **Obtain a JWT** from `/login` as described above.
2. **Connect to SignalR**:

    - **JavaScript Example:**
      ```javascript
      const connection = new signalR.HubConnectionBuilder()
        .withUrl("/robotControlHub", {
          accessTokenFactory: () => "<JWT_TOKEN_HERE>"
        })
        .build();
      await connection.start();
      ```

    - **.NET Example:**
      ```csharp
      var connection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/robotControlHub", options =>
        {
            options.AccessTokenProvider = () => Task.FromResult("<JWT_TOKEN_HERE>");
        })
        .Build();
      await connection.StartAsync();
      ```

    - The server will validate the JWT and allow connection if valid.

---

## Folder Naming for SignalR Hubs

Place SignalR hub classes in the `Hubs` folder:

```
/ProjectRoot
    /Controllers
    /Services
    /Hubs
        RobotControlHub.cs
    /Models
    /Grains
    ...
```
This follows ASP.NET Core conventions for clarity and maintainability.

---

## Azure Deployment (Container Apps)

Azure Container Apps are used for production deployment:

- Fully managed, scalable runtime (KEDA)
- Native Docker image  support
- Simple integration with Azure Container Registry

**To deploy:**
1. Create a Container App Environment
2. Push your built Docker image to Azure Container Registry
3. Deploy using:
   ```bash
   az containerapp create ... # with your image and configuration
   ```

---

## CI/CD Pipeline

Automated via GitHub Actions:

- Restores and builds on push to `main`
- Runs unit tests
- (Planned) Auto-deploy to Azure via `az` CLI

---

## Work Breakdown

| Task                | Time Estimate    |
|---------------------|------------------|
| Read docs           | 30 mins          |
| Write tests         | 2 hours          |
| Implement features  | 4+ hours          |
| Refactor            | 1 hour           |
| CI/CD setup         | 1 hour           |
| Orleans research    | 2+ hours         |
| SignalR Stream      | 1-2 hours        |
| Testing/debugging   | 3+ hours         |

---

## Contributing

Pull requests, issues, and feedback are welcome!

---
