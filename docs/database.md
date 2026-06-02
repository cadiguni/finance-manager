# FinTrack Database

The initial database target is PostgreSQL through Entity Framework Core.

Main entities:

- `User`
- `Account`
- `Category`
- `Transaction`
- `InstallmentGroup`
- `RecurringRule`
- `ImportBatch`

Enums are stored as strings in the database to keep records readable and migration diffs easier to understand.

The MVP currently exposes category, account, transaction and monthly dashboard endpoints.

The monthly dashboard is computed from transactions in the selected month:

- incomes increase the balance;
- expenses decrease the balance;
- paid and unpaid expenses are summarized separately;
- category summaries group transactions by category.

Development seed data is inserted through EF Core migrations for the fixed demo user. It includes default accounts, categories and sample June 2026 transactions so a fresh environment has data for the dashboard.
