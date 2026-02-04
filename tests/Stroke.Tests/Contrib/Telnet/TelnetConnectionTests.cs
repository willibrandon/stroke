namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using System.Text;
using Stroke.Contrib.Telnet;
using Stroke.Core.Primitives;
using Stroke.FormattedText;
using Stroke.Input.Pipe;
using Stroke.Styles;
using Xunit;

/// <summary>
/// Unit tests for <see cref="TelnetConnection"/>.
/// </summary>
public class TelnetConnectionTests : IDisposable
{
    private Socket? _serverSocket;
    private Socket? _clientSocket;
    private Socket? _acceptedSocket;
    private TelnetServer? _server;

    public void Dispose()
    {
        _acceptedSocket?.Dispose();
        _clientSocket?.Dispose();
        _serverSocket?.Dispose();
    }

    #region Property Tests

    [Fact]
    public void Size_DefaultsTo80x24()
    {
        var (connection, _) = CreateConnectionPair();

        Assert.Equal(new Size(24, 80), connection.Size);
    }

    [Fact]
    public void IsClosed_InitiallyFalse()
    {
        var (connection, _) = CreateConnectionPair();

        Assert.False(connection.IsClosed);
    }

    [Fact]
    public void Socket_ReturnsProvidedSocket()
    {
        var (connection, acceptedSocket) = CreateConnectionPair();

        Assert.Same(acceptedSocket, connection.Socket);
    }

    [Fact]
    public void RemoteAddress_ReturnsClientEndPoint()
    {
        var (connection, _) = CreateConnectionPair();

        Assert.NotNull(connection.RemoteAddress);
        Assert.Equal(IPAddress.Loopback, connection.RemoteAddress.Address);
    }

    [Fact]
    public void Server_ReturnsProvidedServer()
    {
        var (connection, _) = CreateConnectionPair();

        Assert.Same(_server, connection.Server);
    }

    [Fact]
    public void Encoding_ReturnsProvidedEncoding()
    {
        var (connection, _) = CreateConnectionPair();

        Assert.Equal(Encoding.UTF8, connection.Encoding);
    }

    [Fact]
    public void Style_ReturnsProvidedStyle()
    {
        var style = DummyStyle.Instance;
        var (connection, _) = CreateConnectionPair(style: style);

        Assert.Same(style, connection.Style);
    }

    [Fact]
    public void EnableCpr_ReturnsProvidedValue()
    {
        var (connectionWithCpr, _) = CreateConnectionPair(enableCpr: true);
        var (connectionWithoutCpr, _) = CreateConnectionPair(enableCpr: false);

        Assert.True(connectionWithCpr.EnableCpr);
        Assert.False(connectionWithoutCpr.EnableCpr);
    }

    #endregion

    #region Close Tests

    [Fact]
    public void Close_SetsIsClosedTrue()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();

