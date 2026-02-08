# Tasks: Tutorial Example (SQLite CLI)

**Input**: Design documents from `/specs/069-tutorial-sqlite-cli/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/sqlitecli.md

**Tests**: Not explicitly requested in the feature specification. TUI Driver verification is included as a cross-cutting validation phase (not xUnit tests).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Project Scaffolding)

**Purpose**: Create the Tutorial example project, add NuGet dependency, register in the solution file

- [ ] T001 Create project file `examples/Stroke.Examples.Tutorial/Stroke.Examples.Tutorial.csproj` with `net10.0`, `OutputType Exe`, `LangVersion 13`, `Nullable enable`, `ImplicitUsings enable`, `ProjectReference` to `../../src/Stroke/Stroke.csproj`, and `PackageReference` to `Microsoft.Data.Sqlite` v9.* (FR-014)
- [ ] T002 Add `Stroke.Examples.Telnet`, `Stroke.Examples.Ssh`, and `Stroke.Examples.Tutorial` projects to `examples/Stroke.Examples.sln` — Telnet and SSH already exist but were omitted from the solution (Constitution §VII violation); all three MUST be added following the existing pattern (FR-015)
- [ ] T003 Create skeleton `examples/Stroke.Examples.Tutorial/Program.cs` with `namespace Stroke.Examples.Tutorial`, `Main(string[] args)` entry point, conditional routing (single `"sqlite-cli"` example via `string.Equals` with `StringComparison.OrdinalIgnoreCase`, matching the contract), database argument parsing (`args.Length < 2 ? ":memory:" : args[1]`), `ShowUsage()` helper, and error path for unknown examples (FR-013, FR-010, FR-011)

---

## Phase 2: User Story 1 — Interactive SQL REPL Session (Priority: P1) 🎯 MVP

**Goal**: A working REPL that prompts with `"> "`, executes SQL against an in-memory SQLite database, displays results in Python tuple format, provides Tab completion for 124 SQL keywords, and applies SQL syntax highlighting with styled completion menus

**Independent Test**: Launch with `dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli`, type `CREATE TABLE users (id INTEGER, name TEXT)`, `INSERT INTO users VALUES (1, 'alice')`, `INSERT INTO users VALUES (2, 'bob')`, `SELECT * FROM users`, verify output shows `(1, 'alice')` and `(2, 'bob')`. Type `sel` + Tab, verify completion menu appears. Verify SQL keywords are syntax-highlighted.

### Implementation for User Story 1

- [ ] T004 [US1] Create `examples/Stroke.Examples.Tutorial/SqliteCli.cs` with static class `SqliteCli`, static `SqlCompleter` field as `WordCompleter` with exactly 124 SQL keywords (verbatim from spec FR-004) and `ignoreCase: true`
- [ ] T005 [US1] Add static `SqlStyle` field to `SqliteCli.cs` using `Style.FromDict()` with four rules: `completion-menu.completion` → `bg:#008888 #ffffff`, `completion-menu.completion.current` → `bg:#00aaaa #000000`, `scrollbar.background` → `bg:#88aaaa`, `scrollbar.button` → `bg:#222222` (FR-006)
- [ ] T006 [US1] Implement `SqliteCli.Run(string database)` method in `SqliteCli.cs`: open `SqliteConnection` with `Data Source={database}`, create `PromptSession<string>` with `lexer: PygmentsLexer.FromFilename("example.sql")`, `completer: SqlCompleter`, `style: SqlStyle` (FR-001, FR-005, FR-012)
- [ ] T007 [US1] Implement REPL loop body in `SqliteCli.Run()`: call `session.Prompt("> ")`, execute SQL via `SqliteCommand.ExecuteReader()`, format each row as Python tuple string — single-quoted strings, bare integers/floats, `None` for NULL, trailing comma for single-element rows — and print with `Console.WriteLine()`. No explicit transaction needed for FR-016 — Microsoft.Data.Sqlite auto-commits individual statements (see plan.md §4 "Database Connection Scope"). Empty input is passed to SQLite as-is and produces no output (spec edge case) (FR-001, FR-002, FR-003, FR-016)
- [ ] T008 [US1] Verify the project builds with `dotnet build examples/Stroke.Examples.Tutorial` and confirm no compilation errors

**Checkpoint**: At this point, the REPL should accept SQL input, display results in Python tuple format, offer Tab completion for SQL keywords with teal styling, and show syntax-highlighted input. Empty input and non-query statements produce no output.

---

## Phase 3: User Story 2 — Error Recovery and Graceful Exit (Priority: P2)

**Goal**: The REPL handles invalid SQL gracefully (prints error in `TypeName('message')` format), recovers from Ctrl-C (discards input, shows new prompt), and exits cleanly on Ctrl-D (prints `"GoodBye!"`)

**Independent Test**: Type `INVALID SQL` and press Enter — verify `SqliteException('near "INVALID": syntax error')` appears and REPL continues. Press Ctrl-C mid-input — verify fresh prompt appears. Press Ctrl-D — verify `GoodBye!` is printed and application exits.

### Implementation for User Story 2

