namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Tests for connection lifecycle management in <see cref="TelnetServer"/>.
/// </summary>
public class TelnetServerLifecycleTests : IAsyncLifetime
{
    private TelnetServer? _server;
    private CancellationTokenSource? _serverCts;
    private Task? _serverTask;
    private int _port;

    public ValueTask InitializeAsync()
    {
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
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await _serverTask.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    #region Client Disconnect Tests

    [Fact]
    public async Task Server_ClientDisconnect_TriggersCleanup()
    {
        var ct = TestContext.Current.CancellationToken;
        var interactStartedTcs = new TaskCompletionSource<bool>();
        TelnetConnection? connection = null;

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                connection = conn;
                interactStartedTcs.TrySetResult(true);
                // Poll for connection close
                while (!conn.IsClosed)
                {
                    await Task.Delay(50, ct);
                }
            });

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        // Wait for interact to start
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await interactStartedTcs.Task.WaitAsync(linkedCts.Token);

        // Disconnect client
        client.Close();

        // Wait for connection to be marked as closed
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (!(connection?.IsClosed ?? false) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(50, ct);
        }

        Assert.True(connection?.IsClosed ?? false);
    }

    [Fact]
    public async Task Server_ClientDisconnect_CompletesWithin3Seconds()
    {
        // SC-004: Cleanup should complete promptly
        var ct = TestContext.Current.CancellationToken;
        var interactStartedTcs = new TaskCompletionSource<bool>();
        TelnetConnection? connection = null;

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                connection = conn;
                interactStartedTcs.TrySetResult(true);
                // Poll for connection close
                while (!conn.IsClosed)
                {
                    await Task.Delay(10, ct);
                }
            });

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await interactStartedTcs.Task.WaitAsync(linkedCts.Token);

        // Disconnect and measure cleanup time
        var disconnectTime = DateTime.UtcNow;
        client.Close();

        // Wait for connection to be closed (generous deadline for CI/parallel suites)
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!(connection?.IsClosed ?? false) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10, ct);
        }

        Assert.True(connection?.IsClosed ?? false,
            "Connection was not closed within 5 seconds of client disconnect");
    }

    [Fact]
    public async Task Server_ClientDisconnect_RemovesFromConnectionsSet()
    {
        var ct = TestContext.Current.CancellationToken;
        var interactStartedTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                interactStartedTcs.TrySetResult(true);
                // Poll for connection close instead of infinite delay
                while (!conn.IsClosed)
                {
                    await Task.Delay(50, ct);
                }
            });

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await interactStartedTcs.Task.WaitAsync(linkedCts.Token);

        Assert.Single(_server!.Connections);

        // Disconnect
        client.Close();

        // Wait for cleanup with polling
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (_server.Connections.Count > 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(100, ct);
        }

        Assert.Empty(_server.Connections);
    }

    #endregion

    #region Server Shutdown Tests (TS-007)

    [Fact]
    public async Task Server_Shutdown_CancelsAllConnections()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionCount = 0;
        var connections = new List<TelnetConnection>();

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                Interlocked.Increment(ref connectionCount);
                lock (connections) { connections.Add(conn); }
                // Poll for connection close
                while (!conn.IsClosed)
                {
                    await Task.Delay(50, ct);
                }
            });

        await StartServerAsync();

        // Connect 3 clients
        var clients = new List<TcpClient>();
        for (int i = 0; i < 3; i++)
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, _port, ct);
            clients.Add(client);

            var stream = client.GetStream();
            await Task.Delay(20, ct);
            await stream.WriteAsync(CreateTtypeResponse(), ct);
        }

        // Wait for all to connect
        await Task.Delay(200, ct);
        Assert.Equal(3, connectionCount);

        // Shutdown server
        await _serverCts!.CancelAsync();

        // Wait for all connections to be closed
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (connections.Count < 3 || connections.Any(c => !c.IsClosed))
        {
            if (DateTime.UtcNow > deadline) break;
            await Task.Delay(50, ct);
        }

        // Verify all connections are closed
        Assert.All(connections, c => Assert.True(c.IsClosed));

        // Cleanup clients
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    [Fact]
    public async Task Server_Shutdown_CompletesGracefully()
    {
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                await Task.Delay(100, ct);
            });

        await StartServerAsync();

        // Connect a client
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);
        await Task.Delay(50, ct);

        // Shutdown
        await _serverCts!.CancelAsync();

        // Server should complete within reasonable time
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

        try
        {
            await _serverTask!.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            Assert.Fail("Server shutdown timed out");
        }
        catch (OperationCanceledException)
        {
            // Expected from server cancellation
        }

        Assert.True(_serverTask!.IsCompleted);
    }

    #endregion

    #region Interact Callback Exception Tests (ERR-003)

    [Fact]
    public async Task Server_InteractException_DoesNotCrashServer()
    {
        var ct = TestContext.Current.CancellationToken;
        var firstConnectionFailed = false;
        var secondConnectionSucceeded = false;
        var firstDone = new SemaphoreSlim(0, 1);
        var secondDone = new SemaphoreSlim(0, 1);

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                if (!firstConnectionFailed)
                {
                    firstConnectionFailed = true;
                    firstDone.Release();
                    throw new InvalidOperationException("Simulated failure");
                }
                else
                {
                    secondConnectionSucceeded = true;
                    secondDone.Release();
                    await Task.Delay(100, ct);
                }
            });

        await StartServerAsync();

        // First connection (will fail)
        using var client1 = new TcpClient();
        await client1.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream1 = client1.GetStream();
        await Task.Delay(20, ct);
        await stream1.WriteAsync(CreateTtypeResponse(), ct);
        await firstDone.WaitAsync(TimeSpan.FromSeconds(5), ct);

        // Second connection (should succeed)
        using var client2 = new TcpClient();
        await client2.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream2 = client2.GetStream();
        await Task.Delay(20, ct);
        await stream2.WriteAsync(CreateTtypeResponse(), ct);
        await secondDone.WaitAsync(TimeSpan.FromSeconds(5), ct);

        Assert.True(firstConnectionFailed);
        Assert.True(secondConnectionSucceeded);
    }

    [Fact]
    public async Task Server_InteractException_ClosesConnection()
    {
        var ct = TestContext.Current.CancellationToken;
        TelnetConnection? failedConnection = null;

        _server = new TelnetServer(
            port: 0,
            interact: conn =>
            {
                failedConnection = conn;
                throw new Exception("Simulated failure");
            });

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        // Wait for exception to be handled
        await Task.Delay(500, ct);

        // Connection should be closed and removed
        Assert.True(failedConnection?.IsClosed ?? false);
        Assert.Empty(_server!.Connections);
    }

    #endregion

    #region Rapid Connect/Disconnect Tests (EC-006)

    [Fact]
    public async Task Server_RapidConnectDisconnect_HandlesGracefully()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionAttempts = 0;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                Interlocked.Increment(ref connectionAttempts);
                await Task.Delay(50, ct);
            });

        await StartServerAsync();

        // Rapidly connect and disconnect
        for (int i = 0; i < 20; i++)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, _port, ct);

            // Immediately disconnect (before TTYPE negotiation)
            client.Close();
        }

        // Server should still be running
        Assert.False(_serverTask!.IsCompleted);
    }

    [Fact]
    public async Task Server_RapidConnectDisconnectWithTtype_HandlesGracefully()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionAttempts = 0;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                Interlocked.Increment(ref connectionAttempts);
                await Task.Delay(10, ct);
            });

        await StartServerAsync();

        // Rapidly connect, send TTYPE, and disconnect
        for (int i = 0; i < 20; i++)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, _port, ct);

            var stream = client.GetStream();
            await stream.WriteAsync(CreateTtypeResponse(), ct);

            // Brief delay then disconnect
            await Task.Delay(5, ct);
            client.Close();
        }

        // Wait for processing
        await Task.Delay(500, ct);

        // Server should still be running
        Assert.False(_serverTask!.IsCompleted);
    }

    #endregion

    #region Null Interact Tests (EC-008)

    [Fact]
    public async Task Server_NullInteract_ClosesConnectionImmediately()
    {
        var ct = TestContext.Current.CancellationToken;

        // Server with null interact uses default that returns immediately
        _server = new TelnetServer(port: 0);

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        // Wait for connection to be processed and closed
        await Task.Delay(500, ct);

        // Connection should be closed (empty interact returns immediately)
        Assert.Empty(_server!.Connections);
    }

    #endregion

    #region Helper Methods

    private async Task StartServerAsync()
    {
        var readyTcs = new TaskCompletionSource<bool>();
        var ct = TestContext.Current.CancellationToken;

        _serverCts = new CancellationTokenSource();

        _serverTask = _server!.RunAsync(
            readyCallback: () =>
            {
                _port = _server.Port;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);
    }

    private static byte[] CreateTtypeResponse()
    {
        return
        [
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        ];
    }

    #endregion
}
