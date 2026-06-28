---
name: pr
description: >
  Use when the user wants to commit changes, push to remote, and create a Pull Request on GitHub.
  Triggers on: "zrób PR", "commit i PR", "push i PR", "utwórz pull request", "commit changes",
  "create PR", "scommituj", "wrzuć na gita". Also invoked by the /pr slash command.
---

# Git Commit + Push + PR Workflow

## Goal
Automatically prepare a commit message following Conventional Commits, push to the remote branch, and open a PR on GitHub with a title and short list of changes.

## Step 1 — Analyse changes

Before writing the commit message, run:

```bash
git diff --staged --stat
git diff --stat
git status
```

If nothing is staged, run `git add -A` and inform the user what was added.

## Step 2 — Conventional Commit message

Format: `<type>(<scope>): <description in English, imperative>`

**Types:**
- `feat` — new feature
- `fix` — bug fix
- `refactor` — refactoring without behaviour change
- `test` — adding/changing tests
- `chore` — config, dependency, or CI changes
- `docs` — documentation

**Scope** — feature or layer name, e.g.: `Domain`, `Application`, `Infrastructure`, `Contracts`, `Api`, `Web`, `IntegrationTests`

**Examples:**
```
feat(Api): add create example endpoint
fix(Domain): enforce invariant on aggregate creation
refactor(Application): extract use case to separate class
test(IntegrationTests): add API integration tests with Testcontainers
chore(Infrastructure): add EF migration for new entity
```

**Rules:**
- Description in English, imperative mood (add, fix, extract — not added, fixed)
- Max 72 characters on the first line
- If there is more than one logical unit of change — propose splitting into multiple commits

## Step 3 — Push

Check the current branch:
```bash
git branch --show-current
```

If the branch is `main` or `master` — **stop and ask the user** whether they really want to push directly.

Push:
```bash
git push origin <branch> --set-upstream
```

## Step 4 — Create PR via GitHub CLI

```bash
gh pr create \
  --title "<conventional commit title>" \
  --body "$(cat <<'EOF'
## Changes

<list of changes as bullet points — max 5 items, in English>

EOF
)" \
  --base main
```

**PR body format:**
```markdown
## Changes

- <what was added/changed — specific, not generic>
- <next change>
- ...
```

**Body rules:**
- Max 5 bullet points
- Each point starts with a verb (Add, Fix, Extract, Update, Remove)
- No unnecessary context — just what changed

## Step 5 — Confirm

After creating the PR, display:
- PR URL
- Commit title
- Branch name

## Errors

- `gh: command not found` → inform the user that GitHub CLI (`gh`) must be installed: https://cli.github.com
- `not authenticated` → `gh auth login`
- Push conflict → do not force push, inform the user
