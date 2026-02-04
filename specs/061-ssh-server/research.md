# Research: SSH Server Integration

**Feature**: 061-ssh-server
**Created**: 2026-02-03

## Research Questions & Findings

### R1: Which .NET SSH Server Library to Use?

**Decision**: FxSsh (v1.3.0, MIT license)

**Rationale**:
- **Open source** with MIT license (compatible with Stroke's MIT license)
- **Server-side focused** (unlike SSH.NET which is client-only)
- **Modern .NET support** (.NET 8.0+, cross-platform)
- **Active maintenance** (recent updates on NuGet)
- **Comprehensive SSH-2 implementation** following RFC 4250-4254

**Alternatives Considered**:
1. **SSH.NET (v2025.1.0)** - Rejected: Client-only library, no server-side capability
2. **Rebex SSH Pack** - Rejected: Commercial ($899+), not compatible with open-source project
3. **Custom implementation** - Rejected: SSH protocol complexity makes this impractical

**Sources**:
- [FxSsh on NuGet](https://www.nuget.org/packages/FxSsh)
- [FxSsh on GitHub](https://github.com/Aimeast/FxSsh)
- [SSH Library Comparison Analysis](https://gist.github.com/Aimeast/ae648d1f76bba48858b600517e77bbda)

---

### R2: FxSsh API Patterns for Session Management

**Decision**: Use FxSsh's event-driven model with custom session adapter

**Key FxSsh Patterns**:
1. **SshServer class** - Core server, manages host keys via `AddHostKey()`
2. **ConnectionAccepted event** - Fires when clients connect, provides `Session` object
3. **UserAuthService.UserAuth event** - Authentication hook with `UserAuthArgs`
4. **ConnectionService events**:
   - `CommandOpened` → Shell/exec requests
   - `PtyReceived` → Terminal size negotiation
   - `WindowChange` → Terminal resize events
   - Channel `DataReceived`/`SendData` → Bidirectional data flow

**Mapping to Python PTK Pattern**:

| Python asyncssh | FxSsh | Stroke |
|-----------------|-------|--------|
| `SSHServer.begin_auth()` | `UserAuthService.UserAuth` | `StrokeSshServer.BeginAuth()` virtual |
| `SSHServer.session_requested()` | `ConnectionService.CommandOpened` | `StrokeSshServer.CreateSession()` virtual |
| `SSHServerSession.connection_made()` | `ConnectionAccepted` event | `StrokeSshSession` constructor |
| `SSHServerSession.data_received()` | Channel `DataReceived` | `StrokeSshSession.DataReceived()` |
| `SSHServerSession.terminal_size_changed()` | `WindowChange` event | `StrokeSshSession.TerminalSizeChanged()` |
| `session._chan.write()` | Channel `SendData()` | `SshChannelStdout.Write()` |
| `session._chan.set_line_mode()` | N/A (FxSsh has no line mode) | `ISshChannel.SetLineMode()` (no-op) |

**Sources**:
- [AsyncSSH API Documentation](https://asyncssh.readthedocs.io/en/latest/api.html)
- [FxSsh GitHub README](https://github.com/Aimeast/FxSsh)

---

### R3: LF to CRLF Conversion Pattern

**Decision**: Wrap SSH channel in TextWriter that converts LF to CRLF (identical to TelnetServer's `ConnectionStdout`)

**Rationale**:
- SSH uses NVT (Network Virtual Terminal) conventions like Telnet
- Python PTK does this in the nested `Stdout` class: `self._chan.write(data.replace("\n", "\r\n"))`
- Existing `Stroke.Contrib.Telnet.ConnectionStdout` provides proven pattern

**Implementation Pattern**:
```csharp
public override void Write(string? value)
{
    if (value == null) return;
    var converted = value.Replace("\n", "\r\n");
    _channel.SendData(converted);
}
```

---

### R4: Terminal Size Handling

**Decision**: Use FxSsh's `PtyReceived` event for initial size and `WindowChange` event for resize

**Python PTK Pattern**:
```python
def _get_size(self) -> Size:
    if self._chan is None:
        return Size(rows=20, columns=79)  # Default
    else:
        width, height, pixwidth, pixheight = self._chan.get_terminal_size()
        return Size(rows=height, columns=width)
```

**FxSsh Equivalent**:
- `PtyReceived` event provides initial `width` and `height`
- `WindowChange` event provides updated dimensions
- Store current size in session, return default (79x20) until negotiated

---

### R5: Session Isolation Pattern

**Decision**: Each session creates isolated `PipeInput`, `Vt100Output`, and `AppSession` (matching TelnetConnection pattern)

**From Python PTK**:
```python
with create_pipe_input() as self._input:
    with create_app_session(input=self._input, output=self._output) as session:
        self.app_session = session
        await self.interact(self)
```

**Stroke Equivalent** (from TelnetConnection):
```csharp
using var pipeInput = new SimplePipeInput();
using var session = AppCtx.CreateAppSession(_pipeInput, _vt100Output);
await _interact(this).ConfigureAwait(false);
```

---

### R6: Authentication Extensibility

**Decision**: Virtual `BeginAuth(string username)` method returning `bool` (false = no auth required)

**Python PTK Pattern**:
```python
class PromptToolkitSSHServer(asyncssh.SSHServer):
    def begin_auth(self, username: str) -> bool:
        # No authentication.
        return False
```

**FxSsh Mapping**:
- Hook into `UserAuthService.UserAuth` event
- Call virtual `BeginAuth(username)` on server instance
- Set `UserAuthArgs.Result` based on return value

---

### R7: Channel Abstraction for Testability

**Decision**: `ISshChannel` interface abstracting FxSsh channel operations

**Rationale** (from FR-014, FR-015):
- Enables testing without actual SSH connections
- Allows future library changes without API impact
- Same pattern used in web frameworks (IHttpContext, etc.)

**Interface Design**:
```csharp
public interface ISshChannel
{
    void Write(string data);
    void Close();
    string GetTerminalType();
    (int Width, int Height) GetTerminalSize();
    Encoding GetEncoding();
    void SetLineMode(bool enabled);  // No-op for SSH (FxSsh has no line mode)
}
```

---

### R8: Host Key Management

**Decision**: `StrokeSshServer` constructor accepts host key path or PEM string

**FxSsh Pattern**:
```csharp
sshServer.AddHostKey("rsa-sha2-256", rsaPrivateKeyPem);
sshServer.AddHostKey("ecdsa-sha2-nistp256", ecdsaPrivateKeyPem);
```

**Key Generation Utility** (for examples):
```csharp
var rsaKey = FxSsh.KeyGenerator.GenerateRsaKeyPem(2048);
```

---

### R9: Integration Test Strategy

**Decision**: Use SSH.NET as client for integration tests (since it's a well-tested SSH client)

**Pattern**:
```csharp
// Server side
var server = new StrokeSshServer(interact: async session => { ... });
await server.RunAsync(readyCallback: () => serverReady.Set());

// Client side (using SSH.NET)
using var client = new SshClient("127.0.0.1", port, "user", "password");
client.Connect();
using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
```

**Rationale**: SSH.NET is MIT-licensed, well-maintained, and provides robust client functionality for testing.

---

## Summary

All research questions resolved. Key decisions:
1. **FxSsh** for SSH server implementation (MIT, cross-platform, server-focused)
2. **Event-driven architecture** mapping FxSsh events to Python PTK patterns
3. **ISshChannel abstraction** for testability
4. **Virtual methods** for authentication/session extensibility
5. **SSH.NET** as test client for integration testing
