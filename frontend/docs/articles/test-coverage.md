# Test Coverage Report

## Current Coverage Status

<iframe src="../coverage/index.html" width="100%" height="600px" frameborder="0"></iframe>

## Quick Stats

-   **Line Coverage**: 97%
-   **Branch Coverage**: 80%
-   **Method Coverage**: -

## Coverage by Module

| Module               | Line Coverage | Branch Coverage |
| -------------------- | ------------- | --------------- |
| EF.Blockchain.Domain | 98.9%         | 92.4%           |
| EF.Blockchain.Server | 96.1%         | 61.2%           |

## Test Results

[View Detailed Coverage Report](../coverage/index.html)

## Running Tests Locally

```bash
cd ./backend/EF.Blockchain
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:../../frontend/docs/coverage
```
