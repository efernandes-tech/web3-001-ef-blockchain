# EF Blockchain

A blockchain application with mining, wallet, and web interface.

## Live Demo

-   **App**: https://ef-blockchain.caprover.edersonfernandes.tec.br
-   **Miner**: https://miner-ef-blockchain.caprover.edersonfernandes.tec.br/miner-logs.txt
-   **API**: https://server-ef-blockchain.caprover.edersonfernandes.tec.br/swagger
-   **Docs**: https://docs-ef-blockchain.caprover.edersonfernandes.tec.br

## Features

-   Blockchain mining with rewards
-   Wallet management
-   Real-time miner logs
-   Web dashboard
-   Docker deployment

## Tech Stack

**Frontend**: React + TypeScript + Vite + Tailwind CSS
**Backend**: .NET 8 + ASP.NET Core
**Database**: In-memory blockchain
**Deployment**: Docker + CapRover + nginx

## Quick Start

### Backend

```bash
cd backend/src/EF.Blockchain.Server
dotnet run
```

### Miner/Wallet Console

```bash
cd backend/src/EF.Blockchain.Client
dotnet run
```

### Frontend

```bash
cd frontend/site
npm install
npm run dev
```

## Project Structure

```
├── backend/
│   └── src/EF.Blockchain.Server/   # .NET blockchain + API
├── frontend/
│   ├── site/                       # React web app
│   └── docs/                       # DocFX documentation
└── README.md
```

## Docker Deployment

Each component has its own Dockerfile for CapRover deployment.

## Author

**Ederson Fernandes**
[Website](https://edersonfernandes.com.br) | [LinkedIn](https://www.linkedin.com/in/efernandes-tech)
