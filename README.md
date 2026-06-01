# FinTrack

FinTrack is a personal finance management system for tracking income, expenses, accounts, categories, installments, recurring transactions, imports and dashboards.

## Current Scope

The project is starting with the backend MVP:

- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Layered architecture
- CRUD: categories, accounts and transactions
- Monthly dashboard summary

## Project Structure

```text
finance-manager/
├── backend/
│   ├── FinTrack.Api/
│   ├── FinTrack.Application/
│   ├── FinTrack.Domain/
│   └── FinTrack.Infrastructure/
├── docs/
├── infra/
│   └── docker-compose.yml
└── README.md
```

## Running Locally

Start PostgreSQL:

```powershell
docker compose -f infra/docker-compose.yml up -d
```

Restore and build the backend:

```powershell
dotnet restore
dotnet build
```

Apply database migrations:

```powershell
dotnet ef database update --project backend/FinTrack.Infrastructure --startup-project backend/FinTrack.Api
```

Run the API:

```powershell
dotnet run --project backend/FinTrack.Api
```

Swagger will be available at the URL printed by the API, usually:

```text
https://localhost:7000/swagger
```

## Category Endpoints

- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `DELETE /api/categories/{id}`

Example request:

```json
{
  "name": "Food",
  "type": "Expense",
  "parentCategoryId": null
}
```

## Account Endpoints

- `GET /api/accounts`
- `GET /api/accounts/{id}`
- `POST /api/accounts`
- `PUT /api/accounts/{id}`
- `DELETE /api/accounts/{id}`

Example request:

```json
{
  "name": "Main Bank Account",
  "type": "BankAccount",
  "initialBalance": 1000.00
}
```

## Transaction Endpoints

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}`

Available filters for `GET /api/transactions`:

- `startDate`
- `endDate`
- `categoryId`
- `accountId`
- `type`
- `isPaid`

Example filtered request:

```text
GET /api/transactions?startDate=2026-06-01&endDate=2026-06-30&type=Expense&isPaid=false
```

Example create request:

```json
{
  "accountId": "00000000-0000-0000-0000-000000000000",
  "categoryId": "00000000-0000-0000-0000-000000000000",
  "description": "Groceries",
  "amount": 150.75,
  "type": "Expense",
  "date": "2026-06-01",
  "dueDate": "2026-06-05",
  "isPaid": false,
  "paymentDate": null
}
```

## Dashboard Endpoints

- `GET /api/dashboard/monthly-summary?year=2026&month=6`

The monthly summary returns:

- `totalIncome`
- `totalExpense`
- `balance`
- `expensesByCategory`
- `incomeByCategory`
- `upcomingPayments`
- `paidExpenses`
- `unpaidExpenses`

## Notes

The API currently uses a fixed demo user id. Authentication with JWT is planned for a future phase.
