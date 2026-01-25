using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="DynamicValidator"/>.
/// </summary>
public sealed class DynamicValidatorTests
{
    #region Normal Operation Tests (T024)

    [Fact]
    public void Validate_GetterReturnsValidator_UsesReturnedValidator()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new DynamicValidator(() => innerValidator);
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_GetterReturnsValidator_ValidInput_DoesNotThrow()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var validator = new DynamicValidator(() => innerValidator);
        var document = new Document("hello");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_GetterCalledEachTime()
    {
        // Arrange
        var callCount = 0;
        var innerValidator = new DummyValidator();
        var validator = new DynamicValidator(() =>
        {
            callCount++;
            return innerValidator;
        });
        var document = new Document("test");

        // Act
        validator.Validate(document);
        validator.Validate(document);
        validator.Validate(document);

        // Assert
        Assert.Equal(3, callCount);
    }

    [Fact]
    public void Validate_DifferentValidatorsAtRuntime()
    {
        // Arrange
        IValidator? currentValidator = null;
        var validator = new DynamicValidator(() => currentValidator);
        var emptyDoc = new Document("");

        // Act & Assert - null returns DummyValidator behavior
        validator.Validate(emptyDoc); // Should not throw

        // Switch to strict validator
        currentValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(emptyDoc));
        Assert.Equal("Cannot be empty", ex.Message);

        // Switch back to null
        currentValidator = null;
        validator.Validate(emptyDoc); // Should not throw again
    }

    [Fact]
    public async Task ValidateAsync_GetterReturnsValidator_UsesReturnedValidator()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new DynamicValidator(() => innerValidator);
        var document = new Document("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    #endregion

    #region Null Return (DummyValidator Fallback) Tests (T025)

    [Fact]
    public void Validate_GetterReturnsNull_AcceptsAllInput()
    {
        // Arrange
        var validator = new DynamicValidator(() => null);
        var document = new Document("");

        // Act & Assert - should not throw (DummyValidator behavior)
        validator.Validate(document);
    }

    [Fact]
    public void Validate_GetterReturnsNull_MultipleDocuments()
    {
        // Arrange
        var validator = new DynamicValidator(() => null);

        // Act & Assert - all should pass
        validator.Validate(new Document(""));
        validator.Validate(new Document("hello"));
        validator.Validate(new Document("!@#$%"));
    }

    [Fact]
    public async Task ValidateAsync_GetterReturnsNull_AcceptsAllInput()
    {
        // Arrange
        var validator = new DynamicValidator(() => null);
        var document = new Document("");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public void Validate_GetterReturnsNullThenValidator_SwitchesCorrectly()
    {
        // Arrange
        IValidator? currentValidator = null;
        var validator = new DynamicValidator(() => currentValidator);
        var emptyDoc = new Document("");

        // First call - null, accepts all
        validator.Validate(emptyDoc);

        // Switch to validator that rejects empty
        currentValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        Assert.Throws<ValidationError>(() => validator.Validate(emptyDoc));

        // Switch back to null
        currentValidator = null;
        validator.Validate(emptyDoc);
    }

    #endregion

    #region Null GetValidator Parameter Tests (T026)

    [Fact]
    public void Constructor_NullGetValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new DynamicValidator(null!));
    }

    [Fact]
    public void Validate_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new DynamicValidator(() => new DummyValidator());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public async Task ValidateAsync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new DynamicValidator(() => new DummyValidator());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await validator.ValidateAsync(null!));
    }

    #endregion

    #region Getter Exception Propagation Tests (T027)

    [Fact]
    public void Validate_GetterThrows_ExceptionPropagates()
    {
        // Arrange
        var validator = new DynamicValidator(() =>
            throw new InvalidOperationException("Getter error"));
        var document = new Document("test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.Validate(document));
    }

    [Fact]
    public async Task ValidateAsync_GetterThrows_ExceptionPropagates()
    {
        // Arrange
        var validator = new DynamicValidator(() =>
            throw new InvalidOperationException("Getter error"));
        var document = new Document("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await validator.ValidateAsync(document));
    }

    [Fact]
    public void Validate_ReturnedValidatorThrowsNonValidationError_Propagates()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(doc =>
            throw new InvalidOperationException("Inner error"));
        var validator = new DynamicValidator(() => innerValidator);
        var document = new Document("test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.Validate(document));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void GetValidator_Property_ReturnsGetterFunction()
    {
        // Arrange
        Func<IValidator?> getter = () => new DummyValidator();
        var validator = new DynamicValidator(getter);

        // Assert
        Assert.Same(getter, validator.GetValidator);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void DynamicValidator_ImplementsIValidator()
    {
        var validator = new DynamicValidator(() => null);

        Assert.IsAssignableFrom<IValidator>(validator);
    }

    [Fact]
    public void DynamicValidator_ExtendsValidatorBase()
    {
        var validator = new DynamicValidator(() => null);

        Assert.IsAssignableFrom<ValidatorBase>(validator);
    }

    [Fact]
    public void DynamicValidator_IsSealed()
    {
        Assert.True(typeof(DynamicValidator).IsSealed);
    }

    #endregion

    #region Composition Tests

    [Fact]
    public void Validate_DynamicReturningDynamic()
    {
        // Arrange - dynamic returning another dynamic
        var innerDynamic = new DynamicValidator(() =>
            ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty"));
        var outerDynamic = new DynamicValidator(() => innerDynamic);
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => outerDynamic.Validate(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_DynamicReturningConditional()
    {
        // Arrange
        var filterState = true;
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var conditional = new ConditionalValidator(innerValidator, () => filterState);
        var dynamic = new DynamicValidator(() => conditional);
        var document = new Document("");

        // Act & Assert - filter true, validation runs
        Assert.Throws<ValidationError>(() => dynamic.Validate(document));

        // Change filter
        filterState = false;

        // Validation skipped
        dynamic.Validate(document);
    }

    #endregion
}