- [ ] T009 [US2] Add `KeyboardInterruptException` handler to the REPL loop in `SqliteCli.cs` — `catch (KeyboardInterruptException) { continue; }` to discard current input and show a new prompt (FR-007)
- [ ] T010 [US2] Add `EOFException` handler to the REPL loop in `SqliteCli.cs` — `catch (EOFException) { break; }` followed by `Console.WriteLine("GoodBye!")` after the loop (FR-008)
- [ ] T011 [US2] Add SQL error handler to the SQL execution block in `SqliteCli.cs` — `catch (Exception e) { Console.WriteLine($"{e.GetType().Name}('{e.Message}')"); }` to display errors in Python `repr(e)` format (FR-009)

**Checkpoint**: At this point, the REPL is fully robust — invalid SQL, Ctrl-C, and Ctrl-D all behave correctly without crashing.

---

## Phase 4: User Story 3 — File-Based Database Support (Priority: P3)

**Goal**: The REPL supports an optional file path argument to work with a persistent SQLite database, defaulting to in-memory when no argument is provided

**Independent Test**: Run with `dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli test.db`, create a table and insert data, exit with Ctrl-D, relaunch with the same path, verify data persists with a SELECT query.

### Implementation for User Story 3

- [ ] T012 [US3] Smoke-test file-based database support: run `dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli test.db`, execute `CREATE TABLE t(x)`, `INSERT INTO t VALUES(1)`, exit with Ctrl-D, relaunch with same path, run `SELECT * FROM t`, confirm `(1,)` output. Arg parsing (T003) and connection string + `using` disposal (T006) already implement FR-010, FR-011, FR-017 — this task validates the integration end-to-end

**Checkpoint**: At this point, all three user stories are complete — the REPL works with both in-memory and file-based databases.

---

## Phase 5: Polish & Cross-Cutting Verification

**Purpose**: End-to-end validation of all user stories via TUI Driver, confirming 128/128 example coverage

- [ ] T013 Verify with TUI Driver: launch `dotnet run --project examples/Stroke.Examples.Tutorial -- sqlite-cli`, execute a full CRUD cycle (`CREATE TABLE`, `INSERT`, `SELECT`, `UPDATE`, `DELETE`), confirm Python-style tuple output, test Tab completion appears for partial SQL keywords, verify syntax highlighting renders, test Ctrl-C recovery, test Ctrl-D exits with `"GoodBye!"`, test invalid SQL shows `TypeName('message')` error format (SC-001 through SC-008)
- [ ] T014 Verify with TUI Driver: launch with file-based database argument, create data, exit, relaunch, confirm data persistence (US3 verification)
- [ ] T015 Run `dotnet build examples/Stroke.Examples.sln` to verify all 9 example projects (Prompts, Choices, Dialogs, FullScreen, PrintText, ProgressBar, Telnet, Ssh, Tutorial) plus Stroke library build together without errors

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **User Story 1 (Phase 2)**: Depends on Setup (T001–T003) — provides the project structure and entry point
- **User Story 2 (Phase 3)**: Depends on US1 (T004–T008) — error handling wraps the existing REPL loop
- **User Story 3 (Phase 4)**: Depends on US1 (T004–T008) — file-based support uses the same `Run()` method
- **Polish (Phase 5)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Setup (Phase 1) — no dependencies on other stories
- **User Story 2 (P2)**: Can start after US1 core REPL exists — adds exception handlers around existing loop
- **User Story 3 (P3)**: Can start after US1 core REPL exists — verifies argument passing already implemented in Setup

### Within Each User Story

- T004 → T005 → T006 → T007 → T008 (sequential within `SqliteCli.cs` — same file)
- T009, T010, T011 are logically parallel (different catch blocks) but same file, so sequential

### Parallel Opportunities

- T001 and T003 touch different files (`*.csproj` vs `Program.cs`) — can run in parallel after T002
- T013 and T014 are independent TUI Driver verification sessions — can run in parallel
- US2 (T009–T011) and US3 (T012) are independent stories — could be implemented in parallel by different developers, but both modify the same `SqliteCli.cs` file so sequential execution is recommended

---

## Parallel Example: Phase 5 Verification

```bash
# Launch both TUI Driver verifications in parallel:
Task: "TUI Driver CRUD cycle + error handling verification" (T013)
Task: "TUI Driver file-based database verification" (T014)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T003)
2. Complete Phase 2: User Story 1 (T004–T008)
3. **STOP and VALIDATE**: Build and run the example — verify REPL, completion, highlighting, tuple output
4. This delivers a working SQLite REPL tutorial example (MVP)

### Incremental Delivery

1. Setup (T001–T003) → Project scaffolding ready
2. User Story 1 (T004–T008) → Working REPL with completion + highlighting (MVP!)
3. User Story 2 (T009–T011) → Robust error handling + graceful exit
4. User Story 3 (T012) → File-based database support
5. Polish (T013–T015) → TUI Driver verification + solution build check

### Single Developer Strategy

All tasks are in the same 3-file scope (`SqliteCli.cs`, `Program.cs`, `.csproj`), so sequential execution is the natural approach. Total estimated scope: ~120 LOC across 3 files.

---

## Notes

- No xUnit tests required — this is an example project (per plan.md: "xUnit not required for example projects")
- TUI Driver verification in Phase 5 serves as the real-world testing per Constitution VIII
- All 124 SQL keywords must be copied verbatim from spec FR-004 — do not add or remove keywords
- Row output format must exactly match Python's `str(tuple)` with `repr()` on elements
- Error format must be `TypeName('message')` — not full stack traces
- Exit message must be exactly `"GoodBye!"` (capital G, capital B, exclamation mark)
- This example completes 128/128 applicable example ports (100% coverage, SC-008)
