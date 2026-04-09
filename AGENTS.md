# AGENTS — Using AI agents with this repository

Purpose

- Provide concise, actionable guidance so AI agents (and humans using them) can work effectively in this repository.
- Describe repo structure, common workflows, build/test commands, conventions, and examples of high-quality prompts that lead to safe, reliable edits.

Principles for agents

- Be conservative: make minimal, focused changes that address the explicit request. Avoid wide-sweeping edits.
- Prefer the root cause over surface fixes. If unsure, ask a short clarifying question instead of guessing.
- Keep commits small and testable. Run the project's build and unit tests locally after changes when possible.
- Do not modify files outside the requested scope unless a change is necessary and justified.
- Preserve existing code style and API compatibility unless the user explicitly asks for refactor or breaking change.

Prerequisites (developer environment)
- .NET SDK (compatible with repo's target; commonly .NET 7/8 — check individual project files).
- Node.js + npm (used for `dotauth-admin` Angular app). The repo currently builds with the local Node version.
- Angular CLI (for local `ng serve` / `ng build` tasks). Install globally if you run the admin UI.

Quick repo overview
- Root: mono-repo with multiple .NET projects and an Angular admin UI.
- dotauth-admin/: Angular admin UI (frontend). Key files:
  - [dotauth-admin/src/app/resource-search.component.ts](dotauth-admin/src/app/resource-search.component.ts#L1) — example component to edit.
  - [dotauth-admin/angular.json](dotauth-admin/angular.json#L1)
- src/: many C# projects (server, clients, stores, UMA, SMS, etc.). Typical patterns:
  - `dotauth.authserver`, `dotauth.authserverpg`, `dotauth.authserverpgredis` — auth server projects.
  - `dotauth.shared` and `dotauth.*.models` — shared DTOs and domain models.
- tests/: unit and acceptance tests for the server and stores.

Build & test commands
- Frontend (admin UI):
  - Install deps: `cd dotauth-admin && npm install`
  - Serve (dev): `cd dotauth-admin && ng serve` (or use root `ng serve` if configured)
  - Build: `cd dotauth-admin && ng build --configuration=development`
- Backend / solution:
  - Restore and build: `dotnet restore && dotnet build` (run from repo root or workspace solution)
  - Run tests: `dotnet test` (or run specific test projects under `tests/`)

How to ask an agent to make changes (prompt template)
- Use this structure when calling an agent:
  1. Goal: one-sentence summary of what you want changed.
  2. Context: files, modules, or features affected (provide file paths). Mention relevant tests if any.
  3. Constraints: compatibility, performance, security, or style rules.
  4. Acceptance criteria: how to verify the change (build command and tests to run).
  5. Optional: small examples of input/output if behavior change is involved.

Example prompt for a bug fix (good):
- Goal: "Fix search parsing so the search box splits on spaces and commas and ignores empty values."
- Context: `dotauth-admin/src/app/resource-search.component.ts` (component template + `search()` method).
- Constraints: Do not change API calls; respect existing TypeScript/Angular patterns; add unit tests if feasible.
- Acceptance: Build succeeds and `ng build --configuration=development` runs without TypeScript/compile errors. Manual test: run UI and try `"foo, bar baz"`.

Example prompt for a feature (good):
- Goal: "Show greyed attribute labels for missing fields in resource cards, and apply theme primary color to primary buttons."
- Context: `dotauth-admin/src/app/resource-search.component.ts` and global CSS variables.
- Constraints: No breaking changes to backend; use CSS variable `--primary-color` fallback.
- Acceptance: Build succeeds; visual review confirms the styling.

Testing & verification guidance for agents
- After edits, run the relevant build commands: frontend `ng build` or root `dotnet build`.
- Run unit tests that cover changed code if available: `dotnet test` or test runner configured for Angular.
- If tests fail, provide failing test output and propose fixes rather than making large speculative changes.

Formatting and style
- Follow existing spacing, naming, and file structure conventions in the repo.
- For TypeScript/Angular edits:
  - Use single-file component modifications when possible.
  - Close template and decorator strings properly — malformed template strings cause compiler errors.
- For C# edits:
  - Keep public API signatures stable unless explicitly requested to change them.

What agents should not do
- Do not run or modify CI/CD workflows without user consent.
- Do not add or modify secret or credential files.
- Avoid large-scale automatic refactors; ask for permission for big changes.

If you want me to act as an agent now
- Provide a clear prompt following the template above and list exact files to edit. Example: "Update `resource-search.component.ts` to split terms on spaces and commas, remove empty strings, and update placeholder text. Run `ng build` and report errors." 

Notes for human reviewers
- Always inspect diffs before committing edits suggested by an agent.
- Prefer small commits with clear messages so rollbacks are simple.

Contact / Troubleshooting
- If builds fail after an agent edit, capture the first compiler error and the surrounding lines and paste them into the next request; that speeds debugging.

---
Generated on 2026-03-11. This file is intended to be a living document; please update with any repo-specific conventions you want agents to follow.
