# CI/CD — konfiguracja dev/prod dla `.github/workflows/deploy.yml`

Ręczna konfiguracja jednorazowa, do wykonania w Azure + GitHub UI/CLI. Workflow (`deploy.yml`) już istnieje w repo i czeka na te sekrety/environmenty.

Dane subskrypcji (z `az account show`):
- Subscription ID: `8b79ccae-8751-404f-839d-4a3ccdf08dd7`
- Tenant ID: `d1db2c51-9d55-4644-976f-9010fcb4b1bc`

## 1. Resource group dla prod

`rg-pricing-dev` już istnieje. Brakuje prod:

```bash
az group create --name rg-pricing-prod --location westeurope
```

## 2. Service Principals — jeden per środowisko (least privilege)

Osobny SP na dev i prod, każdy ograniczony `--scopes` tylko do swojej resource group — SP od deployu na dev fizycznie nie ma dostępu do prod, nawet gdyby wyciekł.

```bash
# SP dla dev — dostęp tylko do rg-pricing-dev
az ad sp create-for-rbac \
  --name "sp-pricing-dev" \
  --role Contributor \
  --scopes /subscriptions/8b79ccae-8751-404f-839d-4a3ccdf08dd7/resourceGroups/rg-pricing-dev

# SP dla prod — dostęp tylko do rg-pricing-prod
az ad sp create-for-rbac \
  --name "sp-pricing-prod" \
  --role Contributor \
  --scopes /subscriptions/8b79ccae-8751-404f-839d-4a3ccdf08dd7/resourceGroups/rg-pricing-prod
```

Każde polecenie zwróci JSON:

```json
{
  "appId": "...",
  "displayName": "sp-pricing-dev",
  "password": "...",
  "tenant": "d1db2c51-9d55-4644-976f-9010fcb4b1bc"
}
```

**Zapisz oba wyniki od razu** (np. do `infra/.local/`, ten katalog jest w `.gitignore`) — `password` nie da się odzyskać po zamknięciu terminala, tylko wygenerować nowy.

Z tego JSON-a zbuduj wartość sekretu `AZURE_CREDENTIALS` w formacie oczekiwanym przez `azure/login@v2`:

```json
{
  "clientId": "<appId>",
  "clientSecret": "<password>",
  "subscriptionId": "8b79ccae-8751-404f-839d-4a3ccdf08dd7",
  "tenantId": "d1db2c51-9d55-4644-976f-9010fcb4b1bc"
}
```

(Dwa takie bloki — jeden dla dev SP, jeden dla prod SP.)

## 3. GitHub Environments

Settings repo → **Environments** → **New environment**.

### `dev`
- Utwórz środowisko `dev`.
- Bez protection rules (ma deployować bez zatrzymania).
- **Environment secrets**:
  - `AZURE_CREDENTIALS` → JSON z SP dev (krok 2)
  - `SQL_ADMIN_PASSWORD` → hasło SQL admina dla `rg-pricing-dev` (to samo co już ustawione przy ręcznym deployu, albo nowe)

### `prod`
- Utwórz środowisko `prod`.
- **Deployment protection rules** → zaznacz **Required reviewers** → dodaj siebie (lub inne konto z dostępem do repo).
- Opcjonalnie: **Wait timer** (dodatkowe opóźnienie po akceptacji, zwykle zbędne dla jednoosobowego repo).
- **Environment secrets**:
  - `AZURE_CREDENTIALS` → JSON z SP prod (krok 2) — **inny SP niż dev**
  - `SQL_ADMIN_PASSWORD` → osobne, nowe hasło dla `rg-pricing-prod` (nie reużywaj hasła z dev)

Uwaga: ta sama nazwa sekretu (`AZURE_CREDENTIALS`, `SQL_ADMIN_PASSWORD`) istnieje niezależnie w dwóch środowiskach — job w workflow odwołuje się do `secrets.AZURE_CREDENTIALS`, a GitHub podstawia właściwą wartość w zależności od tego, w jakim `environment:` dany job aktualnie jest.

## 4. Test

```bash
gh workflow run deploy.yml
```

albo push do `main` ze zmianą w `infra/**`. Oczekiwany przebieg:

1. `deploy-dev` — leci od razu, kończy się `rg-pricing-dev` zaktualizowaną.
2. `deploy-prod` — wisi w stanie *Waiting* (widoczne w Actions → run → "Review deployments"). Kliknij **Approve and deploy**.
3. Po akceptacji job odpala się i deployuje do `rg-pricing-prod`.

## 5. Sprzątanie po ćwiczeniu (opcjonalnie)

Jeśli chcesz ograniczyć koszty/ekspozycję po zakończeniu ćwiczenia:

```bash
az ad sp delete --id <appId-dev>
az ad sp delete --id <appId-prod>
az group delete --name rg-pricing-prod --yes --no-wait
```

(`rg-pricing-dev` zostaw, jeśli dalej z niego korzystasz.)
