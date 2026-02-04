# Requirements Quality Checklist: SSH Server Integration

**Purpose**: Thorough requirements validation for author self-review and PR review
**Created**: 2026-02-03
**Feature**: [spec.md](../spec.md)
**Focus Areas**: API Fidelity, Protocol Correctness, Concurrency & Thread Safety, Integration Completeness

---

## Requirement Conflicts & Ambiguities

- [x] CHK001 - **CONFLICT**: Spec header says "SSH.NET library" but plan/research decided "FxSsh" — which is authoritative? [Conflict, Spec §Input vs Plan §Summary] — **RESOLVED**: Updated spec.md to say "FxSsh library"
- [x] CHK002 - Is the term "SSH.NET" in spec.md a typo that should be corrected to "FxSsh"? [Ambiguity, Spec §Input] — **RESOLVED**: Corrected to "FxSsh"
- [x] CHK003 - Are default terminal size dimensions consistently specified as width×height (79×20) or height×width? [Consistency, Spec §FR-004 vs Data-Model] — **RESOLVED**: FR-004 now explicitly says `Size(columns=79, rows=20)` (width×height)
- [x] CHK004 - FR-011 requires "disable SSH library line editing via SetLineMode(false)" but research confirms FxSsh has no line mode — is this requirement achievable? [Conflict, Spec §FR-011 vs Research §R2] — **RESOLVED**: FR-011 revised to document this as no-op for FxSsh
- [x] CHK005 - Is SetLineMode documented as a no-op acceptable, or should FR-011 be revised? [Clarity, Spec §FR-011] — **RESOLVED**: FR-011 now explains the no-op behavior while maintaining API consistency

---

## API Fidelity (Python PTK Parity)

### Class & Method Mapping

