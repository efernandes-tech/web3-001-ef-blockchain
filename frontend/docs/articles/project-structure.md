# Project Structure

Understanding the organization and architecture of the EF.Blockchain Web3 project.

## Repository Overview

```
web3-001-ef-blockchain/
├── backend/                # .NET Core API
├── frontend/               # React Web Application
├── README.md
└── .gitignore
```

## Backend Structure

The backend follows Minimal APIs principles:

```
backend/
├── src/
│   ├── EF.Blockchain.Server/       # Web API Layer
│   │   ├── Endpoints/              # API Endpoints
│   │   ├── Middleware/             # Custom middleware
│   │   ├── Program.cs              # Application entry point
│   │   └── appsettings.json        # Configuration
│   ├── EF.Blockchain.Domain/       # Domain Layer
│   │   └── Entities/               # Domain entities
│   └── EF.Blockchain.Client/       # Console apps
│       ├── WalletApp/
│       └── MinerApp/
└── tests/
    ├── UnitTest/                       # Unit tests
    └── IntegrationTest/                # Integration tests
```

### Key Backend Components

**Endpoints**: Handle HTTP requests and responses

-   `BlockEndpoints` - Blockchain operations
-   `WalletEndpoints` - Wallet management
-   `TransactionEndpoints` - Transaction handling
-   `StatusEndpoints` - Status

## Frontend Structure

React application with TypeScript:

```
frontend/
├── public/                 # Static assets
│   └── ef-blockchain.svg
├── src/
│   ├── components/         # Reusable UI components
│   ├── types/              # TypeScript type definitions
│   ├── App.tsx             # Main app component
│   └── main.tsx            # Application entry point
├── package.json
└── tsconfig.json
```

### Frontend Key Features

**UI Framework**: Tailwind CSS for consistent design
**Icons**: Lucide React

## Architecture Patterns

### Backend Architecture

-   **Minimal API**: Separation of concerns
-   **Dependency Injection**: Loose coupling

### Frontend Architecture

-   **Component-Based**: Modular React components

## Configuration Files

### Backend Configuration

-   `appsettings.json` - Application settings
-   `appsettings.Development.json` - Development settings
-   `appsettings.Production.json` - Production settings

### Frontend Configuration

-   `package.json` - Dependencies and scripts
-   `tsconfig.json` - TypeScript configuration
-   `.prettierrc.json` - Code formatting rules

This structure ensures maintainability, scalability, and clear separation of concerns across all layers of the application.
