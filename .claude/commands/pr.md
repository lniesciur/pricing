# /pr — Commit, Push & Pull Request

Run the full workflow: analyse repository changes, create a commit following Conventional Commits, push to the current branch, and open a Pull Request on GitHub.

## Steps

1. Run the `pr` skill
2. Execute all 5 steps of the skill in order
3. Display the URL of the created PR at the end

## Optional arguments

You can pass additional context after the command:

- `/pr fix login bug` — use as a hint for the commit type and description
- `/pr feat pricing sync` — scope and type will be set accordingly

If no arguments are provided — analyse the changes independently and propose a commit message before executing.