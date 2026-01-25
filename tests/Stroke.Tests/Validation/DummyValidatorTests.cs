using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="DummyValidator"/>.
/// </summary>
public sealed class DummyValidatorTests
{
    #region Basic Validation Tests (T015)

    [Fact]
    public void Validate_EmptyString_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_NonEmptyString_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("hello world");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_SpecialCharacters_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("!@#$%^&*()_+-=[]{}|;':\",./<>?");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_UnicodeCharacters_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("你好世界 🌍 مرحبا");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_NewlinesAndTabs_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("line1\nline2\tcolumn");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_VeryLongText_DoesNotThrow()
    {
        // Arrange
        var validator = new DummyValidator();
        var longText = new string('x', 100_000);
        var document = new Document(longText);

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    #endregion

    #region Async Validation Tests

    [Fact]
    public async Task ValidateAsync_EmptyString_CompletesWithoutException()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_NonEmptyString_CompletesWithoutException()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("hello");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsCompletedTask()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("test");

        // Act
        var task = validator.ValidateAsync(document);

        // Assert - should be already completed (sync validation)
        Assert.True(task.IsCompletedSuccessfully);
        await task;
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void Validate_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new DummyValidator();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public async Task ValidateAsync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new DummyValidator();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await validator.ValidateAsync(null!));
    }

    #endregion

    #region Type Tests

    [Fact]
    public void DummyValidator_ImplementsIValidator()
    {
        var validator = new DummyValidator();

        Assert.IsAssignableFrom<IValidator>(validator);
    }

    [Fact]
    public void DummyValidator_ExtendsValidatorBase()
    {
        var validator = new DummyValidator();

        Assert.IsAssignableFrom<ValidatorBase>(validator);
    }

    [Fact]
    public void DummyValidator_IsSealed()
    {
        Assert.True(typeof(DummyValidator).IsSealed);
    }

    #endregion

    #region Reusability Tests

    [Fact]
    public void Validate_SameInstance_MultipleDocuments()
    {
        // Arrange
        var validator = new DummyValidator();

        // Act & Assert - same validator validates multiple documents
        validator.Validate(new Document("first"));
        validator.Validate(new Document("second"));
        validator.Validate(new Document("third"));
    }

    [Fact]
    public void Validate_SameDocument_MultipleTimes()
    {
        // Arrange
        var validator = new DummyValidator();
        var document = new Document("reused");

        // Act & Assert - same document validated multiple times
        for (int i = 0; i < 100; i++)
        {
            validator.Validate(document);
        }
    }

    #endregion
}