        Assert.True(connection.IsClosed);
    }

    [Fact]
    public void Close_IsIdempotent()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();
        connection.Close();
        connection.Close();

        Assert.True(connection.IsClosed);
    }

    [Fact]
    public void Close_ClosesSocket()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();

        // Socket should be disconnected
        Assert.False(connection.Socket.Connected);
    }

    #endregion

    #region Send Tests

    [Fact]
    public void Send_OnClosedConnection_IsNoOp()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();

        // Should not throw
        var ex = Record.Exception(() => connection.Send("Hello"));
        Assert.Null(ex);
    }

    [Fact]
    public void Send_BeforeReady_IsNoOp()
    {
        // Connection is not ready until TTYPE is received
        var (connection, _) = CreateConnectionPair();

        // Should not throw - output is not initialized yet
        var ex = Record.Exception(() => connection.Send("Hello"));
        Assert.Null(ex);
    }

    #endregion

    #region EraseScreen Tests

    [Fact]
    public void EraseScreen_OnClosedConnection_IsNoOp()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();

        // Should not throw
        var ex = Record.Exception(() => connection.EraseScreen());
        Assert.Null(ex);
    }

    [Fact]
    public void EraseScreen_BeforeReady_IsNoOp()
    {
        var (connection, _) = CreateConnectionPair();

        // Should not throw - output is not initialized yet
        var ex = Record.Exception(() => connection.EraseScreen());
        Assert.Null(ex);
    }

    #endregion

    #region Feed Tests

    [Fact]
    public void Feed_OnClosedConnection_IsNoOp()
    {
        var (connection, _) = CreateConnectionPair();

        connection.Close();

        // Should not throw
        var ex = Record.Exception(() => connection.Feed([0x41, 0x42, 0x43]));
        Assert.Null(ex);
    }

    [Fact]
    public void Feed_WithNawsData_UpdatesSize()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS subnegotiation: IAC SB NAWS <width-hi> <width-lo> <height-hi> <height-lo> IAC SE
        // Width: 120 (0x0078), Height: 40 (0x0028)
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x78, // width = 120
            0x00, 0x28, // height = 40
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Size should be updated (note: Size is rows x columns)
        Assert.Equal(new Size(40, 120), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsZeroValues_ClampsToOne()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS with 0x0 dimensions
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x00, // width = 0
            0x00, 0x00, // height = 0
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Should be clamped to 1x1 (EC-009)
        Assert.Equal(new Size(1, 1), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsLargeValues_ClampsTo500()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS with large dimensions (1000x1000)
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x03, 0xE8, // width = 1000
            0x03, 0xE8, // height = 1000
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Should be clamped to 500x500 (EC-004)
        Assert.Equal(new Size(500, 500), connection.Size);
    }

    #endregion

    #region SendAbovePrompt Tests

    [Fact]
    public void SendAbovePrompt_WithoutAppContext_ThrowsInvalidOperationException()
    {
        var (connection, _) = CreateConnectionPair();

        // Trigger TTYPE to make connection ready
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Should throw because no app context is active
        Assert.Throws<InvalidOperationException>(() => connection.SendAbovePrompt("Test"));
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Close_FromMultipleThreads_IsThreadSafe()
    {
        var (connection, _) = CreateConnectionPair();
        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        // Close from multiple threads concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(connection.Close, ct));
        }

        await Task.WhenAll(tasks);

        Assert.True(connection.IsClosed);
    }

    [Fact]
    public async Task Send_FromMultipleThreads_IsThreadSafe()
    {
        var (connection, _) = CreateConnectionPair();
        var ct = TestContext.Current.CancellationToken;
        var tasks = new List<Task>();

        // Trigger TTYPE to make connection ready
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.TTYPE,
            TelnetConstants.IS,
            (byte)'V', (byte)'T', (byte)'1', (byte)'0', (byte)'0',
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Send from multiple threads concurrently
        for (int i = 0; i < 10; i++)
        {
            var message = $"Message {i}";
            tasks.Add(Task.Run(() => connection.Send(message), ct));
        }

        // Should not throw
        await Task.WhenAll(tasks);
    }

    #endregion

    #region Helper Methods

    private (TelnetConnection connection, Socket acceptedSocket) CreateConnectionPair(
        IStyle? style = null,
        bool enableCpr = true)
    {
        // Create socket pair
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _serverSocket.Listen(1);

        var port = ((IPEndPoint)_serverSocket.LocalEndPoint!).Port;

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _clientSocket.Connect(IPAddress.Loopback, port);

        _acceptedSocket = _serverSocket.Accept();

        // Create server instance
        _server = new TelnetServer(port: port);

        // Create pipe input
        var pipeInput = new SimplePipeInput();

        // Create connection
        var connection = new TelnetConnection(
            _acceptedSocket,
            (IPEndPoint)_clientSocket.LocalEndPoint!,
            _ => Task.CompletedTask,
            _server,
            Encoding.UTF8,
            style,
            pipeInput,
            enableCpr);

        return (connection, _acceptedSocket);
    }

    #endregion
}
