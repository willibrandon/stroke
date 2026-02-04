namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Tests for concurrent connection handling in <see cref="TelnetServer"/>.
/// </summary>
public class TelnetServerConcurrencyTests : IAsyncLifetime
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

    #region Multiple Connection Tests

    [Fact]
    public async Task Server_AcceptsMultipleConnections()
    {
        var ct = TestContext.Current.CancellationToken;
        var connectionCount = 0;
        var connectionTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                Interlocked.Increment(ref connectionCount);
                if (connectionCount >= 5)
                {
                    connectionTcs.TrySetResult(true);
                }
                await Task.Delay(500, ct);
            });

        await StartServerAsync();

        // Connect 5 clients
        var clients = new List<TcpClient>();
        for (int i = 0; i < 5; i++)
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, _port, ct);
            clients.Add(client);

            // Send TTYPE to trigger interact callback
            var stream = client.GetStream();
            await Task.Delay(20, ct);
            await stream.WriteAsync(CreateTtypeResponse(), ct);
        }

        // Wait for all connections to invoke interact
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await connectionTcs.Task.WaitAsync(linkedCts.Token);

        Assert.Equal(5, connectionCount);

        // Cleanup
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    [Fact]
    public async Task Server_Handles50ConcurrentConnections()
    {
        // SC-001: Support 50+ concurrent connections
        var ct = TestContext.Current.CancellationToken;
        var connectionCount = 0;
        var allConnectedTcs = new TaskCompletionSource<bool>();
        const int targetConnections = 50;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                var count = Interlocked.Increment(ref connectionCount);
                if (count >= targetConnections)
                {
                    allConnectedTcs.TrySetResult(true);
                }
                await Task.Delay(1000, ct);
            });

        await StartServerAsync();

        // Connect 50 clients concurrently
        var clients = new List<TcpClient>();
        var connectTasks = new List<Task>();

        for (int i = 0; i < targetConnections; i++)
        {
            connectTasks.Add(Task.Run(async () =>
            {
                var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, _port, ct);
                lock (clients)
                {
                    clients.Add(client);
                }

                var stream = client.GetStream();
                await Task.Delay(10, ct);
                await stream.WriteAsync(CreateTtypeResponse(), ct);
            }, ct));
        }

        await Task.WhenAll(connectTasks);

        // Wait for all connections to invoke interact
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(10));
        await allConnectedTcs.Task.WaitAsync(linkedCts.Token);

        Assert.Equal(targetConnections, connectionCount);

        // Cleanup
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    #endregion

    #region Connection Isolation Tests (ISO-001/002/003)

    [Fact]
    public async Task Server_ConnectionsAreIsolated()
    {
        var ct = TestContext.Current.CancellationToken;
        var connections = new List<TelnetConnection>();
        var allConnectedTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                lock (connections)
                {
                    connections.Add(conn);
                    if (connections.Count >= 3)
                    {
                        allConnectedTcs.TrySetResult(true);
                    }
                }
                await Task.Delay(500, ct);
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

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await allConnectedTcs.Task.WaitAsync(linkedCts.Token);

        // Verify each connection is a distinct object
        Assert.Equal(3, connections.Count);
        Assert.NotSame(connections[0], connections[1]);
        Assert.NotSame(connections[1], connections[2]);
        Assert.NotSame(connections[0], connections[2]);

        // Each connection should have its own socket
        Assert.NotSame(connections[0].Socket, connections[1].Socket);
        Assert.NotSame(connections[1].Socket, connections[2].Socket);

        // Cleanup
        foreach (var client in clients)
        {
            client.Dispose();
        }
    }

    #endregion

    #region Connections Enumeration Tests (TS-008)

    [Fact]
    public async Task Connections_EnumerationDuringModification_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;
        var interactStartedTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                interactStartedTcs.TrySetResult(true);
                await Task.Delay(500, ct);
            });

        await StartServerAsync();

        // Connect a client
        using var client1 = new TcpClient();
        await client1.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream1 = client1.GetStream();
        await Task.Delay(20, ct);
        await stream1.WriteAsync(CreateTtypeResponse(), ct);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await interactStartedTcs.Task.WaitAsync(linkedCts.Token);

        // Enumerate connections while adding new one
        var enumerationTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (var conn in _server!.Connections)
                {
                    _ = conn.Size; // Access something to force enumeration
                }
            }
        }, ct);

        var addTask = Task.Run(async () =>
        {
            using var client2 = new TcpClient();
            await client2.ConnectAsync(IPAddress.Loopback, _port, ct);
            var stream2 = client2.GetStream();
            await Task.Delay(20, ct);
            await stream2.WriteAsync(CreateTtypeResponse(), ct);
            await Task.Delay(100, ct);
        }, ct);

        // Should not throw
        await Task.WhenAll(enumerationTask, addTask);
    }

    [Fact]
    public async Task Connections_ReturnsSnapshot()
    {
        var ct = TestContext.Current.CancellationToken;
        var interactTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                interactTcs.TrySetResult(true);
                await Task.Delay(500, ct);
            });

        await StartServerAsync();

        // Connect a client
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await interactTcs.Task.WaitAsync(linkedCts.Token);

        // Get two snapshots
        var snapshot1 = _server!.Connections;
        var snapshot2 = _server.Connections;

        // Should be different instances (snapshots)
        Assert.NotSame(snapshot1, snapshot2);

        // But should contain the same connection
        Assert.Single(snapshot1);
        Assert.Single(snapshot2);
    }

    #endregion

    #region Concurrent Send Tests (TS-002)

    [Fact]
    public async Task Send_ConcurrentCalls_DoNotCorrupt()
    {
        var ct = TestContext.Current.CancellationToken;
        TelnetConnection? connection = null;
        var connectionReadyTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                connection = conn;
                connectionReadyTcs.TrySetResult(true);
                await Task.Delay(2000, ct); // Keep connection alive
            });

        await StartServerAsync();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);
        var stream = client.GetStream();
        await Task.Delay(20, ct);
        await stream.WriteAsync(CreateTtypeResponse(), ct);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await connectionReadyTcs.Task.WaitAsync(linkedCts.Token);

        // Send from multiple threads concurrently
        var sendTasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var msg = $"Message {i}";
            sendTasks.Add(Task.Run(() => connection!.Send(msg), ct));
        }

        // Should not throw
        await Task.WhenAll(sendTasks);
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
