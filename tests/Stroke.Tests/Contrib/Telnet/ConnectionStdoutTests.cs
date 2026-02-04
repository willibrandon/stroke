namespace Stroke.Tests.Contrib.Telnet;

using System.Net.Sockets;
using System.Net;
using System.Text;
using Stroke.Contrib.Telnet;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ConnectionStdout"/>.
/// </summary>
public class ConnectionStdoutTests : IDisposable
{
    private Socket? _serverSocket;
    private Socket? _clientSocket;
    private Socket? _acceptedSocket;

    public void Dispose()
    {
        _acceptedSocket?.Dispose();
        _clientSocket?.Dispose();
        _serverSocket?.Dispose();
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullSocket_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ConnectionStdout(null!, Encoding.UTF8));
    }

    [Fact]
    public void Constructor_WithNullEncoding_ThrowsArgumentNullException()
    {
        using var socket = CreateConnectedSocketPair(out _, out _);
        Assert.Throws<ArgumentNullException>(() =>
            new ConnectionStdout(socket, null!));
    }

    [Fact]
    public void Constructor_WithValidArgs_CreatesWriter()
    {
        using var socket = CreateConnectedSocketPair(out _, out _);
        var writer = new ConnectionStdout(socket, Encoding.UTF8);

        Assert.NotNull(writer);
        Assert.Equal(Encoding.UTF8, writer.Encoding);
    }

    #endregion

    #region Write Tests

    [Fact]
    public void Write_WithString_SendsToSocket()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("Hello");

        var received = ReceiveFromSocket(clientSocket);
        Assert.Equal("Hello", received);
    }

    [Fact]
    public void Write_WithNull_DoesNotThrow()
    {
        var (writer, _) = CreateWriterPair();

        var ex = Record.Exception(() => writer.Write((string?)null));

        Assert.Null(ex);
    }

    [Fact]
    public void Write_WithEmptyString_DoesNotSend()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("");

        // Nothing should be sent for empty string
        Assert.False(clientSocket.Poll(100, SelectMode.SelectRead));
    }

    [Fact]
    public void Write_WithChar_SendsToSocket()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write('X');

        var received = ReceiveFromSocket(clientSocket);
        Assert.Equal("X", received);
    }

    [Fact]
    public void Write_WithCharArray_SendsToSocket()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write(['H', 'i', '!'], 0, 3);

        var received = ReceiveFromSocket(clientSocket);
        Assert.Equal("Hi!", received);
    }

    #endregion

    #region LF → CRLF Conversion (FR-006)

    [Fact]
    public void Write_WithLF_ConvertsToCRLF()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("Hello\nWorld");

        var received = ReceiveBytesFromSocket(clientSocket);
        // Should contain CRLF (0x0D 0x0A) instead of just LF (0x0A)
        Assert.Equal("Hello\r\nWorld"u8.ToArray(), received);
    }

    [Fact]
    public void Write_WithMultipleLFs_ConvertsAllToCRLF()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("A\nB\nC\n");

        var received = ReceiveBytesFromSocket(clientSocket);
        Assert.Equal("A\r\nB\r\nC\r\n"u8.ToArray(), received);
    }

    [Fact]
    public void Write_WithExistingCRLF_DoubleConverts()
    {
        // Per contract: CRLF → CR-CR-LF (matches Python PTK behavior)
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("Line\r\nEnd");

        var received = ReceiveBytesFromSocket(clientSocket);
        // \r\n becomes \r\r\n (the \n is converted to \r\n)
        Assert.Equal("Line\r\r\nEnd"u8.ToArray(), received);
    }

    [Fact]
    public void Write_WithOnlyCR_DoesNotConvert()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Write("Line\rEnd");

        var received = ReceiveBytesFromSocket(clientSocket);
        // CR alone should not be affected
        Assert.Equal("Line\rEnd"u8.ToArray(), received);
    }

    #endregion

    #region Close and IsClosed Tests

    [Fact]
    public void Close_SetsIsClosedTrue()
    {
        var (writer, _) = CreateWriterPair();

        Assert.False(writer.IsClosed);
        writer.Close();
        Assert.True(writer.IsClosed);
    }

    [Fact]
    public void Close_IsIdempotent()
    {
        var (writer, _) = CreateWriterPair();

        writer.Close();
        writer.Close();
        writer.Close();

        Assert.True(writer.IsClosed);
    }

    [Fact]
    public void Write_AfterClose_IsNoOp()
    {
        var (writer, clientSocket) = CreateWriterPair();

        writer.Close();
        writer.Write("Should not be sent");

        // Nothing should be sent
        Assert.False(clientSocket.Poll(100, SelectMode.SelectRead));
    }

    [Fact]
    public void Flush_AfterClose_IsNoOp()
    {
        var (writer, _) = CreateWriterPair();

        writer.Close();
        var ex = Record.Exception(() => writer.Flush());

        Assert.Null(ex);
    }

    #endregion

    #region IsAtty Tests

    [Fact]
    public void IsAtty_ReturnsTrue()
    {
        var (writer, _) = CreateWriterPair();

        Assert.True(writer.IsAtty);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task Write_FromMultipleThreads_DoesNotCorrupt()
    {
        var (writer, clientSocket) = CreateWriterPair();
        var tasks = new List<Task>();
        var ct = TestContext.Current.CancellationToken;

        // Write from multiple threads concurrently
        for (int i = 0; i < 10; i++)
        {
            var message = $"Thread{i}\n";
            tasks.Add(Task.Run(() => writer.Write(message), ct));
        }

        await Task.WhenAll(tasks);

        // Receive all data
        var buffer = new byte[4096];
        clientSocket.ReceiveTimeout = 500;
        var totalReceived = 0;
        try
        {
            while (true)
            {
                var received = clientSocket.Receive(buffer, totalReceived, buffer.Length - totalReceived, SocketFlags.None);
                if (received == 0) break;
                totalReceived += received;
            }
        }
        catch (SocketException)
        {
            // Timeout - expected
        }

        var receivedString = Encoding.UTF8.GetString(buffer, 0, totalReceived);

        // All threads should have written (order may vary)
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains($"Thread{i}\r\n", receivedString);
        }
    }

    #endregion

    #region Helper Methods

    private Socket CreateConnectedSocketPair(out Socket listener, out Socket client)
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        listener.Listen(1);
        _serverSocket = listener;

        var port = ((IPEndPoint)listener.LocalEndPoint!).Port;

        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _clientSocket = client;
        client.Connect(IPAddress.Loopback, port);

        var accepted = listener.Accept();
        _acceptedSocket = accepted;

        return accepted;
    }

    private (ConnectionStdout writer, Socket clientSocket) CreateWriterPair()
    {
        var serverSocket = CreateConnectedSocketPair(out _, out var clientSocket);
        var writer = new ConnectionStdout(serverSocket, Encoding.UTF8);
        return (writer, clientSocket);
    }

    private static string ReceiveFromSocket(Socket socket)
    {
        var buffer = new byte[4096];
        socket.ReceiveTimeout = 1000;
        var received = socket.Receive(buffer);
        return Encoding.UTF8.GetString(buffer, 0, received);
    }

    private static byte[] ReceiveBytesFromSocket(Socket socket)
    {
        var buffer = new byte[4096];
        socket.ReceiveTimeout = 1000;
        var received = socket.Receive(buffer);
        var result = new byte[received];
        Array.Copy(buffer, result, received);
        return result;
    }

    #endregion
}