- [x] CHK006 - Are all public classes from Python PTK's `contrib/ssh/server.py` explicitly mapped to C# equivalents? [Completeness, Spec §Key Entities] — **RESOLVED**: Added "API Mapping (Python PTK → C#)" section with complete class/method tables
- [x] CHK007 - Is `PromptToolkitSSHSession` (Python) → `StrokeSshSession` (C#) naming convention documented? [Clarity, Gap] — **RESOLVED**: Documented in Class Name Mapping table with notes on case convention
- [x] CHK008 - Is `PromptToolkitSSHServer` (Python) → `StrokeSshServer` (C#) naming convention documented? [Clarity, Gap] — **RESOLVED**: Documented in Class Name Mapping table
- [x] CHK009 - Are all Python PTK SSH module public methods listed with their C# equivalents? [Completeness, Spec §FR-001 to FR-015] — **RESOLVED**: Added comprehensive Method Mapping table
- [x] CHK010 - Is the `session_requested()` Python method mapped to `CreateSession()` explicitly documented? [Clarity, Research §R2] — **RESOLVED**: Documented in Method Mapping table with notes
- [x] CHK011 - Is the nested `Stdout` class pattern from Python faithfully ported as `SshChannelStdout`? [Completeness, Research §R3] — **RESOLVED**: Added "Nested Stdout Class → SshChannelStdout" mapping table
- [x] CHK012 - Are constructor parameter orderings consistent with Python PTK conventions? [Consistency, Contracts vs Python] — **RESOLVED**: Added Constructor Parameter Mapping tables for both classes

### Virtual Method Extensibility

- [x] CHK013 - Are the semantics of `BeginAuth` return value (false = skip auth) clearly documented? [Clarity, Spec §FR-010] — **RESOLVED**: Added "Virtual Method Semantics" section with BeginAuth semantics
- [x] CHK014 - Is there guidance on implementing password vs public key authentication via `BeginAuth`? [Coverage, Gap] — **RESOLVED**: Added password auth pattern and noted public key is out of scope
- [x] CHK015 - Is the `CreateSession` virtual method signature consistent with Python's `session_requested`? [Consistency, Contracts §CreateSession] — **RESOLVED**: Documented in Method Mapping table, CreateSession semantics added
- [x] CHK016 - Are subclassing patterns for custom authentication documented with examples? [Coverage, Contracts §Custom Authentication] — **RESOLVED**: Added code example for AuthenticatedSshServer subclass pattern

---

## Protocol Correctness (SSH/NVT)

### Line Ending Conversion

- [x] CHK017 - Is the LF→CRLF conversion requirement specified for all output paths? [Completeness, Spec §FR-007] — **RESOLVED**: Added "Protocol Details (NVT/SSH)" section with explicit output conversion rule
- [x] CHK018 - Is CRLF→LF conversion for input specified, or is input passed through unchanged? [Gap, Spec §FR-005] — **RESOLVED**: Documented that input is passed through unchanged; Stroke input parser handles all sequences
- [x] CHK019 - Are embedded CR or CRLF in input data handling requirements defined? [Edge Case, Gap] — **RESOLVED**: Documented in Protocol Details that embedded CRLF passes through unchanged
- [x] CHK020 - Is the NVT specification reference (RFC 854) documented for traceability? [Traceability, Gap] — **RESOLVED**: RFC 854 explicitly referenced in Protocol Details section

### Terminal Negotiation

- [x] CHK021 - Are requirements for initial terminal size negotiation timing specified? [Completeness, Spec §US-2] — **RESOLVED**: Documented that initial size is received via PtyReceived event
- [x] CHK022 - Are default terminal dimensions (79×20) justified with rationale? [Clarity, Spec §FR-004] — **RESOLVED**: Added rationale: matches Python PTK, 79 allows 1-char margin in 80-col terminals
- [x] CHK023 - Is terminal size clamping range (1-500) documented with rationale? [Clarity, Data-Model §Runtime Validation] — **RESOLVED**: Documented clamping to 1-500 to prevent memory issues
- [x] CHK024 - Are requirements for terminal type negotiation (xterm, vt100, etc.) specified? [Completeness, Contracts §GetTerminalType] — **RESOLVED**: Documented common terminal types and how they're obtained
- [x] CHK025 - Is behavior defined when client doesn't send terminal type? [Edge Case, Gap] — **RESOLVED**: Documented vt100 as default fallback

### Cursor Position Requests

- [x] CHK026 - Are CPR enable/disable requirements clear for both server and session levels? [Clarity, Spec §FR-002, §US-4] — **RESOLVED**: Documented EnableCpr property on both server and session
- [x] CHK027 - Is CPR response timeout specified? [Gap, Spec §US-4] — **RESOLVED**: Documented 1-second default timeout via Vt100Output's existing mechanism
- [x] CHK028 - Is behavior defined when CPR is enabled but terminal doesn't respond? [Edge Case, Gap] — **RESOLVED**: Documented graceful handling of missing responses via timeout

---

## Concurrency & Thread Safety

### Server-Level Threading

- [x] CHK029 - Is the thread safety mechanism (`ConcurrentDictionary`, `Lock`) mandated or just recommended? [Clarity, Plan §Constitution Check XI] — **RESOLVED**: Added "Concurrency & Thread Safety Requirements" section with mandated mechanisms per class
- [x] CHK030 - Are concurrent connection acceptance requirements specified (accept while processing)? [Completeness, Data-Model §Concurrent Access] — **RESOLVED**: Documented in "Concurrent Access Guarantees" section
- [x] CHK031 - Is maximum concurrent connections limit defined beyond SC-002's "100 without exhaustion"? [Clarity, Spec §SC-002] — **RESOLVED**: Added "Concurrency Limits" section with 100 session target and degradation behavior
- [x] CHK032 - Are rapid connect/disconnect cycle handling requirements measurable? [Measurability, Spec §Edge Cases] — **RESOLVED**: Documented thread-safe collections and tested via stress tests

### Session-Level Threading

- [x] CHK033 - Is it specified which operations on `StrokeSshSession` are thread-safe? [Completeness, Contracts §StrokeSshSession] — **RESOLVED**: Thread safety table shows Lock for session state, volatile for booleans
- [x] CHK034 - Are requirements for concurrent `DataReceived` calls defined? [Coverage, Gap] — **RESOLVED**: Documented that FxSsh serializes per-session calls; cross-session is concurrent
- [x] CHK035 - Is atomicity scope for session state mutations documented? [Clarity, Data-Model §Thread Safety] — **RESOLVED**: Documented per-operation atomicity; compound operations need caller sync
- [x] CHK036 - Are requirements for cross-session communication (broadcast) defined? [Coverage, Gap] — **RESOLVED**: Out of scope for core spec; application code can use Connections snapshot for broadcast

### Shutdown & Cleanup

- [x] CHK037 - Is graceful shutdown sequence explicitly ordered (stop accepting → close sessions → cleanup)? [Completeness, Gap] — **RESOLVED**: Added explicit 5-step shutdown sequence in spec
- [x] CHK038 - Is the 2-second cleanup timeout (SC-004) enforced or best-effort? [Clarity, Spec §SC-004] — **RESOLVED**: Documented 5-second timeout with force-close after; SC-004 updated to 5 seconds for consistency
- [x] CHK039 - Are requirements for session cleanup on interact callback exception defined? [Coverage, Spec §Edge Cases] — **RESOLVED**: Documented in Edge Cases that exception is logged and channel closed
- [x] CHK040 - Is cancellation token propagation to sessions specified? [Completeness, Contracts §RunAsync] — **RESOLVED**: Added "Cancellation Token Propagation" section with full flow

---

## Integration Completeness (FxSsh Mapping)

### Event Mapping

- [x] CHK041 - Are all FxSsh events mapped to Stroke handlers explicitly? [Completeness, Research §R2] — **RESOLVED**: Added "FxSsh Integration Details" section with complete event mapping table
- [x] CHK042 - Is `ConnectionAccepted` → session creation flow documented? [Clarity, Data-Model §Event Flow] — **RESOLVED**: Documented in event mapping table
- [x] CHK043 - Is `UserAuthService.UserAuth` → `BeginAuth` mapping documented? [Clarity, Research §R6] — **RESOLVED**: Documented with UserAuthArgs.Result handling
- [x] CHK044 - Is `PtyReceived` → terminal size storage mapping documented? [Clarity, Research §R4] — **RESOLVED**: Documented in event mapping table
- [x] CHK045 - Is `WindowChange` → `TerminalSizeChanged` mapping documented? [Clarity, Research §R4] — **RESOLVED**: Documented with app invalidation trigger
- [x] CHK046 - Is `CommandOpened` → session start flow documented? [Clarity, Data-Model §Event Flow] — **RESOLVED**: Documented I/O infrastructure creation

### Host Key Management

- [x] CHK047 - Are supported host key algorithms (RSA, ECDSA, Ed25519) explicitly listed? [Completeness, Data-Model §Validation Rules] — **RESOLVED**: Added complete algorithm list with recommendations
- [x] CHK048 - Is host key file format (PEM) validated at constructor time or runtime? [Clarity, Contracts §hostKeyPath] — **RESOLVED**: Documented constructor-time validation
- [x] CHK049 - Are requirements for invalid/missing host key handling defined? [Edge Case, Gap] — **RESOLVED**: Added specific exception types and messages
- [x] CHK050 - Is host key generation guidance provided for examples/testing? [Coverage, Quickstart] — **RESOLVED**: Quickstart.md has ssh-keygen commands; referenced in spec

### Authentication Flow

- [x] CHK051 - Is the authentication flow when `BeginAuth` returns `true` fully specified? [Completeness, Gap] — **RESOLVED**: Added complete 5-step flow for auth required case
- [x] CHK052 - Are password validation callback requirements defined? [Gap, Spec §FR-010] — **RESOLVED**: Documented UserAuth hook and Result setting
- [x] CHK053 - Are public key authentication requirements defined? [Gap] — **RESOLVED**: Documented as out of scope for Python PTK parity
- [x] CHK054 - Is authentication timeout behavior specified? [Gap, Data-Model §Runtime Validation] — **RESOLVED**: Documented 30-second FxSsh default timeout
- [x] CHK055 - Are authentication failure logging requirements defined? [Gap] — **RESOLVED**: Documented Warning level logging with username, IP, reason

---

## Edge Cases & Error Handling

### Connection Lifecycle

- [x] CHK056 - Is data buffering before session initialization explicitly bounded (buffer size limit)? [Clarity, Spec §Edge Cases] — **RESOLVED**: Added 64KB buffer limit in Edge Cases and Buffer Size Limits section
- [x] CHK057 - Is SSH negotiation timeout delegated to FxSsh or custom? [Clarity, Spec §Edge Cases] — **RESOLVED**: Documented as FxSsh's 30-second default
- [x] CHK058 - Are network failure detection requirements specified? [Completeness, Spec §US-5] — **RESOLVED**: Added "Network Failure Detection" section with TCP timeout details
- [x] CHK059 - Is partial data handling (mid-escape-sequence disconnect) specified? [Edge Case, Gap] — **RESOLVED**: Documented in Edge Cases that partial data is discarded

### Resource Management

- [x] CHK060 - Are all disposable resources (`PipeInput`, `AppSession`, `Vt100Output`) listed for cleanup? [Completeness, Spec §FR-012] — **RESOLVED**: Added "Disposable Resource Lifecycle" table with all 4 resources
- [x] CHK061 - Is disposal order specified to prevent use-after-dispose? [Clarity, Gap] — **RESOLVED**: Documented 4-step disposal order with rationale
- [x] CHK062 - Are memory leak prevention requirements testable? [Measurability, Spec §US-5] — **RESOLVED**: Added testability note: 100 cycles, check heap growth
- [x] CHK063 - Is socket cleanup (shutdown + close) ordering specified? [Clarity, Gap] — **RESOLVED**: Documented channel closed last for graceful SSH disconnect

### Error Propagation

- [x] CHK064 - Is interact callback exception handling specified (log and close)? [Completeness, Spec §Edge Cases] — **RESOLVED**: Added to Exception Handling Strategy table
- [x] CHK065 - Are channel write failure handling requirements defined? [Gap] — **RESOLVED**: Documented BrokenPipeError handling in table
- [x] CHK066 - Is encoding error handling (invalid byte sequences) specified? [Gap] — **RESOLVED**: Documented U+FFFD replacement character approach
- [x] CHK067 - Are FxSsh library exception types that should be caught documented? [Gap] — **RESOLVED**: Listed SshConnectionException and SshAuthenticationException

---

## Success Criteria Measurability

- [x] CHK068 - Can SC-001 ("complete interactive workflows") be objectively measured? [Measurability, Spec §SC-001] — **RESOLVED**: Added measurement method and pass criteria (3 workflow types)
- [x] CHK069 - Is SC-002's "100 concurrent sessions" testable with specific acceptance criteria? [Measurability, Spec §SC-002] — **RESOLVED**: Added memory limit (500MB), timing (60s), and pass criteria
- [x] CHK070 - Is SC-003's "100ms" latency measurable from resize event to re-render? [Measurability, Spec §SC-003] — **RESOLVED**: Added measurement method (timer to _on_resize) and 95th percentile criteria
- [x] CHK071 - Is SC-004's "2 seconds" cleanup verifiable with specific start/end points? [Measurability, Spec §SC-004] — **RESOLVED**: Updated to 5 seconds with explicit start/end points defined
- [x] CHK072 - Is SC-005's "80% coverage" calculated over which code (only SSH, or including deps)? [Clarity, Spec §SC-005] — **RESOLVED**: Scoped to Stroke.Contrib.Ssh namespace only, no exclusions
- [x] CHK073 - Is SC-006's "faithfully ported" defined with specific API checklist? [Clarity, Spec §SC-006] — **RESOLVED**: References API Mapping section with explicit pass criteria

---

## Documentation & Traceability

- [x] CHK074 - Are all 15 functional requirements (FR-001 to FR-015) traceable to acceptance scenarios? [Traceability] — **RESOLVED**: Added "FR → Acceptance Scenarios" traceability table
- [x] CHK075 - Are all 5 user stories traceable to functional requirements? [Traceability] — **RESOLVED**: Added "User Stories → Functional Requirements" traceability table
- [x] CHK076 - Are all 6 success criteria traceable to testable requirements? [Traceability] — **RESOLVED**: Added "Success Criteria → Testable Requirements" traceability table
- [x] CHK077 - Is the asyncssh → FxSsh adaptation rationale documented for future maintainers? [Clarity, Research §R1] — **RESOLVED**: Added "Adaptation Notes (asyncssh → FxSsh)" section with rationale
- [x] CHK078 - Are differences from Python PTK's asyncssh patterns explicitly documented? [Clarity, Gap] — **RESOLVED**: Added key differences table (async context, set_line_mode, subclassing)

---

## Testing Requirements

- [x] CHK079 - Are integration test requirements (real SSH connections) specified? [Completeness, Plan §Testing] — **RESOLVED**: Added "Integration Test Requirements" section with real connection mandate
- [x] CHK080 - Is SSH.NET as test client a requirement or suggestion? [Clarity, Research §R9] — **RESOLVED**: Documented as requirement (MIT license, well-maintained)
- [x] CHK081 - Are concurrent stress test requirements (10+ threads, 1000+ ops) specified? [Completeness, Constitution XI] — **RESOLVED**: Added "Concurrency Test Requirements" with specific numbers
- [x] CHK082 - Are test isolation requirements (port allocation, cleanup) specified? [Gap] — **RESOLVED**: Added "Test Isolation Requirements" section
- [x] CHK083 - Is test coverage calculation methodology defined? [Clarity, Spec §SC-005] — **RESOLVED**: SC-005 now specifies Coverlet, namespace scope, no exclusions

---

## Summary

**Total Items**: 83
**Completed**: 83 ✅
**Categories**:
- Conflicts & Ambiguities: 5/5 ✅
- API Fidelity: 11/11 ✅
- Protocol Correctness: 12/12 ✅
- Concurrency & Thread Safety: 12/12 ✅
- Integration Completeness: 15/15 ✅
- Edge Cases & Error Handling: 12/12 ✅
- Success Criteria Measurability: 6/6 ✅
- Documentation & Traceability: 5/5 ✅
- Testing Requirements: 5/5 ✅

**Critical Issues Resolved**:
1. ~~**CHK001**: Spec/Plan library conflict (SSH.NET vs FxSsh)~~ → Updated spec.md to say "FxSsh library"
2. ~~**CHK004/CHK005**: FR-011 SetLineMode requirement may be unachievable~~ → FR-011 revised to document as no-op for FxSsh
3. ~~**CHK051-055**: Authentication flow underspecified~~ → Added complete "Authentication Flow" section
4. ~~**CHK018-020**: Input handling requirements missing~~ → Added "Protocol Details (NVT/SSH)" section

**Spec Strengthening Completed**: 2026-02-03
