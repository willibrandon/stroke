using Xunit;

namespace Stroke.Tests.Contrib.Ssh;

/// <summary>
/// Collection definition for SSH server tests.
/// Tests in this collection run sequentially to avoid port conflicts.
/// </summary>
[CollectionDefinition("SSH Server Tests")]
public class SshServerTestCollection : ICollectionFixture<SshServerTestCollection.NoopFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.

    /// <summary>
    /// No-op fixture - just used to define the collection.
    /// </summary>
    public class NoopFixture { }
}
