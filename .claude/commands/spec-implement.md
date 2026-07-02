# Command: spec-implement

Implement a feature from an existing specification, layer by layer.

## Usage

```
/spec-implement SPEC-NNN
```

---

## Instructions

Read `_specs/active/SPEC-NNN.md` before doing anything else.

Update the specification status to `in-progress` both in:

- the specification file
- `_specs/INDEX.md`

Implement the feature in the following order. Do not start the next layer until the current one builds successfully and all tests pass.

1. **Domain** (skip if the specification says `None` under Domain Changes)
2. **Contracts**
3. **Application**
4. **Infrastructure** (run `/migrate <Module>` after EF changes)
5. **Api**

> **Why Application before Infrastructure?**
>
> Infrastructure implements interfaces defined by Domain/Application. Application must compile first.

---

## After completing each implementation layer

1. Run:

```
dotnet build Pricing.slnx
```

Fix all compilation errors before continuing.

2. Run:

```
dotnet test Pricing.slnx
```

Fix all failing tests before continuing.

If any failing test is unrelated to the current specification:

- report it explicitly
- ask the user before continuing

3. Update the corresponding items in the specification's **Implementation Checklist**.

4. Briefly report:

- what was completed
- what layer comes next

---

# ★ After Domain layer is confirmed (build + tests pass)

Generate Domain unit tests using two dedicated sub-agents.

---

## Step 1 — Spawn `domain-test-planner`

Spawn sub-agent:

```
domain-test-planner
```

Pass:

- Paths of every Domain file created or modified
- Module name (e.g. `Inventory`)
- Output plan path:

```
tests/_plans/<Module>-domain-test-plan.md
```

Wait for the planner to finish.

If the planner fails or does not produce a valid plan:

- stop the workflow
- report the failure to the user
- do NOT spawn `domain-test-writer`

Review the planner summary.

If the planner reports blocking Coverage Gaps or ambiguous business rules:

- stop
- ask the user how to proceed
- do NOT spawn `domain-test-writer`

---

## Step 2 — Spawn `domain-test-writer`

After the planner succeeds, spawn:

```
domain-test-writer
```

Pass:

- Path to the generated test plan
- Module name
- Unit test project path:

```
tests/Pricing.<Module>.Domain.UnitTests/
```

Do **not** pass Domain source files.

The writer must treat the generated test plan as the only source of truth and mechanically implement every test case.

---

## Continue implementation

While the writer generates tests, continue implementing the **Contracts** layer.

Do not wait for the writer before beginning Contracts.

---

## After the writer finishes

Review the writer summary.

Verify:

- generated files
- generated test count
- TODO markers

Confirm:

```
Generated tests = Plan cases − TODO cases
```

If the counts differ:

- stop
- fix generated tests before continuing

Run:

```
dotnet build Pricing.slnx
```

Fix compilation errors:

- fix generated tests first if possible
- if tests expose real production defects, fix production code instead
- never weaken assertions just to make build pass

Run:

```
dotnet test Pricing.slnx
```

If failures are unrelated to this specification:

- report them explicitly
- ask user before continuing

Update checklist:

```
[x] Unit tests — Domain
```

Report:

- Domain completed
- Domain tests completed
- Contracts already in progress

---

# ★ After Application layer is confirmed (compiles + tests pass)

Spawn sub-agent:

```
application-test-writer
```

while beginning Infrastructure.

Pass:

- Paths of modified Application files
- Module name
- Application unit test project path

Do not wait for completion.

When finished:

1. Review generated files.
2. Run:

```
dotnet build Pricing.slnx
dotnet test Pricing.slnx
```

3. Fix issues.
4. Check:

```
[x] Unit tests — Application
```

---

# ★ After Infrastructure layer is confirmed (compiles + tests pass)

Spawn sub-agent:

```
infrastructure-test-writer
```

while beginning Api.

Pass:

- Paths of modified Infrastructure files
- Module name
- Integration test project path

Do not wait.

When finished:

1. Review generated files.
2. Run:

```
dotnet build Pricing.slnx
dotnet test Pricing.slnx
```

3. Fix issues.
4. Check:

```
[x] Integration tests — Infrastructure
```

---

# After all layers are complete

Run final:

```
dotnet test Pricing.slnx
```

All tests must pass unless pre-existing failures were explicitly acknowledged.

---

## Run specification review

Spawn:

```
spec-reviewer
```

Pass:

- Specification path
- Solution root

Wait for completion.

If result:

```
NEEDS ATTENTION
```

- stop
- show issues
- ask user how to proceed

If result:

```
PASS
```

continue

---

## Close specification

1. Mark:

```
implemented
```

2. Move:

```
_specs/active/
→ _specs/done/
```

3. Update `_specs/INDEX.md`

4. Confirm:

> SPEC-NNN implemented. All checklist items completed.

---

## Rules

- Follow `CLAUDE.md` architecture and conventions.
- Each module has its own `I<Module>UnitOfWork` — never inject generic `IUnitOfWork`.
- Modules communicate only via `*.Facade`.
- Never reference another module’s Application or Domain.
- Stop immediately if an Open Question blocks implementation.
- Never call `SaveChanges()` inside repositories.
- API must remain thin.
- Application layer owns business logic.
- Generated tests are additive only.
- Never modify or delete existing tests to make them pass.
- Fix production code instead.