using System.Collections.Concurrent;
using System.Text;
using Renci.SshNet;
using Stroke.Contrib.Ssh;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Concurrency and thread-safety tests for <see cref="StrokeSshServer"/>.
/// Tests for Phase 8: Concurrency & Stress Testing.
/// </summary>
[Collection("SSH Server Tests")]
public class SshServerConcurrencyTests : IAsyncLifetime
{
    private string _hostKeyPath = string.Empty;
    private int _testPort;
    private CancellationTokenSource? _serverCts;
    private Task? _serverTask;
    private StrokeSshServer? _server;

    public ValueTask InitializeAsync()
    {
        _hostKeyPath = Path.Combine(Path.GetTempPath(), $"stroke_test_key_{Guid.NewGuid()}.pem");
        GenerateRsaHostKey(_hostKeyPath);
        _testPort = GetAvailablePort();
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
            _serverCts.Dispose();
        }

        if (_serverTask != null)
        {
            try { await _serverTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (OperationCanceledException) { }
            catch (TimeoutException) { }
        }

        if (File.Exists(_hostKeyPath))
        {
            try { File.Delete(_hostKeyPath); } catch { }
        }
    }

    #region Concurrent Connections Tests (T039)

    [Fact]
    public async Task ConcurrentConnections_MultipleSessions_NoFailures()
    {
        // T039: Verify multiple concurrent sessions work without failures
        // Using 5 concurrent sessions for reasonable test duration
        const int sessionCount = 5;
        var ct = TestContext.Current.CancellationToken;
        var sessionsStarted = new ConcurrentBag<StrokeSshSession>();
        var allSessionsStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                sessionsStarted.Add(session);
                if (sessionsStarted.Count >= sessionCount)
                {
                    allSessionsStarted.TrySetResult(true);
                }
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(15));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            // Connect sessions concurrently
            var connectionTasks = Enumerable.Range(0, sessionCount).Select(_ => Task.Run(() =>
            {
                var client = CreateSshClient();
                client.Connect();
                var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                return (client, shell);
            })).ToList();

            var connections = await Task.WhenAll(connectionTasks);

