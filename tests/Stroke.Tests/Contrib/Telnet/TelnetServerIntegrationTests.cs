namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Integration tests for <see cref="TelnetServer"/>.
/// Tests end-to-end functionality with real socket connections.
/// </summary>
public class TelnetServerIntegrationTests : IAsyncLifetime
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
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await _serverTask.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    #region Server Startup Tests

    [Fact]
    public async Task RunAsync_StartsAndAcceptsConnection()
    {
        await StartServerAsync();
        var ct = TestContext.Current.CancellationToken;

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        Assert.True(client.Connected);
    }

    [Fact]
    public async Task RunAsync_InvokesReadyCallback()
    {
        var readyCalled = false;
        var readyTcs = new TaskCompletionSource<bool>();

        _server = new TelnetServer(port: 0);
        _serverCts = new CancellationTokenSource();

        _serverTask = _server.RunAsync(
            readyCallback: () =>
            {
                readyCalled = true;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        // Wait for ready callback with timeout
        var ct = TestContext.Current.CancellationToken;
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromMilliseconds(100));
        try
        {
            await readyTcs.Task.WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Timeout - check state anyway
        }

        Assert.True(readyCalled);
    }

    [Fact]
    public async Task RunAsync_ReadyCallback_CompletesUnder100ms()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var readyTcs = new TaskCompletionSource<bool>();
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(port: 0);
        _serverCts = new CancellationTokenSource();

        _serverTask = _server.RunAsync(
            readyCallback: () => readyTcs.TrySetResult(true),
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);
        stopwatch.Stop();

        // SC-005: Server startup should complete under 100ms
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Server startup took {stopwatch.ElapsedMilliseconds}ms, expected <100ms");
    }

    [Fact]
    public async Task RunAsync_WithPort0_AssignsActualPort()
    {
        var readyTcs = new TaskCompletionSource<bool>();
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(port: 0);
        _serverCts = new CancellationTokenSource();

        _serverTask = _server.RunAsync(
            readyCallback: () =>
            {
                _port = _server.Port;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);

        Assert.NotEqual(0, _server.Port);
        Assert.InRange(_server.Port, 1, 65535);
    }

    #endregion

    #region Initialization Sequence Tests

    [Fact]
    public async Task Server_SendsInitializationSequences()
    {
        await StartServerAsync();
        var ct = TestContext.Current.CancellationToken;

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        var stream = client.GetStream();
        var buffer = new byte[256];
        var totalRead = 0;

        // Read with timeout
        stream.ReadTimeout = 500;
        try
        {
            while (totalRead < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(totalRead), ct);
                if (read == 0) break;
                totalRead += read;
            }
        }
        catch (IOException)
        {
            // Timeout - that's expected after reading initial sequences
        }

        // Verify initialization sequences were sent (FR-002)
        var data = buffer.AsSpan(0, totalRead);

        // Check for IAC DO LINEMODE (255, 253, 34)
        Assert.True(ContainsSequence(data, [TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.LINEMODE]),
            "Missing IAC DO LINEMODE");

        // Check for IAC WILL SUPPRESS_GO_AHEAD (255, 251, 3)
        Assert.True(ContainsSequence(data, [TelnetConstants.IAC, TelnetConstants.WILL, TelnetConstants.SUPPRESS_GO_AHEAD]),
            "Missing IAC WILL SUPPRESS_GO_AHEAD");

        // Check for IAC WILL ECHO (255, 251, 1)
        Assert.True(ContainsSequence(data, [TelnetConstants.IAC, TelnetConstants.WILL, TelnetConstants.ECHO]),
            "Missing IAC WILL ECHO");

        // Check for IAC DO NAWS (255, 253, 31)
        Assert.True(ContainsSequence(data, [TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.NAWS]),
            "Missing IAC DO NAWS");

        // Check for IAC DO TTYPE (255, 253, 24)
        Assert.True(ContainsSequence(data, [TelnetConstants.IAC, TelnetConstants.DO, TelnetConstants.TTYPE]),
            "Missing IAC DO TTYPE");
    }

    #endregion

    #region Interact Callback Tests

    [Fact]
    public async Task Server_InvokesInteractCallback()
    {
        var interactCalled = false;
        var interactTcs = new TaskCompletionSource<bool>();
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                interactCalled = true;
                interactTcs.TrySetResult(true);
                await Task.Delay(100, ct); // Keep connection alive briefly
            });

        _serverCts = new CancellationTokenSource();
        var readyTcs = new TaskCompletionSource<bool>();

        _serverTask = _server.RunAsync(
            readyCallback: () =>
            {
                _port = _server.Port;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        // Send TTYPE response to trigger connection ready state
        var stream = client.GetStream();

        // Wait a moment for init sequences
        await Task.Delay(50, ct);

        // Send TTYPE response: IAC SB TTYPE IS VT100 IAC SE
        var ttypeResponse = new byte[]
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        };
        await stream.WriteAsync(ttypeResponse, ct);

        // Wait for interact callback with timeout
        await interactTcs.Task.WaitAsync(linkedCts.Token);

        Assert.True(interactCalled);
    }

    [Fact]
    public async Task Server_AddsConnectionToConnectionsSet()
    {
        var connectionAddedTcs = new TaskCompletionSource<TelnetConnection>();
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(
            port: 0,
            interact: async conn =>
            {
                connectionAddedTcs.TrySetResult(conn);
                await Task.Delay(100, ct); // Keep connection alive briefly
            });

        _serverCts = new CancellationTokenSource();
        var readyTcs = new TaskCompletionSource<bool>();

        _serverTask = _server.RunAsync(
            readyCallback: () =>
            {
                _port = _server.Port;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        // Send TTYPE to trigger ready
        var stream = client.GetStream();
        await Task.Delay(50, ct);

        var ttypeResponse = new byte[]
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        };
        await stream.WriteAsync(ttypeResponse, ct);

        var connection = await connectionAddedTcs.Task.WaitAsync(linkedCts.Token);

        // Connection should be in the Connections set
        Assert.Contains(connection, _server.Connections);
    }

    #endregion

    #region Input/Output Roundtrip Tests

    [Fact]
    public async Task Server_ReceivesDataFromClient()
    {
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(
            port: 0,
            interact: async _ =>
            {
                // Connection receives data through its Feed method
                // We'll just keep it alive while client sends data
                await Task.Delay(500, ct);
            });

        _serverCts = new CancellationTokenSource();
        var readyTcs = new TaskCompletionSource<bool>();

        _serverTask = _server.RunAsync(
            readyCallback: () =>
            {
                _port = _server.Port;
                readyTcs.TrySetResult(true);
            },
            cancellationToken: _serverCts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        await readyTcs.Task.WaitAsync(linkedCts.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        var stream = client.GetStream();
        await Task.Delay(50, ct);

        // Send TTYPE first
        var ttypeResponse = new byte[]
        {
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        };
        await stream.WriteAsync(ttypeResponse, ct);

        // Send some data
        var testData = "Hello, Telnet!"u8.ToArray();
        await stream.WriteAsync(testData, ct);

        // Connection should receive and process the data
        // (verification is implicit - if server doesn't crash, it handled the data)
        await Task.Delay(100, ct);
    }

    #endregion

    #region Shutdown Tests

    [Fact]
    public async Task RunAsync_CancellationStopsServer()
    {
        await StartServerAsync();
        var ct = TestContext.Current.CancellationToken;

        // Connect a client
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, _port, ct);

        // Cancel the server
        await _serverCts!.CancelAsync();

        // Wait for server to stop
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            await _serverTask!.WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Server task should complete
        Assert.True(_serverTask!.IsCompleted);
    }

    #endregion

    #region Helper Methods

    private async Task StartServerAsync()
    {
        var readyTcs = new TaskCompletionSource<bool>();
        var ct = TestContext.Current.CancellationToken;

        _server = new TelnetServer(port: 0);
        _serverCts = new CancellationTokenSource();

        _serverTask = _server.RunAsync(
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

    private static bool ContainsSequence(ReadOnlySpan<byte> data, ReadOnlySpan<byte> sequence)
    {
        if (sequence.Length > data.Length) return false;

        for (int i = 0; i <= data.Length - sequence.Length; i++)
        {
            if (data.Slice(i, sequence.Length).SequenceEqual(sequence))
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
