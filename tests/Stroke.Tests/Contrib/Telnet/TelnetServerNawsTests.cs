namespace Stroke.Tests.Contrib.Telnet;

using System.Net;
using System.Net.Sockets;
using System.Text;
using Stroke.Contrib.Telnet;
using Stroke.Core.Primitives;
using Stroke.Input.Pipe;
using Xunit;

/// <summary>
/// Tests for NAWS (Negotiate About Window Size) functionality in telnet connections.
/// </summary>
public class TelnetServerNawsTests : IDisposable
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

    #region NAWS Parsing Tests

    [Fact]
    public void Feed_WithNaws_ExtractsCorrectDimensions()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=100, height=50
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x64, // width = 100 (0x0064)
            0x00, 0x32, // height = 50 (0x0032)
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        // Size is (rows, columns)
        Assert.Equal(new Size(50, 100), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsLargeWidth_CapsAt500()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=1000, height=50
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x03, 0xE8, // width = 1000
            0x00, 0x32, // height = 50
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(50, 500), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsLargeHeight_CapsAt500()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=100, height=1000
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x64, // width = 100
            0x03, 0xE8, // height = 1000
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(500, 100), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsZeroWidth_TreatsAsOne()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=0, height=50
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x00, // width = 0
            0x00, 0x32, // height = 50
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(50, 1), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsZeroHeight_TreatsAsOne()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=100, height=0
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x64, // width = 100
            0x00, 0x00, // height = 0
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(1, 100), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsBothZero_TreatsAs1x1()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=0, height=0
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x00, // width = 0
            0x00, 0x00, // height = 0
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(1, 1), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsBothMax_CapsAt500x500()
    {
        var (connection, _) = CreateConnectionPair();

        // Send NAWS: width=65535, height=65535 (max uint16)
        // NOTE: Each 0xFF byte must be escaped as 0xFF 0xFF (IAC IAC) in telnet protocol
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0xFF, 0xFF, 0xFF, 0xFF, // width = 65535 (escaped: each 0xFF becomes 0xFF 0xFF)
            0xFF, 0xFF, 0xFF, 0xFF, // height = 65535 (escaped)
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(500, 500), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsTypicalTerminal_WorksCorrectly()
    {
        var (connection, _) = CreateConnectionPair();

        // Typical terminal: 80x24
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x50, // width = 80
            0x00, 0x18, // height = 24
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(24, 80), connection.Size);
    }

    [Fact]
    public void Feed_WithNawsWideTerminal_WorksCorrectly()
    {
        var (connection, _) = CreateConnectionPair();

        // Wide terminal: 200x50
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0xC8, // width = 200
            0x00, 0x32, // height = 50
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(50, 200), connection.Size);
    }

    #endregion

    #region Default Size Tests

    [Fact]
    public void Size_WithoutNaws_DefaultsTo80x24()
    {
        var (connection, _) = CreateConnectionPair();

        // No NAWS sent - should use default
        Assert.Equal(new Size(24, 80), connection.Size);
    }

    #endregion

    #region Multiple NAWS Updates Tests

    [Fact]
    public void Feed_WithMultipleNaws_UpdatesSizeEachTime()
    {
        var (connection, _) = CreateConnectionPair();

        // First NAWS: 80x24
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x50, 0x00, 0x18,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);
        Assert.Equal(new Size(24, 80), connection.Size);

        // Second NAWS: 120x40 (resize)
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x78, 0x00, 0x28,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);
        Assert.Equal(new Size(40, 120), connection.Size);

        // Third NAWS: 200x60 (another resize)
        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0xC8, 0x00, 0x3C,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);
        Assert.Equal(new Size(60, 200), connection.Size);
    }

    #endregion

    #region NAWS with Other Data Tests

    [Fact]
    public void Feed_WithNawsMixedWithData_ParsesBothCorrectly()
    {
        var (connection, _) = CreateConnectionPair();

        // Mix of regular data and NAWS
        connection.Feed([
            (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o',
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            0x00, 0x64, 0x00, 0x32,
            TelnetConstants.IAC, TelnetConstants.SE,
            (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d'
        ]);

        // Size should be updated
        Assert.Equal(new Size(50, 100), connection.Size);
    }

    #endregion

    #region Boundary Value Tests

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 500)]
    [InlineData(500, 1)]
    [InlineData(500, 500)]
    [InlineData(80, 24)]
    [InlineData(132, 43)]
    [InlineData(256, 256)]
    public void Feed_WithNawsValidDimensions_SetsCorrectSize(int width, int height)
    {
        var (connection, _) = CreateConnectionPair();

        var widthHi = (byte)(width >> 8);
        var widthLo = (byte)(width & 0xFF);
        var heightHi = (byte)(height >> 8);
        var heightLo = (byte)(height & 0xFF);

        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            widthHi, widthLo, heightHi, heightLo,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(height, width), connection.Size);
    }

    [Theory]
    [InlineData(501, 100, 500, 100)]
    [InlineData(100, 501, 100, 500)]
    [InlineData(1000, 1000, 500, 500)]
    // Note: 65535 (0xFFFF) values require IAC escaping, tested separately in Feed_WithNawsBothMax_CapsAt500x500
    public void Feed_WithNawsOversizedDimensions_ClampsTo500(
        int inputWidth, int inputHeight, int expectedWidth, int expectedHeight)
    {
        var (connection, _) = CreateConnectionPair();

        var widthHi = (byte)(inputWidth >> 8);
        var widthLo = (byte)(inputWidth & 0xFF);
        var heightHi = (byte)(inputHeight >> 8);
        var heightLo = (byte)(inputHeight & 0xFF);

        connection.Feed([
            TelnetConstants.IAC, TelnetConstants.SB, TelnetConstants.NAWS,
            widthHi, widthLo, heightHi, heightLo,
            TelnetConstants.IAC, TelnetConstants.SE
        ]);

        Assert.Equal(new Size(expectedHeight, expectedWidth), connection.Size);
    }

    #endregion

    #region Helper Methods

    private (TelnetConnection connection, Socket acceptedSocket) CreateConnectionPair()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        _serverSocket.Listen(1);

        var port = ((IPEndPoint)_serverSocket.LocalEndPoint!).Port;

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _clientSocket.Connect(IPAddress.Loopback, port);

        _acceptedSocket = _serverSocket.Accept();

        _server = new TelnetServer(port: port);
        var pipeInput = new SimplePipeInput();

        var connection = new TelnetConnection(
            _acceptedSocket,
            (IPEndPoint)_clientSocket.LocalEndPoint!,
            _ => Task.CompletedTask,
            _server,
            Encoding.UTF8,
            null,
            pipeInput,
            true);

        return (connection, _acceptedSocket);
    }

    #endregion
}
