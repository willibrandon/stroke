using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ConditionalValidator"/>.
/// </summary>
public sealed class ConditionalValidatorTests
{
    #region Filter=True Scenario Tests (T017)

    [Fact]
    public void Validate_FilterReturnsTrue_ValidInput_DoesNotThrow()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("hello");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_FilterReturnsTrue_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_FilterReturnsTrue_InnerValidatorCalled()
    {
        // Arrange
        var callCount = 0;
        var innerValidator = ValidatorBase.FromCallable(text =>
        {
            callCount++;
            return true;
        });
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("test");

        // Act
        validator.Validate(document);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ValidateAsync_FilterReturnsTrue_ValidInput_Completes()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("hello");

        // Act & Assert
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_FilterReturnsTrue_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    #endregion

    #region Filter=False Scenario Tests (T018)

    [Fact]
    public void Validate_FilterReturnsFalse_InvalidInput_DoesNotThrow()
    {
        // Arrange - inner validator would fail, but filter is false
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ConditionalValidator(innerValidator, () => false);
        var document = new Document(""); // Would fail inner validation

        // Act & Assert - should not throw because filter is false
        validator.Validate(document);
    }

    [Fact]
    public void Validate_FilterReturnsFalse_InnerValidatorNotCalled()
    {
        // Arrange
        var callCount = 0;
        var innerValidator = ValidatorBase.FromCallable(text =>
        {
            callCount++;
            return false; // Would throw
        });
        var validator = new ConditionalValidator(innerValidator, () => false);
        var document = new Document("test");

        // Act
        validator.Validate(document);

        // Assert - inner validator should not be called
        Assert.Equal(0, callCount);
    }

    [Fact]
    public async Task ValidateAsync_FilterReturnsFalse_InvalidInput_Completes()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ConditionalValidator(innerValidator, () => false);
        var document = new Document("");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public void Validate_DynamicFilter_FilterChanges()
    {
        // Arrange
        var filterState = false;
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ConditionalValidator(innerValidator, () => filterState);
        var emptyDoc = new Document("");

        // Act & Assert - filter false, validation skipped
        validator.Validate(emptyDoc);

        // Change filter state
        filterState = true;

        // Act & Assert - filter true, validation runs
        Assert.Throws<ValidationError>(() => validator.Validate(emptyDoc));
    }

    #endregion

    #region Null Parameter Handling Tests (T019)

    [Fact]
    public void Constructor_NullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ConditionalValidator(null!, () => true));
    }

    [Fact]
    public void Constructor_NullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        var innerValidator = new DummyValidator();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ConditionalValidator(innerValidator, null!));
    }

    [Fact]
    public void Validate_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new ConditionalValidator(new DummyValidator(), () => true);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public async Task ValidateAsync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new ConditionalValidator(new DummyValidator(), () => true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await validator.ValidateAsync(null!));
    }

    #endregion

    #region Filter Exception Propagation Tests (T020)

    [Fact]
    public void Validate_FilterThrows_ExceptionPropagates()
    {
        // Arrange
        var innerValidator = new DummyValidator();
        var validator = new ConditionalValidator(innerValidator, () =>
            throw new InvalidOperationException("Filter error"));
        var document = new Document("test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.Validate(document));
    }

    [Fact]
    public async Task ValidateAsync_FilterThrows_ExceptionPropagates()
    {
        // Arrange
        var innerValidator = new DummyValidator();
        var validator = new ConditionalValidator(innerValidator, () =>
            throw new InvalidOperationException("Filter error"));
        var document = new Document("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await validator.ValidateAsync(document));
    }

    [Fact]
    public void Validate_InnerValidatorThrowsNonValidationError_Propagates()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(doc =>
            throw new InvalidOperationException("Inner error"));
        var validator = new ConditionalValidator(innerValidator, () => true);
        var document = new Document("test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => validator.Validate(document));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Validator_Property_ReturnsWrappedValidator()
    {
        // Arrange
        var innerValidator = new DummyValidator();
        var validator = new ConditionalValidator(innerValidator, () => true);

        // Assert
        Assert.Same(innerValidator, validator.Validator);
    }

    [Fact]
    public void Filter_Property_ReturnsFilter()
    {
        // Arrange
        Func<bool> filter = () => true;
        var validator = new ConditionalValidator(new DummyValidator(), filter);

        // Assert
        Assert.Same(filter, validator.Filter);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void ConditionalValidator_ImplementsIValidator()
    {
        var validator = new ConditionalValidator(new DummyValidator(), () => true);

        Assert.IsAssignableFrom<IValidator>(validator);
    }

    [Fact]
    public void ConditionalValidator_ExtendsValidatorBase()
    {
        var validator = new ConditionalValidator(new DummyValidator(), () => true);

        Assert.IsAssignableFrom<ValidatorBase>(validator);
    }

    [Fact]
    public void ConditionalValidator_IsSealed()
    {
        Assert.True(typeof(ConditionalValidator).IsSealed);
    }

    #endregion

    #region Composition Tests

    [Fact]
    public void Validate_NestedConditionalValidators()
    {
        // Arrange - conditional wrapping another conditional
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var condition1 = new ConditionalValidator(innerValidator, () => true);
        var condition2 = new ConditionalValidator(condition1, () => true);
        var document = new Document("");

        // Act & Assert
        Assert.Throws<ValidationError>(() => condition2.Validate(document));
    }

    [Fact]
    public void Validate_NestedConditional_OuterFalse_SkipsAll()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var condition1 = new ConditionalValidator(innerValidator, () => true);
        var condition2 = new ConditionalValidator(condition1, () => false);
        var document = new Document("");

        // Act & Assert - outer filter false, skips everything
        condition2.Validate(document);
    }

    #endregion
}
