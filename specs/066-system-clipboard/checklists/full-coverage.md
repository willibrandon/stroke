# Full Coverage Checklist: System Clipboard

**Purpose**: Thorough requirements quality validation across all dimensions — cross-platform, security, API fidelity, edge cases, and thread safety
**Created**: 2026-02-07
**Feature**: [spec.md](../spec.md)
**Depth**: Thorough (~45 items)
**Audience**: Reviewer (PR gate)

## Requirement Completeness

- [x] CHK001 - Are clipboard operation requirements defined for all five platform variants (Windows, macOS, Linux Wayland, Linux X11, WSL)? [Completeness, Spec §FR-002]
- [x] CHK002 - Is the behavior specified for when `SetData` succeeds on the provider but the OS clipboard write partially fails (e.g., GlobalAlloc succeeds but SetClipboardData fails on Windows)? [Completeness, Spec §FR-008]
- [x] CHK003 - Are requirements defined for clipboard operations when the process is running in a headless/SSH session with no display server? [Gap]
- [x] CHK004 - Is the expected behavior specified when `GetText` returns a non-empty string but with different line endings (\r\n vs \n) across platforms? [Gap]
- [x] CHK005 - Are requirements for the `ClipboardProviderNotAvailableException` message content specified per platform (Linux, WSL)? Does the spec define what "clear error with installation guidance" means concretely for each platform? [Clarity, Spec §FR-011]
- [x] CHK006 - Is the behavior specified when `SystemClipboard` is constructed with the auto-detect constructor on an unsupported/unknown OS (e.g., FreeBSD)? [Completeness, Spec §FR-003]
- [x] CHK007 - Are requirements defined for how the WSL provider handles `powershell.exe` output encoding (UTF-8 vs UTF-16 BOM)? [Gap]

## Requirement Clarity

- [x] CHK008 - Is "silently swallowed" for write failures (FR-008) sufficiently precise? Does it mean no exception, no logging, no return value, or all three? [Clarity, Spec §FR-008]
- [x] CHK009 - Is the 5-second timeout (FR-009) specified as wall-clock time or process CPU time? Is the behavior on timeout defined — does it kill the process, return empty, or throw? [Clarity, Spec §FR-009]
- [x] CHK010 - Is "argument lists rather than shell-interpreted command strings" (FR-012) specific enough to guide implementation? Does it reference `ProcessStartInfo.ArgumentList` explicitly? [Clarity, Spec §FR-012]
- [x] CHK011 - Is the term "clipboard mechanism" in FR-003 and FR-011 unambiguously defined? Could it be confused with IClipboard vs IClipboardProvider vs OS clipboard? [Clarity, Spec §FR-003]
- [x] CHK012 - Is "construction time" in FR-003 clearly defined as the SystemClipboard constructor, not lazy initialization? [Clarity, Spec §FR-003]
- [x] CHK013 - Is the WSL detection mechanism specified clearly enough? Does the spec or plan define what constitutes WSL detection (e.g., `/proc/version` containing "microsoft")? [Clarity, Spec §FR-014]

## Cross-Platform Consistency

- [x] CHK014 - Are clipboard read/write requirements consistently defined across all process-based providers (macOS, Linux, WSL)? Do they all share the same timeout, error handling, and process execution safety requirements? [Consistency, Spec §FR-008, §FR-009, §FR-012]
- [x] CHK015 - Is the detection priority order (FR-013, FR-014) consistent between the spec and the contracts? The spec says "Wayland before X11" and "WSL before native Linux" — do the contracts reproduce this exact order? [Consistency, Spec §FR-013, §FR-014, Contract §ClipboardProviderDetector]
- [x] CHK016 - Are thread safety requirements consistent between SystemClipboard (Lock-based) and providers (stateless)? Is it clear that SystemClipboard's Lock protects the `_lastData` cache, not the providers themselves? [Consistency, Spec §FR-010, Contract §SystemClipboard]
- [x] CHK017 - Is the `wl-paste --no-newline` flag requirement documented? The contracts mention it for wl-paste but the spec doesn't — is this consistent? [Consistency, Contract §LinuxClipboardProvider]
- [x] CHK018 - Are the acceptance scenarios in US3 consistent with the detection order? US3 doesn't mention WSL as a separate scenario — is this a gap? [Consistency, Spec §US3]

## API Fidelity (Python Prompt Toolkit Port)