            // Wait for all sessions to start
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));
            await Task.WhenAny(allSessionsStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            // Verify all sessions started
            Assert.Equal(sessionCount, sessionsStarted.Count);
            Assert.Equal(sessionCount, _server.Connections.Count);

            // All sessions should have unique references
            var uniqueSessions = sessionsStarted.Distinct().ToList();
            Assert.Equal(sessionCount, uniqueSessions.Count);

            // Cleanup
            canFinish.TrySetResult(true);
            foreach (var (client, shell) in connections)
            {
                shell.Dispose();
                client.Disconnect();
                client.Dispose();
            }
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    [Fact]
    public async Task RapidConnectDisconnect_MultipleCycles_NoLeaks()
    {
        // T039: Verify rapid connect/disconnect cycles don't cause leaks
        // Using 10 cycles for reasonable test duration
        const int cycles = 10;
        var ct = TestContext.Current.CancellationToken;

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                // Short-lived session
                await Task.Delay(50, ct);
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            for (int i = 0; i < cycles; i++)
            {
                using var client = CreateSshClient();
                client.Connect();
                using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                // Brief interaction
                await Task.Delay(100, ct);

                client.Disconnect();

                // Allow cleanup
                await Task.Delay(50, ct);
            }

            // After all cycles, no connections should remain
            // Give a bit more time for final cleanup
            await Task.Delay(500, ct);
            Assert.Empty(_server.Connections);
        }
        finally
        {
            await StopServerAsync();
        }
    }

    [Fact]
    public async Task MixedOperations_ConcurrentSendAndResize_ThreadSafe()
    {
        // T039: Verify concurrent data send and resize operations are thread-safe
        var ct = TestContext.Current.CancellationToken;
        StrokeSshSession? capturedSession = null;
        var sessionStarted = new TaskCompletionSource<bool>();
        var canFinish = new TaskCompletionSource<bool>();
        var exceptions = new ConcurrentBag<Exception>();

        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: async session =>
            {
                await Task.Yield();
                capturedSession = session;
                sessionStarted.TrySetResult(true);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(10));
                try { await canFinish.Task.WaitAsync(linkedCts.Token); } catch { }
            },
            hostKeyPath: _hostKeyPath);

        await StartServerAsync(ct);

        try
        {
            using var client = CreateSshClient();
            client.Connect();
            using var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await Task.WhenAny(sessionStarted.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

            Assert.NotNull(capturedSession);

            // Concurrent operations: resize from multiple threads
            var resizeTasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 10; j++)
                    {
                        capturedSession.TerminalSizeChanged(80 + i, 24 + j);
                        var size = capturedSession.GetSize();
                        // Size should always be valid
                        Assert.True(size.Columns >= 1 && size.Columns <= 500);
                        Assert.True(size.Rows >= 1 && size.Rows <= 500);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            })).ToList();

            await Task.WhenAll(resizeTasks);

            // No exceptions should have occurred
            Assert.Empty(exceptions);

            canFinish.TrySetResult(true);
            client.Disconnect();
        }
        finally
        {
            canFinish.TrySetResult(true);
            await StopServerAsync();
        }
    }

    #endregion

    #region Thread-Safety Verification (T041-T042)

    [Fact]
    public void Connections_UsesConcurrentDictionary()
    {
        // T041: Verify Connections uses thread-safe collection
        _server = new StrokeSshServer(
            host: "127.0.0.1",
            port: _testPort,
            interact: _ => Task.CompletedTask,
            hostKeyPath: _hostKeyPath);

        // Connections property should return a snapshot that is safe to iterate
        var connections = _server.Connections;
        Assert.NotNull(connections);
        Assert.IsType<HashSet<StrokeSshSession>>(connections);
    }

    [Fact]
    public async Task Session_LockProtectsMutableState()
    {
        // T042: Verify session uses Lock for mutable state
        // This is a structural test - verify the pattern is used
        var channel = new TestSshChannel();
        var session = new StrokeSshSession(
            channel,
            _ => Task.CompletedTask,
            enableCpr: true);

        // Concurrent access should not throw
        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(() =>
        {
            session.TerminalSizeChanged(80, 24);
            var size = session.GetSize();
            Assert.True(size.Columns > 0);
        })).ToList();

        await Task.WhenAll(tasks);
    }

    #endregion

    #region Helper Methods

    private async Task StartServerAsync(CancellationToken ct)
    {
        _serverCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var readyTcs = new TaskCompletionSource<bool>();

        _serverTask = _server!.RunAsync(
            readyCallback: () => readyTcs.TrySetResult(true),
            cancellationToken: _serverCts.Token);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
        await Task.WhenAny(readyTcs.Task, Task.Delay(Timeout.Infinite, timeoutCts.Token));

        await Task.Delay(100, ct);
    }

    private async Task StopServerAsync()
    {
        if (_serverCts != null)
        {
            await _serverCts.CancelAsync();
        }

        if (_serverTask != null)
        {
            try { await _serverTask.WaitAsync(TimeSpan.FromSeconds(5)); }
            catch (OperationCanceledException) { }
            catch (TimeoutException) { }
        }
    }

    private SshClient CreateSshClient()
    {
        var connectionInfo = new ConnectionInfo(
            "127.0.0.1", _testPort, "testuser",
            new PasswordAuthenticationMethod("testuser", ""));
        return new SshClient(connectionInfo);
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(
            System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void GenerateRsaHostKey(string path)
    {
        const string rsaKey = @"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQClBPKsmiTxCoez
E4Wt4nvNDVjLtkGQPUS/SbhJCL8J7pKrvsKRyXKM9GjGdxA7GKZR7sjqCWCU12oD
+EJuf/mpcA0JsBY4gUU8bp95U5mEM+ZOr2aNXYXAXy/J6GQRyd1jgkD2pm1qsWZJ
ahvXNJGMgx1lqI9woKU+PRssMtv1YupAxsSlo+xcfvC57fkaKIwUeCr6CkUlpIJN
zP5lvatKcpmjiWLSRHUpypx2wgZPz4SnB2qE77UH/yu1DlcsQgbVa7nr+qMWfxP7
Cpa+eTJC19c1GP+XxFaeqXDtuGaRlfBX/iihEcBnQuvsZJLg5jN0WKqvnpDfHgh/
M4IopC2pAgMBAAECggEBAImt6ibV6NJvHa7sL9FXMEFxzE8SffsxEyWiBS5yLKnF
sfu3CbEG6RrvZGeJuTIFK+caGekh78HfRGWRgSOehJe4lDgsAS4dtL1p8oYQmPnz
L0khEKgLimdpQ37q9GrfCGZYq4jebFXjMts3u4i/JFyenC1QCHVIovWdmAk1Wc2N
5bY+qlQZ4dSBl15cqNg0w4bbEF+yT+fOMm/raZANTr/IDIt88LcKpyimvUSGlZEE
OWx4hXAPbF4h/tqeH4O+Xzmtq/1pWwdbwNqWwOXow320c97U4ofCuDXcy0TeOwiX
gcPnaG99jX4Cy+IwdcVnDsJpN4FC2/sm1kGeOTRpIl0CgYEAy8gPV5RESpOyQ00j
dr36JQoymLwXDS114tuMPF7dX5YX3S+yyhl0ADa11tVH15CzOaVSF1wfxDeb1TRs
XcJoZBsxlH24BMPETB1ADy43pslRrkex54hcM4jDe3OYBTsVmV5A2sdxFGEKAbPn
uWsk8jeZ9AsEV3M2vinzJmS5XVsCgYEAz04dSW0GXiTBESYmno9DJiyx3dT4T0eq
bwtpZPCShEjU4BChwFg9V5fAmzw1iCrdYD68mwcxQYurp3Vgqo6u1YogRpeNfljq
VpKnVDbd3a1CTYYyWw81f4HzflpmWLgq1BGKkdwD83xZaFh7Y46cm+xEtrJpiVFM
GTagAokFvEsCgYB1EouV4g1V1wJ73c45Aq26J+CnlK+dl3d5jG5FpK6DosQ1A5kw
uGzHTqcrND7g3jXJMWw3FWr+nH//fe2f8/drQ6A5UfytaBbXL5rE3eWFAXXWrUPM
468swC6mNuOoZahkAx05U4lojtNj5QqEoMSKD114MfgdkYhquckCTq2brwKBgQC5
s1zS0II6xSvZw9YmhWj+gl0WvVduFWGcNZnE3SgyrddbnCp5VdIlbAASTx4ZC2Th
eXGUYh4CfC5ZRPFB96ywBxqggdQzEU1iHd8ctkWK9VCGh6cGIRqoTO2lCy/RW7Cp
5ci+nls/uu2QZmqppS+vETgAfNPDOXs0vtUZUEs9/wKBgDNQonVvTTQIRbaRbxXu
eVqxAVYBb8PSPBjfigb4/sGzu4iYaxuCHOkA8AK9B9SmGjaQHJ4h9t+kJKe9xNie
v7sG5pguzUyd+AJIafbeh2Iryva/Nw3Shb7Jl6EX/lX3o/B9hRziWKV0IvwCUF/1
iyxhUEyZT7ugi8eNl5zVJgmN
-----END PRIVATE KEY-----";
        File.WriteAllText(path, rsaKey);
    }

    #endregion

    #region Test Doubles

    private class TestSshChannel : ISshChannel
    {
        public bool IsClosed { get; private set; }

        public void Write(string data) { }
        public void Close() => IsClosed = true;
        public string GetTerminalType() => "xterm";
        public (int Width, int Height) GetTerminalSize() => (80, 24);
        public Encoding GetEncoding() => Encoding.UTF8;
        public void SetLineMode(bool enabled) { }
    }

    #endregion
}
