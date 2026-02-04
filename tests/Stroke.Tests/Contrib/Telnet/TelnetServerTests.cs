namespace Stroke.Tests.Contrib.Telnet;

using System.Text;
using Stroke.Contrib.Telnet;
using Stroke.Styles;
using Xunit;

/// <summary>
/// Unit tests for <see cref="TelnetServer"/>.
/// </summary>
public class TelnetServerTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaults_SetsCorrectValues()
    {
        var server = new TelnetServer();

        Assert.Equal("127.0.0.1", server.Host);
        Assert.Equal(23, server.Port);
        Assert.Equal(Encoding.UTF8, server.Encoding);
        Assert.Null(server.Style);
        Assert.True(server.EnableCpr);
    }

    [Fact]
    public void Constructor_WithCustomHost_SetsHost()
    {
        var server = new TelnetServer(host: "0.0.0.0");

        Assert.Equal("0.0.0.0", server.Host);
    }

    [Fact]
    public void Constructor_WithCustomPort_SetsPort()
    {
        var server = new TelnetServer(port: 2323);

        Assert.Equal(2323, server.Port);
    }

    [Fact]
    public void Constructor_WithCustomEncoding_SetsEncoding()
    {
        var encoding = Encoding.ASCII;
        var server = new TelnetServer(encoding: encoding);

        Assert.Same(encoding, server.Encoding);
    }

    [Fact]
    public void Constructor_WithStyle_SetsStyle()
    {
        var style = DummyStyle.Instance;
        var server = new TelnetServer(style: style);

        Assert.Same(style, server.Style);
    }

    [Fact]
    public void Constructor_WithEnableCprFalse_SetsEnableCpr()
    {
        var server = new TelnetServer(enableCpr: false);

        Assert.False(server.EnableCpr);
    }

    [Fact]
    public void Constructor_WithNullHost_UsesDefault()
    {
        var server = new TelnetServer(host: null!);

        Assert.Equal("127.0.0.1", server.Host);
    }

    #endregion

    #region Port Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void Constructor_WithInvalidPort_ThrowsArgumentOutOfRangeException(int port)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new TelnetServer(port: port));

        Assert.Equal("port", ex.ParamName);
        Assert.Contains("0", ex.Message);
        Assert.Contains("65535", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(23)]
    [InlineData(80)]
    [InlineData(2323)]
    [InlineData(65535)]
    public void Constructor_WithValidPort_DoesNotThrow(int port)
    {
        var ex = Record.Exception(() => new TelnetServer(port: port));

        Assert.Null(ex);
    }

    #endregion

    #region Connections Property Tests

    [Fact]
    public void Connections_Initially_IsEmpty()
    {
        var server = new TelnetServer();

        Assert.Empty(server.Connections);
    }

    [Fact]
    public void Connections_ReturnsSnapshot()
    {
        var server = new TelnetServer();

        // Get first snapshot
        var connections1 = server.Connections;

        // Get second snapshot
        var connections2 = server.Connections;

        // Should be different instances (snapshots)
        Assert.NotSame(connections1, connections2);
    }

    #endregion

    #region Interact Callback Tests

    [Fact]
    public void Constructor_WithNullInteract_UsesDefaultCallback()
    {
        // Should not throw with null interact
        var ex = Record.Exception(() => new TelnetServer(interact: null));

        Assert.Null(ex);
    }

    [Fact]
    public void Constructor_WithInteract_SetsCallback()
    {
        var callbackInvoked = false;
        Func<TelnetConnection, Task> interact = _ =>
        {
            callbackInvoked = true;
            return Task.CompletedTask;
        };

        // Creating server should not invoke callback
        _ = new TelnetServer(interact: interact);

        Assert.False(callbackInvoked);
    }

    #endregion

    #region Deprecated Methods Tests

    [Fact]
    public void Start_IsObsolete()
    {
        var method = typeof(TelnetServer).GetMethod("Start");
        var obsoleteAttr = method!.GetCustomAttributes(typeof(ObsoleteAttribute), false);

        Assert.Single(obsoleteAttr);
    }

    [Fact]
    public void StopAsync_IsObsolete()
    {
        var method = typeof(TelnetServer).GetMethod("StopAsync");
        var obsoleteAttr = method!.GetCustomAttributes(typeof(ObsoleteAttribute), false);

        Assert.Single(obsoleteAttr);
    }

    #endregion
}