- [x] CHK019 - Does the spec document the exact behavioral mapping from Python's `PyperclipClipboard` to Stroke's `SystemClipboard`? Are all deviations explicitly listed? [Completeness, Spec §Input]
- [x] CHK020 - Is the `_lastData` cache comparison specified to use text equality (not reference equality), matching Python's `self._data.text == text` behavior? [Clarity, Contract §SystemClipboard.GetData]
- [x] CHK021 - Is the selection type inference logic for external text (`"\n" in text`) faithfully specified? Does it match Python's exact check (`"\n" in text` vs `text.Contains('\n')`)? [Fidelity, Spec §FR-006]
- [x] CHK022 - Is the `Rotate()` no-op behavior documented as matching Python's inherited no-op from the base `Clipboard` class (not a new invention)? [Fidelity, Spec §FR-015]
- [x] CHK023 - Is the deviation from Python's pyperclip library to inline platform providers explicitly documented with rationale? [Completeness, Plan §Deviation Documentation]
- [x] CHK024 - Does the spec define that `SetData` caches data BEFORE attempting the provider write (matching Python's `self._data = data` before `pyperclip.copy(data.text)`)? [Fidelity, Contract §SystemClipboard.SetData]

## Security & Safety

- [x] CHK025 - Are command injection prevention requirements (FR-012) specific enough? Do they mandate `ProcessStartInfo.ArgumentList` rather than just "argument lists"? [Clarity, Spec §FR-012]
- [x] CHK026 - Is the requirement for `UseShellExecute = false` documented for all subprocess-based providers? [Gap]
- [x] CHK027 - Is the requirement for `CreateNoWindow = true` documented for all subprocess-based providers? [Gap]
- [x] CHK028 - Are requirements defined for sanitizing or validating text before writing to OS clipboard? Could malicious text in the clipboard cause harm when pasted elsewhere? [Gap, Security]
- [x] CHK029 - Is the Win32 memory management lifecycle specified clearly enough to prevent memory leaks (GlobalAlloc → GlobalLock → copy → GlobalUnlock → SetClipboardData, with GlobalFree only on failure)? [Completeness, Contract §WindowsClipboardProvider]
- [x] CHK030 - Are requirements defined for ensuring `CloseClipboard()` is always called even when intermediate Win32 API calls fail (finally block)? [Completeness, Contract §WindowsClipboardProvider]
- [x] CHK031 - Is the `powershell.exe -NoProfile` flag requirement documented to prevent execution of user profile scripts during clipboard reads? [Security, Contract §WslClipboardProvider]

## Edge Case & Scenario Coverage

- [x] CHK032 - Is the behavior specified when clipboard text is an empty string vs null? Does `GetText()` return `""` or could it return `null`? [Completeness, Contract §IClipboardProvider]
- [x] CHK033 - Is the behavior specified when the same `SystemClipboard` instance writes text, an external app modifies the clipboard to the same text, and then `GetData()` is called? Should it return the cached data or infer type? [Edge Case, Spec §FR-005]
- [x] CHK034 - Is the behavior specified for clipboard text containing only `\r\n` (Windows line endings) vs `\n` (Unix)? Does the newline inference in FR-006 handle both? [Edge Case, Spec §FR-006]
- [x] CHK035 - Is the behavior specified when `SetData` is called with `SelectionType.Block`? FR-005 mentions block, but FR-006 only infers Lines or Characters — is Block ever inferred from external text? [Consistency, Spec §FR-005, §FR-006]
- [x] CHK036 - Are requirements defined for the scenario where clipboard tools exist on PATH but are a different version or broken (e.g., xclip installed but segfaults)? [Edge Case, Gap]
- [x] CHK037 - Is the behavior specified for Unicode text containing surrogate pairs, combining characters, or zero-width characters in clipboard operations? [Edge Case, Gap]
- [x] CHK038 - Is the behavior specified when the Linux `which` command itself is not available (some minimal containers)? [Edge Case, Contract §ClipboardProviderDetector]

## Thread Safety Requirements

- [x] CHK039 - Is the atomicity scope clearly defined? The spec says "all public operations MUST be synchronized" (FR-010) — does this cover the compound SetData operation (cache + provider write) as a single atomic unit? [Clarity, Spec §FR-010]
- [x] CHK040 - Are requirements defined for what happens when a provider.SetText() call blocks for 5 seconds while holding the SystemClipboard lock? Could this cause deadlocks or starvation for GetData callers? [Gap, Spec §FR-009, §FR-010]
- [x] CHK041 - Is the thread safety documentation requirement specified for the SystemClipboard XML docs? Constitution XI requires documenting thread safety guarantees. [Completeness, Constitution §XI]
- [x] CHK042 - Are concurrent stress test requirements specified (Constitution XI: "10+ threads, 1000+ operations")? [Completeness, Constitution §XI]

## Acceptance Criteria Quality

- [x] CHK043 - Can SC-001 ("paste text from any external application") be objectively measured? "Any external application" is unbounded — should it specify representative applications? [Measurability, Spec §SC-001]
- [x] CHK044 - Is SC-005 ("complete within 5 seconds, even when platform clipboard tools are unresponsive") testable without an unresponsive clipboard tool? Are test requirements defined for simulating this scenario? [Measurability, Spec §SC-005]
- [x] CHK045 - Is SC-008 ("80% test coverage") measured at the class level, file level, or namespace level? Is it line coverage, branch coverage, or method coverage? [Clarity, Spec §SC-008]
- [x] CHK046 - Can SC-007 ("zero behavior change") be objectively verified? Does it mean no code paths are affected, or no observable behavior changes? [Measurability, Spec §SC-007]

## Dependencies & Assumptions

- [x] CHK047 - Is the assumption that `pbcopy`/`pbpaste` ship with all macOS versions validated? Are minimum macOS version requirements specified? [Assumption, Spec §Assumptions]
- [x] CHK048 - Is the assumption that WSL environments have `clip.exe` and `powershell.exe` validated for WSL1 vs WSL2? Are both WSL versions in scope? [Assumption, Spec §Assumptions]
- [x] CHK049 - Is the dependency on `PlatformUtils.IsWsl` (a new property to be added) documented as a cross-file dependency? [Dependency, Plan §Source Code]
- [x] CHK050 - Are requirements for the `which` command availability on all Linux distributions documented? Some minimal Docker images lack `which`. [Dependency, Contract §ClipboardProviderDetector]

## Notes

- Check items off as completed: `[x]`
- Items marked `[Gap]` indicate potential missing requirements that should be addressed in the spec
- Items marked `[Fidelity]` relate to Python Prompt Toolkit port accuracy
- Items referencing `Contract §X` point to the design contracts in `contracts/`
- This checklist was generated at Thorough depth with Full Coverage focus
