# WEX Code Challenge - Card Transaction API

A .NET 9 Web API for managing credit cards and purchase transactions with currency conversion via the Treasury Reporting Rates of Exchange API.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (for PostgreSQL)

## Getting Started

### 1. Start the database

```bash
docker compose up -d
```

### 2. Run the API

```bash
dotnet run --project src/WexChallenge.Api
```

The API will be available at `http://localhost:5118`.

### 3. Run the tests

```bash
dotnet test
```

Tests use an in-memory SQLite database and a fake exchange rate service, so Docker is not needed to run them.

## API Endpoints

### Create a Card

```
POST /api/cards
Content-Type: application/json

{
  "creditLimit": 5000.00
}
```

**Response** (201 Created):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "creditLimit": 5000.00,
  "createdAt": "2024-10-15T10:30:00Z"
}
```

### Store a Purchase Transaction

```
POST /api/cards/{cardId}/transactions
Content-Type: application/json

{
  "description": "Coffee shop",
  "transactionDate": "2024-10-15T00:00:00Z",
  "amount": 4.50
}
```

**Response** (201 Created):
```json
{
  "id": "...",
  "description": "Coffee shop",
  "transactionDate": "2024-10-15T00:00:00Z",
  "amount": 4.50
}
```

### Retrieve a Transaction in a Specified Currency

```
GET /api/transactions/{id}?currency=Canada-Dollar
```

Uses the Treasury Reporting Rates of Exchange API to find an exchange rate on or before the transaction date (within 6 months). Returns an error if no rate is available in that window.

**Response** (200 OK):
```json
{
  "id": "...",
  "description": "Coffee shop",
  "transactionDate": "2024-10-15T00:00:00Z",
  "originalAmount": 4.50,
  "exchangeRate": 1.35,
  "convertedAmount": 6.08,
  "currency": "Canada-Dollar"
}
```

### Retrieve Available Balance

```
GET /api/cards/{cardId}/balance
GET /api/cards/{cardId}/balance?currency=Euro Zone-Euro
```

Returns the card's available balance (credit limit minus total transactions). Optionally converts to a specified currency using the latest available exchange rate.

**Response** (200 OK):
```json
{
  "cardId": "...",
  "creditLimit": 5000.00,
  "totalSpent": 500.00,
  "availableBalance": 4500.00,
  "currency": "Euro Zone-Euro",
  "exchangeRate": 0.92,
  "convertedBalance": 4140.00
}
```

## Architecture

- **ASP.NET Core Minimal API** — lightweight, no controllers needed for this scope
- **Entity Framework Core** with PostgreSQL — production database via Docker
- **Interface-based service design** — `IExchangeRateService` allows swapping the real Treasury API for a fake in tests
- **Integration tests** with `WebApplicationFactory` and SQLite in-memory — fast, isolated, no external dependencies

## Currency Values

The `currency` parameter uses the country-currency names from the Treasury API, for example:
- `Canada-Dollar`
- `Euro Zone-Euro`
- `United Kingdom-Pound`
- `Japan-Yen`
- `Australia-Dollar`

See the full list at: https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange
