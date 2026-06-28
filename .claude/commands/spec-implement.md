# Command: spec-implement
Implement a feature from an existing specification, layer by layer.

## Usage
```
/spec-implement SPEC-NNN
```

## Instructions

Read `_specs/active/SPEC-NNN.md` before doing anything else.

Update spec status to `in-progress` in the file and in `_specs/INDEX.md`.

Implement in this order — do not start the next layer until the current one compiles and all tests pass:

1. **Domain** (skip if spec says "None" under Domain Changes)
2. **Contracts** — create Request/Response/Dto records first; Application depends on them
3. **Application** — use cases; defines interfaces that Infrastructure will implement
4. **Infrastructure** (run `/migrate <Module>` after EF changes)
5. **Api**

> **Why Application before Infrastructure?**
> Infrastructure implements interfaces defined in Domain/Application (IRepository, IUnitOfWork) and registers Application services. Application must compile first.

### After completing each layer

1. Run `dotnet build Pricing.slnx` — fix any compiler errors before continuing.
2. Run `dotnet test Pricing.slnx` — fix any failing tests before continuing.
   - If a failing test is pre-existing (unrelated to this spec), note it explicitly and confirm with the user before moving on.
3. Check off the relevant items in `## Implementation Checklist` in the spec file.
4. Briefly confirm to the user what was done and what comes next.

---

### ★ After Domain layer is confirmed (compiles + tests pass)

Spawn sub-agent **`domain-test-writer`** in parallel while you begin the Contracts layer.

Pass to the sub-agent:
- Paths of all Domain files created or modified in this spec
- Module name (e.g. `Inventory`)
- Path to the unit test directory: `tests/Pricing.<Module>.Domain.UnitTests/`

Do **not** wait for the sub-agent to finish before continuing to Contracts and Application.

When the sub-agent returns:
1. Review its summary (files created, test count).
2. Run `dotnet build Pricing.slnx` + `dotnet test Pricing.slnx` to include the new tests.
3. Fix any compilation errors in the generated tests before proceeding.
4. Check off `[ ] Unit tests — Domain` in the spec checklist.

---

### ★ After Application layer is confirmed (compiles + tests pass)

Spawn sub-agent **`application-test-writer`** in parallel while you begin the Infrastructure layer.

Pass to the sub-agent:
- Paths of all Application files created or modified in this spec (use cases)
- Module name (e.g. `Inventory`)
- Path to the unit test directory: `tests/Pricing.<Module>.Application.UnitTests/`

Do **not** wait for the sub-agent to finish before continuing to Infrastructure.

When the sub-agent returns:
1. Review its summary (files created, test count).
2. Run `dotnet build Pricing.slnx` + `dotnet test Pricing.slnx` to include the new tests.
3. Fix any compilation errors in the generated tests before proceeding.
4. Check off `[ ] Unit tests — Application` in the spec checklist.

---

### ★ After Infrastructure layer is confirmed (compiles + tests pass)

Spawn sub-agent **`infrastructure-test-writer`** in parallel while you begin the Api layer.

Pass to the sub-agent:
- Paths of all Infrastructure files created or modified in this spec (repositories, EF configurations)
- Module name (e.g. `Inventory`)
- Path to the integration test directory: `tests/Pricing.IntegrationTests/`

Do **not** wait for the sub-agent to finish before continuing to Api.

When the sub-agent returns:
1. Review its summary (files created, test count).
2. Run `dotnet build Pricing.slnx` + `dotnet test Pricing.slnx` to include the new tests.
3. Fix any compilation errors in the generated tests before proceeding.
4. Check off `[ ] Integration tests — Infrastructure` in the spec checklist.

---

### After all layers are complete

1. Run `dotnet test Pricing.slnx` one final time — all tests must pass (or pre-existing failures must be explicitly acknowledged).

2. Spawn sub-agent **`spec-reviewer`**.

   Pass to the sub-agent:
   - Path to the spec file: `_specs/active/SPEC-NNN.md`
   - Root path of the solution

   Wait for the reviewer to return before continuing.

3. Display the reviewer's report to the user.
   - If report says **NEEDS ATTENTION**: stop, show gaps, ask the user how to proceed.
   - If report says **PASS**: continue to close-out steps below.

4. Update spec status to `implemented`.
5. Move file from `_specs/active/` to `_specs/done/`.
6. Update `_specs/INDEX.md`.
7. Confirm: **"SPEC-NNN implemented. All checklist items done."**

---

## Rules
- Follow CLAUDE.md architecture and conventions at all times
- Each module has its own UnitOfWork interface (`I<Module>UnitOfWork`) — never inject `IUnitOfWork` directly
- Modules communicate only through `*.Facade` — never reference another module's `*.Application` or `*.Domain`
- If you hit an Open Question that blocks implementation, stop and ask the user
- Never call `SaveChanges` inside a repository
- Keep Api endpoints thin — delegate everything to the use case
- Sub-agent tests are additive — never delete or modify existing tests to make them pass; fix production code instead
