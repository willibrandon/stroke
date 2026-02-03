# Feature Specification: Get Input Example (First Example)

**Feature Branch**: `122-get-input-example`
**Created**: 2026-02-03
**Status**: Draft
**Input**: User description: "Implement the first and simplest Stroke example: GetInput.cs. This establishes the examples infrastructure and demonstrates the most basic prompt usage."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Run the Simplest Prompt Example (Priority: P1)

As a developer learning Stroke, I want to run a simple "hello world" example that prompts me for input and echoes it back, so that I can verify the library works and understand basic prompt usage.

**Why this priority**: This is the canonical first-contact experience for every new user. It validates the entire Stroke stack works end-to-end (input, output, rendering, application lifecycle) with minimal code.

**Independent Test**: Can be fully tested by running `dotnet run --project examples/Stroke.Examples.Prompts`, typing any text, pressing Enter, and observing the echoed output.

**Acceptance Scenarios**:

1. **Given** the example project is built, **When** I run `dotnet run --project examples/Stroke.Examples.Prompts`, **Then** I see the prompt "Give me some input: " displayed (exact text, including trailing space)
2. **Given** the prompt is displayed, **When** I type "Hello, World!" and press Enter, **Then** I see "You said: Hello, World!" echoed back (exact format)
3. **Given** the prompt is displayed, **When** I type nothing and press Enter, **Then** I see "You said: " echoed back (empty input accepted)
4. **Given** the prompt is displayed, **When** I type special characters like "こんにちは" or "🎉", **Then** the input is echoed back correctly with proper Unicode rendering

---

### User Story 2 - Build the Examples Solution (Priority: P1)

As a developer, I want to build the examples solution independently from the main Stroke solution, so that example code remains isolated and I can easily explore them.

**Why this priority**: The examples infrastructure must be in place before any example can run. This is foundational.

**Independent Test**: Can be tested by running `dotnet build examples/Stroke.Examples.sln` and verifying it succeeds with no errors.

**Acceptance Scenarios**:

1. **Given** the Stroke library is built, **When** I run `dotnet build examples/Stroke.Examples.sln`, **Then** the build succeeds with zero errors and zero warnings
2. **Given** the examples solution exists, **When** I open it in an IDE (VS Code, Visual Studio, Rider), **Then** the solution explorer shows `Stroke.Examples.Prompts` project containing `GetInput.cs` file visible without expanding hidden folders

---

### User Story 3 - Run Named Example via Command Line (Priority: P2)

As a developer exploring the examples, I want to run a specific example by name via command-line argument, so that I can easily switch between different examples without editing code.

**Why this priority**: This enables a discoverable example runner pattern that scales to the 129 planned examples.

**Independent Test**: Can be tested by running `dotnet run --project examples/Stroke.Examples.Prompts -- GetInput` and verifying GetInput runs.

**Acceptance Scenarios**:

1. **Given** the examples project is built, **When** I run `dotnet run --project examples/Stroke.Examples.Prompts -- GetInput`, **Then** the GetInput example runs (case-sensitive match)
2. **Given** the examples project is built, **When** I run `dotnet run --project examples/Stroke.Examples.Prompts -- UnknownExample`, **Then** I see an error message in the format: `Unknown example: 'UnknownExample'. Available examples: GetInput` (alphabetically sorted list)
3. **Given** the examples project is built, **When** I run `dotnet run --project examples/Stroke.Examples.Prompts` with no arguments, **Then** GetInput runs by default (first example alphabetically, which is GetInput for this feature)

---

### Edge Cases

- **Ctrl+C during input**: Prompt MUST throw `KeyboardInterruptException` and exit with code 130 (standard Unix SIGINT convention)
- **Narrow terminal**: Prompt text MUST wrap at terminal boundary; minimum supported width is 10 columns
- **Cross-platform behavior**: Input/output MUST be identical on Linux, macOS, and Windows 10+; any platform-specific rendering differences are handled by Stroke internally and are not visible to the user

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST create the following directory structure:
  ```
  examples/
  ├── Stroke.Examples.sln
  └── Stroke.Examples.Prompts/
      ├── Stroke.Examples.Prompts.csproj
      ├── Program.cs
      └── GetInput.cs
  ```
- **FR-002**: System MUST provide a `Stroke.Examples.sln` solution file that builds independently from the main `Stroke.sln` (no shared solution items; references Stroke via `<ProjectReference>`)
- **FR-003**: System MUST provide a `Stroke.Examples.Prompts.csproj` with:
  - `<TargetFramework>net10.0</TargetFramework>`
  - `<OutputType>Exe</OutputType>`
  - `<LangVersion>13</LangVersion>`
  - `<Nullable>enable</Nullable>`
  - `<ImplicitUsings>enable</ImplicitUsings>`
  - `<ProjectReference Include="..\..\src\Stroke\Stroke.csproj" />`
- **FR-004**: System MUST implement a `GetInput` example that prompts with exact text: `"Give me some input: "` (including trailing space)
- **FR-005**: System MUST echo back the user's input in exact format: `"You said: {input}"` followed by newline
- **FR-006**: System MUST provide a `Program.cs` entry point using dictionary-based routing (not reflection) that:
  - Accepts example name as first command-line argument
  - Matches example names case-sensitively (e.g., `GetInput` works, `getinput` fails)
  - Routes to the corresponding example's `Run()` method
