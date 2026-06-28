# Command: migrate

Apply pending EF Core migrations for a specific module.

## Usage

```
/migrate <Module>
```

Example: `/migrate Inventory`, `/migrate Import`, `/migrate Rating`

## How it works

Each module has its own `*.Infrastructure` project with a separate `DbContext` and schema.
Migrations always use `--startup-project src/Pricing.Api` to access DI and the connection string.

The connection string is read from:

1. `dotnet user-secrets` on the Api project (local development)
2. `ConnectionStrings__DefaultConnection` environment variable (CI / production)

## Steps

1. Run the migration for the given module:

```bash
dotnet ef database update \
  --project src/Modules/<Module>/Pricing.<Module>.Infrastructure \
  --startup-project src/Pricing.Api
```

2. Confirm output ends with `Done.` — if not, report the error to the user.

3. Remind the user: migrations require a **direct connection on port 5432**.
   Port 6543 (Supabase transaction pooler) breaks DDL statements.
