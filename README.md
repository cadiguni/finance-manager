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
- React frontend with Vite, TypeScript, Tailwind CSS and Recharts
- Frontend forms for accounts, categories and transactions
- CSV, Excel and credit card statement imports with preview, validation and import history
- Automatic categorization by keyword rules during imports

## Project Structure

```text
finance-manager/
├── backend/
│   ├── FinTrack.Api/
│   ├── FinTrack.Application/
│   ├── FinTrack.Domain/
│   └── FinTrack.Infrastructure/
├── docs/
├── frontend/
│   └── fintrack-web/
├── infra/
│   └── docker-compose.yml
└── README.md
```

## Running With Docker

Start the full stack:

```powershell
docker compose -f infra/docker-compose.yml up --build
```

Services:

- Frontend: `http://localhost:3000`
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- PostgreSQL: `localhost:5432`

In `Development`, the API applies EF Core migrations automatically on startup.

Stop the stack:

```powershell
docker compose -f infra/docker-compose.yml down
```

Reset the local Docker database when you want a clean seed:

```powershell
docker compose -f infra/docker-compose.yml down -v
docker compose -f infra/docker-compose.yml up --build
```

## Running Locally For Development

Use three terminals.

Terminal 1: start PostgreSQL:

```powershell
docker compose -f infra/docker-compose.yml up -d postgres
```

Restore and build the backend:

```powershell
dotnet restore
dotnet build
```

Apply database migrations when running the API outside Docker. Install `dotnet-ef` first if needed with `dotnet tool install --global dotnet-ef`.

```powershell
dotnet ef database update --project backend/FinTrack.Infrastructure --startup-project backend/FinTrack.Api
```

Terminal 2: run the API:

```powershell
dotnet run --project backend/FinTrack.Api
```

Swagger will be available at the URL printed by the API, usually:

```text
http://localhost:5244/swagger
```

Terminal 3: run the frontend:

```powershell
cd frontend/fintrack-web
npm install
npm.cmd run dev
```

The frontend expects the API at:

```text
http://localhost:5244
```

To override it, create `frontend/fintrack-web/.env` using `.env.example`:

```text
VITE_API_BASE_URL=http://localhost:5244
```

On shells that do not block PowerShell scripts, `npm run dev` also works.

The first frontend screen includes:

- monthly dashboard cards and charts;
- transactions table;
- transaction edit, delete and paid/pending actions;
- visual transaction filters by date, type, account, category and status;
- account creation form;
- account listing, edit and delete actions;
- category creation form;
- category listing, edit and delete actions;
- transaction creation form.

The application also includes demo seed data for the development user:

- default accounts;
- default income and expense categories;
- sample June 2026 transactions.

## Tests

Run backend tests:

```powershell
dotnet test
```

Current unit coverage focuses on:

- `AccountService`
- `TransactionService`
- Excel import parsing and commit flow
- credit card statement parsing and commit flow

## CI

The repository includes a GitHub Actions workflow at `.github/workflows/ci.yml`.

It runs:

- `dotnet restore`
- `dotnet build`
- `dotnet test`
- `npm ci`
- `npm run build`
- `docker compose build`

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

## Import Endpoints

- `POST /api/imports/csv/preview`
- `POST /api/imports/csv/commit`
- `POST /api/imports/excel/preview`
- `POST /api/imports/excel/commit`
- `POST /api/imports/card-statement/preview`
- `POST /api/imports/card-statement/commit`
- `GET /api/imports`
- `GET /api/category-keyword-rules`
- `POST /api/category-keyword-rules`
- `PUT /api/category-keyword-rules/{id}`
- `DELETE /api/category-keyword-rules/{id}`

CSV requests send `fileName` and `content`. Excel requests send `fileName`, `contentBase64` and optional `worksheetName`.

Both formats use the same columns:

- `description`
- `amount`
- `type`
- `date`
- `accountId`
- `categoryId`
- `dueDate`
- `isPaid`
- `paymentDate`

Required columns are `description`, `amount`, `type`, `date` and `accountId`. Dates must use `yyyy-MM-dd`.

`categoryId` can be left empty when a matching active keyword rule exists for the transaction description and type. Higher-priority rules are evaluated first.

Credit card statement requests send:

- `fileName`
- `content`
- `accountId`
- `dueDate`
- `isPaid`
- `paymentDate`

The current statement parser expects pasted text extracted from a PDF or copied from a card statement. Each purchase line should follow this shape:

```text
01/06 Mercado Central R$ 120,50
02/06 App Corrida 35.90
2026-06-03 Streaming 29.90
```

Supported purchase dates are `dd/MM`, `dd/MM/yyyy` and `yyyy-MM-dd`. When the line uses `dd/MM`, the year is inferred from `dueDate`. Imported statement rows are created as `Expense` transactions for the selected card account, using the statement due date as `dueDate`. Leave category matching to active keyword rules by registering rules in the category keyword panel before importing.

Example card statement preview request:

```json
{
  "fileName": "fatura-junho.txt",
  "content": "01/06 Mercado Central R$ 120,50\n02/06 App Corrida 35.90",
  "accountId": "00000000-0000-0000-0000-000000000000",
  "dueDate": "2026-06-10",
  "isPaid": false,
  "paymentDate": null
}
```

The app does not read binary `.pdf` files directly yet. For now, extract or copy the statement text and paste it into the `Fatura` import mode in the frontend.

## Notes

The API currently uses a fixed demo user id. Authentication with JWT is planned for a future phase.
