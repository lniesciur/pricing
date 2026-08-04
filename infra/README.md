# infra/

Materiał ćwiczeniowy do nauki ARM templates, na podstawie modułu MS Learn:
[Create Azure Resource Manager templates by using Visual Studio Code](https://learn.microsoft.com/en-us/training/modules/create-azure-resource-manager-template-vs-code/3-exercise-create-and-deploy-template?pivots=powershell)

## Pliki

- `tutorial-empty.json` — pusty szkielet 1:1 z kursu, do sanity-checku deploymentu (brak zasobów)
- `azuredeploy.json` — template dopasowany do Pricing: App Service (Linux, `F1` free) + Azure SQL (`GP_S_Gen5` serverless, darmowy limit)
- `azuredeploy.parameters.dev.json` / `azuredeploy.parameters.prod.json` — parametry per środowisko (`environmentName`, resource group osobna dla każdego: `rg-pricing-dev` / `rg-pricing-prod`)

To ćwiczenie, nie production-ready IaC. Przed użyciem do realnego deploymentu Pricing wymaga: większych SKU, Key Vault na sekrety, prawdopodobnie migracji na Bicep.

## CI/CD

`.github/workflows/deploy.yml` deployuje na push do `main` (zmiany w `infra/**`) lub ręcznie (`workflow_dispatch`):

1. `deploy-dev` — środowisko GitHub `dev`, deployuje do `rg-pricing-dev` bez zatrzymania.
2. `deploy-prod` — środowisko GitHub `prod`, uruchamia się dopiero po sukcesie `deploy-dev` i **czeka na ręczną akceptację** (required reviewer skonfigurowany w Settings → Environments → prod).

Sekrety `AZURE_CREDENTIALS` i `SQL_ADMIN_PASSWORD` są zdefiniowane osobno na poziomie każdego środowiska (dev/prod) — ta sama nazwa sekretu, różna wartość, izolacja między środowiskami.

Konfiguracja jednorazowa (Service Principals, GitHub Environments, secrets) → [`CI-CD-SETUP.md`](./CI-CD-SETUP.md).