- **FR-007**: System MUST default to running `GetInput` when no command-line argument is provided
- **FR-008**: System MUST display error message for unknown examples in format: `Unknown example: '{name}'. Available examples: {comma-separated alphabetical list}`
- **FR-009**: System MUST match Python Prompt Toolkit's `get-input.py` behavior per these criteria:
  - Same prompt text: `"Give me some input: "`
  - Same output format: `f"You said: {answer}"`
  - Same keyboard handling: Enter submits, Ctrl+C interrupts, standard editing keys work
  - Reference file: `/Users/brandon/src/python-prompt-toolkit/examples/prompts/get-input.py`

### Key Entities

- **Example Project**: A standalone executable project containing related examples (Prompts, Dialogs, FullScreen, etc.)
- **Example**: A static class with a `public static void Run()` method that demonstrates a specific Stroke feature
- **Entry Point**: `Program.cs` that uses a `Dictionary<string, Action>` mapping example names to their `Run()` methods
- **Extensibility Pattern**: To add a new example, create `ExampleName.cs` with static `Run()` method, then add entry to dictionary in `Program.cs`

### API Mapping

The C# implementation uses `Prompt.RunPrompt()` from `Stroke.Shortcuts` which maps to Python's `prompt()` function:

| Python | C# | Notes |
|--------|-----|-------|
| `from prompt_toolkit import prompt` | `using Stroke.Shortcuts;` | Namespace import |
| `answer = prompt("...")` | `var answer = Prompt.RunPrompt("...");` | Synchronous call |
| `print(f"You said: {answer}")` | `Console.WriteLine($"You said: {answer}");` | Standard output |

**Acceptable Deviations**: None. The C# API is designed to match Python semantics exactly for this simple case.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: `dotnet build examples/Stroke.Examples.sln` completes successfully with zero errors and zero warnings
- **SC-002**: `dotnet run --project examples/Stroke.Examples.Prompts` displays the prompt within 2 seconds measured from process start (`tui_launch`) to prompt text visible (`tui_wait_for_text`)
- **SC-003**: User can type text and see it echoed back within 100ms measured from Enter key press (`tui_press_key Enter`) to output text visible (`tui_wait_for_text`)
- **SC-004**: Example code in `GetInput.cs` is under 15 lines where a "line" is any non-blank line after removing:
  - `using` statements
  - `namespace` declaration
  - Opening/closing braces on their own line
  - XML doc comments (`///`)
- **SC-005**: Example behavior matches Python Prompt Toolkit's `get-input.py` exactly when tested with identical inputs per FR-009 criteria
- **SC-006**: TUI Driver verification script confirms correct interactive behavior (see Verification Script below)
- **SC-007**: Unicode input (CJK characters, emoji) is echoed back with correct display width and no corruption

### Verification Script

```javascript
// TUI Driver verification for SC-006
async function verifyGetInputExample(tui) {
  // Launch the example
  const session = await tui.launch('dotnet', [
    'run', '--project', 'examples/Stroke.Examples.Prompts'
  ], { cwd: '/Users/brandon/src/stroke' });

  // SC-002: Prompt displays within 2 seconds
  await tui.waitForText(session, 'Give me some input: ', { timeout: 2000 });

  // Test normal input (US1-AS2)
  await tui.sendText(session, 'Hello, World!');
  await tui.pressKey(session, 'Enter');

  // SC-003: Echo within 100ms
  await tui.waitForText(session, 'You said: Hello, World!', { timeout: 100 });

  await tui.close(session);

  // Test empty input (US1-AS3)
  const session2 = await tui.launch('dotnet', [
    'run', '--project', 'examples/Stroke.Examples.Prompts'
  ], { cwd: '/Users/brandon/src/stroke' });

  await tui.waitForText(session2, 'Give me some input: ', { timeout: 2000 });
  await tui.pressKey(session2, 'Enter');
  await tui.waitForText(session2, 'You said: ', { timeout: 100 });

  await tui.close(session2);

  // Test Unicode input (US1-AS4, SC-007)
  const session3 = await tui.launch('dotnet', [
    'run', '--project', 'examples/Stroke.Examples.Prompts'
  ], { cwd: '/Users/brandon/src/stroke' });

  await tui.waitForText(session3, 'Give me some input: ', { timeout: 2000 });
  await tui.sendText(session3, 'こんにちは 🎉');
  await tui.pressKey(session3, 'Enter');
  await tui.waitForText(session3, 'You said: こんにちは 🎉', { timeout: 100 });

  await tui.close(session3);

  // Test named example routing (US3-AS1)
  const session4 = await tui.launch('dotnet', [
    'run', '--project', 'examples/Stroke.Examples.Prompts', '--', 'GetInput'
  ], { cwd: '/Users/brandon/src/stroke' });

  await tui.waitForText(session4, 'Give me some input: ', { timeout: 2000 });
  await tui.close(session4);

  // Test unknown example error (US3-AS2)
  const session5 = await tui.launch('dotnet', [
    'run', '--project', 'examples/Stroke.Examples.Prompts', '--', 'UnknownExample'
  ], { cwd: '/Users/brandon/src/stroke' });

  await tui.waitForText(session5, "Unknown example: 'UnknownExample'", { timeout: 2000 });
  await tui.waitForText(session5, 'Available examples: GetInput', { timeout: 100 });

  await tui.close(session5);
}
```
