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

1. Run the migration for the given module. Each module maps to a specific DbContext — always pass `--context` to avoid the "More than one DbContext was found" error:

| Module    | DbContext               |
|-----------|-------------------------|
| Inventory | InventoryDbContext      |
| Import    | ImportDbContext         |
| Rating    | RatingDbContext         |

```bash
dotnet ef database update \
  --project src/Modules/<Module>/Pricing.<Module>.Infrastructure \
  --startup-project src/Pricing.Api \
  --context <Module>DbContext
```

2. Confirm output ends with `Done.` — if not, report the error to the user.

3. Remind the user: the connection string must include `TrustServerCertificate=True` in dev/CI environments.
