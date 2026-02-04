using System.Text;
using Stroke.Contrib.Ssh;
using Stroke.Styles;
using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Tests for <see cref="PromptToolkitSshServer"/> constructor validation and properties.
/// </summary>
public class SshServerTests
{
    #region Constructor Validation

    [Fact]
    public void Constructor_WithNullHostKeyPath_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new PromptToolkitSshServer(
                host: "127.0.0.1",
                port: 2222,
                interact: _ => Task.CompletedTask,
                hostKeyPath: null));

        Assert.Equal("hostKeyPath", ex.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(65536)]
    [InlineData(70000)]
    public void Constructor_WithInvalidPort_ThrowsArgumentOutOfRangeException(int port)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PromptToolkitSshServer(
                host: "127.0.0.1",
                port: port,
                interact: _ => Task.CompletedTask,
                hostKeyPath: "/tmp/test_key"));

        Assert.Equal("port", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(22)]
    [InlineData(2222)]
    [InlineData(65535)]
    public void Constructor_WithValidPort_Succeeds(int port)
    {
        var server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: port,
            interact: _ => Task.CompletedTask,
            hostKeyPath: "/tmp/test_key");

        Assert.Equal(port, server.Port);
    }

    [Fact]
    public void Constructor_WithNullInteract_UsesDefaultInteract()
    {
        // Should not throw - null interact uses a no-op default
        var server = new PromptToolkitSshServer(
            host: "127.0.0.1",
            port: 2222,
            interact: null,
            hostKeyPath: "/tmp/test_key");

        Assert.NotNull(server);
    }

    [Fact]
    public void Constructor_WithNullHost_UsesDefaultHost()
    {
        var server = new PromptToolkitSshServer(
            host: null!,
            port: 2222,
            interact: _ => Task.CompletedTask,
            hostKeyPath: "/tmp/test_key");

        Assert.Equal("127.0.0.1", server.Host);
    }

    #endregion

    #region Property Defaults

    [Fact]
    public void Host_DefaultsTo127001()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.Equal("127.0.0.1", server.Host);
    }

    [Fact]
    public void Port_DefaultsTo2222()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.Equal(2222, server.Port);
    }

    [Fact]
    public void Encoding_DefaultsToUtf8()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.Equal(Encoding.UTF8, server.Encoding);
    }

    [Fact]
    public void Encoding_CanBeCustomized()
    {
        var server = new PromptToolkitSshServer(
            hostKeyPath: "/tmp/test_key",
            encoding: Encoding.ASCII);

        Assert.Equal(Encoding.ASCII, server.Encoding);
    }

    [Fact]
    public void Style_DefaultsToNull()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.Null(server.Style);
    }

    [Fact]
    public void EnableCpr_DefaultsToTrue()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.True(server.EnableCpr);
    }

    [Fact]
    public void EnableCpr_CanBeDisabled()
    {
        var server = new PromptToolkitSshServer(
            hostKeyPath: "/tmp/test_key",
            enableCpr: false);

        Assert.False(server.EnableCpr);
    }

    [Fact]
    public void Connections_InitiallyEmpty()
    {
        var server = new PromptToolkitSshServer(hostKeyPath: "/tmp/test_key");

        Assert.Empty(server.Connections);
    }

    #endregion

    #region BeginAuth

    [Fact]
    public void BeginAuth_ReturnsFalseByDefault()
    {
        // Create a test subclass to access the protected method
        var server = new TestableServer("/tmp/test_key");

        var result = server.TestBeginAuth("testuser");

        Assert.False(result);
    }

    [Fact]
    public void BeginAuth_CanBeOverridden()
    {
        var server = new AuthRequiredServer("/tmp/test_key");

        var result = server.TestBeginAuth("admin");

        Assert.True(result);
    }

    #endregion

    #region Helper Classes

    private class TestableServer : PromptToolkitSshServer
    {
        public TestableServer(string hostKeyPath) : base(hostKeyPath: hostKeyPath) { }

        public bool TestBeginAuth(string username) => BeginAuth(username);
    }

    private class AuthRequiredServer : PromptToolkitSshServer
    {
        public AuthRequiredServer(string hostKeyPath) : base(hostKeyPath: hostKeyPath) { }

        public bool TestBeginAuth(string username) => BeginAuth(username);

        protected override bool BeginAuth(string username) => true;
    }

    #endregion
}
