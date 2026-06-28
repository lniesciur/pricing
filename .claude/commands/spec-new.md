---
description: Create a feature spec file and branch through a guided conversation
argument-hint: Short feature description
allowed-tools: Read, Write, Glob, Bash(git switch:*, git branch:*, git status:*, git log:*, git diff:*)
---

You are a senior developer and solution architect helping to create a precise feature specification.
Always follow rules in any CLAUDE.md files. Do not generate or save any files until Phase E is confirmed.

User input: $ARGUMENTS

---

## Phase A — Git check

Run `git status`. If there are any uncommitted, unstaged, or untracked files — abort immediately.
Tell the user to commit or stash changes before proceeding. DO NOT CONTINUE.

---

## Phase B — Assign spec ID and branch name (internal, do not show yet)

Scan files matching `_specs/SPEC-*.md` and count them. New spec ID = next number, zero-padded to 3 digits.
- No existing specs → `SPEC-001`
- 6 existing specs → `SPEC-007`

From `$ARGUMENTS` derive:
- `feature_title` — short, human readable, Title Case
- `feature_slug` — lowercase, kebab-case, only `a-z 0-9 -`, max 40 chars
- `branch_name` — `feature/<spec_id>-<feature_slug>` (append `-v2` if branch already exists)
- `module` — which module this feature belongs to: `Import`, `Inventory`, or `Rating`

If you cannot infer a sensible title/slug/module, ask the user to clarify before continuing.

---

## Phase C — Domain analysis (no questions, silent analysis)

Read the existing Domain layer code for the relevant module (`src/Modules/<Module>/Pricing.<Module>.Domain/`).
Based on `$ARGUMENTS` and the existing code, determine on your own:

1. Are new aggregates, entities, or value objects needed?
2. Does any existing aggregate need changes?
3. Are there new business rules or invariants?
4. Are domain events needed (state changes worth broadcasting)?
5. Is a domain service needed (logic spanning multiple aggregates)?

Then state one of:
- **"Domain changes required: [brief list]"** — and summarise what needs to change
- **"No domain changes required"**

Do not ask questions. Move immediately to Phase D.

---

## Phase D — Application, Infrastructure, Api, Contracts

Ask about each layer in order. One layer per message. Wait for confirmation before moving to the next.

**D1 — Application**
- What use cases are needed? (create / update / delete / query)
- Are new port interfaces required (`IRepository`, `IService`, etc.)?
- Does this use case need to call another module via its Facade?

**D2 — Infrastructure**
- Which repositories need to be created or modified?
- Are there external services to integrate?
- Will a new EF migration be needed? (reminder: `/migrate <Module>`)

**D3 — Api**
- What endpoints are needed? (HTTP method, route, request/response shape)
- Any special validation rules?

**D4 — Contracts**
- What request / response records are needed in `src/Modules/<Module>/Pricing.<Module>.Contracts/`?
- Are there any nested Dto types (e.g. items in a list)?
- Are any cross-module types needed in `Pricing.Shared.Contracts`?

---

## Phase E — Review

Generate a filled spec draft using `_specs/SPEC-TEMPLATE.md` as the template.

Front matter:
```yaml
---
id: <spec_id>
title: <feature_title>
module: <module>
status: draft
created: <YYYY-MM-DD>
updated: <YYYY-MM-DD>
branch: <branch_name>
related: []
---
```

Show the full draft and ask:
**"Looks good? Type `yes` to save, or describe what to change."**

Wait for explicit `yes` before proceeding.

---

## Phase F — Save, branch, update index

After `yes`:

1. Run `git switch -c <branch_name>`
2. Save spec to `_specs/active/<spec_id>-<feature_slug>.md`
3. Append one row to `_specs/INDEX.md` (sorted by ID, do not modify existing rows):
   `| <spec_id> | <feature_title> | <module> | draft | <YYYY-MM-DD> | <branch_name> |`
4. Confirm:
   **"Spec <spec_id> saved on branch `<branch_name>`. Ready to implement — run `/spec-implement <spec_id>` to start."**

---

## Status lifecycle

| Status | Meaning |
|--------|---------|
| draft | being written |
| review | waiting for team review |
| approved | ready to implement |
| in-progress | implementation started |
| implemented | done, tests passing |
| deprecated | cancelled or superseded |
