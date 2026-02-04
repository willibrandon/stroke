# Implementation Plan: SSH Server Integration

**Branch**: `061-ssh-server` | **Date**: 2026-02-03 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/061-ssh-server/spec.md`

## Summary

Implement SSH server integration allowing Stroke applications to run over SSH connections using the FxSsh library, following Python Prompt Toolkit's asyncssh-based ssh module patterns. The implementation provides `PromptToolkitSshServer` (inheritable server class with virtual authentication/session hooks) and `PromptToolkitSshSession` (per-connection session managing PipeInput, Vt100Output, AppSession).

## Technical Context

**Language/Version**: C# 13 / .NET 10+
**Primary Dependencies**: FxSsh (v1.3.0, MIT license) for SSH server functionality, existing Stroke libraries (Stroke.Application, Stroke.Input.Pipe, Stroke.Output, Stroke.Styles)
**Storage**: N/A (in-memory session management only)
**Testing**: xUnit (no mocks per Constitution VIII), real SSH connections for integration tests
**Target Platform**: Linux, macOS, Windows 10+ (cross-platform via FxSsh)
**Project Type**: Single .NET library (Stroke.Contrib.Ssh namespace within existing Stroke project)
**Performance Goals**: 100 concurrent SSH sessions per SC-002, terminal resize events reflected within 100ms per SC-003
**Constraints**: Session cleanup within 5 seconds per SC-004, 80% test coverage per Constitution VIII
**Scale/Scope**: Network server component supporting interactive terminal applications over SSH

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Faithful Port | ✅ PASS | Port mirrors Python PTK's `contrib/ssh/server.py` with `PromptToolkitSSHSession` and `PromptToolkitSSHServer` classes |
| II. Immutability | ✅ PASS | Sessions are mutable (necessary for connection state), use Lock for thread safety |
| III. Layered Architecture | ✅ PASS | `Stroke.Contrib.Ssh` depends on Application, Input, Output layers—same pattern as existing `Stroke.Contrib.Telnet` |
| IV. Cross-Platform | ✅ PASS | FxSsh supports Windows, Linux, macOS |
| V. Editing Mode Parity | N/A | No key binding changes |
| VI. Performance | ✅ PASS | Differential rendering delegated to existing Vt100Output/Renderer |
| VII. Full Scope | ✅ PASS | All 15 functional requirements addressed |
| VIII. Real-World Testing | ✅ PASS | Integration tests use real SSH connections, no mocks |
| IX. Planning Documents | ✅ PASS | API follows existing TelnetServer pattern, aligned with api-mapping.md conventions |
| X. File Size Limits | ✅ PASS | Split across 5 files (~200-400 LOC each): SshServer, SshSession, SshChannelStdout, ISshChannel, SshChannel |
| XI. Thread Safety | ✅ PASS | `Lock` with `EnterScope()` for mutable state, `ConcurrentDictionary` for sessions |

## Project Structure

### Documentation (this feature)

```text
specs/061-ssh-server/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── ssh-server.md    # API contracts in markdown
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
src/Stroke/
└── Contrib/
    └── Ssh/
        ├── PromptToolkitSshServer.cs   # Main server class (~350 LOC)
        ├── PromptToolkitSshSession.cs  # Per-connection session (~300 LOC)
        ├── SshChannelStdout.cs         # TextWriter with LF→CRLF (~100 LOC)
        ├── ISshChannel.cs              # Channel abstraction interface (~50 LOC)
        └── SshChannel.cs               # FxSsh channel adapter (~150 LOC)

tests/Stroke.Tests/
└── Contrib/
    └── Ssh/
        ├── SshServerTests.cs               # Constructor/configuration tests
        ├── SshServerLifecycleTests.cs      # Start/stop/ready callback tests
        ├── SshSessionTests.cs              # Session isolation tests
        ├── SshChannelStdoutTests.cs        # LF→CRLF conversion tests
        ├── SshServerIntegrationTests.cs    # Full SSH client→server tests
        └── SshServerConcurrencyTests.cs    # Thread safety stress tests

examples/
└── Stroke.Examples.Ssh/
    ├── Program.cs                    # Example runner
    └── Examples/
        └── AsyncsshServer.cs         # Port of asyncssh-server.py (progress bar, prompts, dialogs)
```

**Structure Decision**: Follows existing `Stroke.Contrib.Telnet` structure pattern. New `Ssh` folder added under `Contrib/` with parallel test and example organization.

## Complexity Tracking

No violations requiring justification. Implementation follows established patterns from TelnetServer.

---

## Post-Design Constitution Re-Check

*Completed after Phase 1 design artifacts generated.*

| Principle | Status | Post-Design Notes |
|-----------|--------|-------------------|
| I. Faithful Port | ✅ PASS | Contracts match Python PTK's `PromptToolkitSSHSession`/`PromptToolkitSSHServer` API surface exactly. Virtual `BeginAuth` and `CreateSession` methods mirror asyncssh pattern. |
| II. Immutability | ✅ PASS | Data model confirms mutable session state with `Lock` synchronization. Immutable `Size` values. |
| III. Layered Architecture | ✅ PASS | Contracts show correct layer dependencies: Contrib.Ssh → Application, Input.Pipe, Output, Styles |
| IV. Cross-Platform | ✅ PASS | FxSsh confirmed cross-platform in research.md |
| V. Editing Mode Parity | N/A | No key binding changes in this feature |
| VI. Performance | ✅ PASS | Research confirms FxSsh handles concurrent sessions efficiently |
| VII. Full Scope | ✅ PASS | All 15 FRs have corresponding contract methods |
| VIII. Real-World Testing | ✅ PASS | Research confirms SSH.NET as real client for integration tests |
| IX. Planning Documents | ✅ PASS | Contracts follow TelnetServer naming conventions |
| X. File Size Limits | ✅ PASS | Project structure confirms 5 files, each under 1000 LOC |
| XI. Thread Safety | ✅ PASS | Data model specifies Lock mechanisms and concurrent access patterns |

**Gate Status**: ✅ All principles pass. Ready for Phase 2 task generation.

---

## Generated Artifacts

| Artifact | Status | Path |
|----------|--------|------|
| research.md | ✅ Complete | `specs/061-ssh-server/research.md` |
| data-model.md | ✅ Complete | `specs/061-ssh-server/data-model.md` |
| contracts/ssh-server.md | ✅ Complete | `specs/061-ssh-server/contracts/ssh-server.md` |
| quickstart.md | ✅ Complete | `specs/061-ssh-server/quickstart.md` |
| CLAUDE.md | ✅ Updated | Agent context updated with FxSsh dependency |
