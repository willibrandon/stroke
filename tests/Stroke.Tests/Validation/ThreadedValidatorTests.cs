using Stroke.Core;
using Stroke.Validation;
using Xunit;

namespace Stroke.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ThreadedValidator"/>.
/// </summary>
public sealed class ThreadedValidatorTests
{
    #region Async Execution Tests (T031)

    [Fact]
    public async Task ValidateAsync_RunsOnBackgroundThread()
    {
        // Arrange
        var callingThreadId = Environment.CurrentManagedThreadId;
        int? validationThreadId = null;

        var innerValidator = ValidatorBase.FromCallable(doc =>
        {
            validationThreadId = Environment.CurrentManagedThreadId;
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act
        await validator.ValidateAsync(document);

        // Assert - validation should have run on a different thread
        Assert.NotNull(validationThreadId);
        Assert.NotEqual(callingThreadId, validationThreadId);
    }

    [Fact]
    public async Task ValidateAsync_ValidInput_Completes()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("hello");

        // Act & Assert - should complete without exception
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_DoesNotBlockCallingThread()
    {
        // Arrange
        var validationStarted = new TaskCompletionSource<bool>();
        var canComplete = new TaskCompletionSource<bool>();

        var innerValidator = ValidatorBase.FromCallable(doc =>
        {
            validationStarted.SetResult(true);
            canComplete.Task.Wait(); // Block validation until signaled
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act - start validation
        var validateTask = validator.ValidateAsync(document).AsTask();

        // Wait for validation to start
        await validationStarted.Task;

        // Assert - task should not be completed yet
        Assert.False(validateTask.IsCompleted);

        // Allow validation to complete
        canComplete.SetResult(true);
        await validateTask;
    }

    [Fact]
    public async Task ValidateAsync_MultipleCallsExecuteIndependently()
    {
        // Arrange
        var callCount = 0;
        var innerValidator = ValidatorBase.FromCallable(doc =>
        {
            Interlocked.Increment(ref callCount);
            Thread.Sleep(10); // Small delay
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act - start multiple validations
        var task1 = validator.ValidateAsync(document).AsTask();
        var task2 = validator.ValidateAsync(document).AsTask();
        var task3 = validator.ValidateAsync(document).AsTask();

        await Task.WhenAll(task1, task2, task3);

        // Assert - all three should have run
        Assert.Equal(3, callCount);
    }

    #endregion

    #region Sync Validate Delegation Tests (T032)

    [Fact]
    public void Validate_DelegatesToWrappedValidator()
    {
        // Arrange
        var callCount = 0;
        var innerValidator = ValidatorBase.FromCallable(doc =>
        {
            callCount++;
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act
        validator.Validate(document);

        // Assert
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Validate_ValidInput_DoesNotThrow()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0);
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("hello");

        // Act & Assert - should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_InvalidInput_ThrowsValidationError()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("");

        // Act & Assert
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public void Validate_RunsOnCallingThread()
    {
        // Arrange
        var callingThreadId = Environment.CurrentManagedThreadId;
        int? validationThreadId = null;

        var innerValidator = ValidatorBase.FromCallable(doc =>
        {
            validationThreadId = Environment.CurrentManagedThreadId;
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act
        validator.Validate(document);

        // Assert - should run on same thread (sync)
        Assert.Equal(callingThreadId, validationThreadId);
    }

    #endregion

    #region Exception Propagation Tests (T033)

    [Fact]
    public async Task ValidateAsync_ValidationError_PropagatesCorrectly()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_NonValidationError_Propagates()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(doc =>
            throw new InvalidOperationException("Something went wrong"));
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await validator.ValidateAsync(document));
    }

    [Fact]
    public async Task ValidateAsync_PreservesCursorPosition()
    {
        // Arrange
        var innerValidator = ValidatorBase.FromCallable(
            text => false,
            errorMessage: "Error",
            moveCursorToEnd: true
        );
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("hello"); // Length 5

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal(5, ex.CursorPosition);
    }

    #endregion

    #region Null Validator Parameter Tests (T034)

    [Fact]
    public void Constructor_NullValidator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ThreadedValidator(null!));
    }

    [Fact]
    public void Validate_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new ThreadedValidator(new DummyValidator());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public async Task ValidateAsync_NullDocument_ThrowsArgumentNullException()
    {
        // Arrange
        var validator = new ThreadedValidator(new DummyValidator());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await validator.ValidateAsync(null!));
    }

    #endregion

    #region Concurrent Stress Tests (T035)

    [Fact]
    public async Task ConcurrentStressTest_10Threads_1000Operations()
    {
        // Arrange
        var successCount = 0;
        var errorCount = 0;
        var innerValidator = ValidatorBase.FromCallable(text =>
        {
            // Simulate some work
            Thread.SpinWait(100);
            return text.Length > 0;
        }, "Cannot be empty");
        var validator = new ThreadedValidator(innerValidator);

        var tasks = new List<Task>();
        var random = new Random(42); // Deterministic seed

        // Act - 10 threads, each doing 100 operations = 1000 total
        var ct = TestContext.Current.CancellationToken;
        for (int thread = 0; thread < 10; thread++)
        {
            var threadId = thread;
            tasks.Add(Task.Run(async () =>
            {
                for (int op = 0; op < 100; op++)
                {
                    var document = new Document(random.Next(2) == 0 ? "" : "valid");
                    try
                    {
                        await validator.ValidateAsync(document);
                        Interlocked.Increment(ref successCount);
                    }
                    catch (ValidationError)
                    {
                        Interlocked.Increment(ref errorCount);
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Assert - all 1000 operations completed
        Assert.Equal(1000, successCount + errorCount);
    }

    [Fact]
    public async Task ConcurrentStressTest_WrappedDummyValidator()
    {
        // Arrange
        var callCount = 0;
        var validator = new ThreadedValidator(new DummyValidator());

        var tasks = new List<Task>();

        // Act - 10 threads, each doing 100 validations
        var ct = TestContext.Current.CancellationToken;
        for (int thread = 0; thread < 10; thread++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int op = 0; op < 100; op++)
                {
                    var document = new Document("test");
                    await validator.ValidateAsync(document);
                    Interlocked.Increment(ref callCount);
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1000, callCount);
    }

    [Fact]
    public async Task ConcurrentStressTest_MixedSyncAndAsync()
    {
        // Arrange
        var syncCount = 0;
        var asyncCount = 0;
        var innerValidator = ValidatorBase.FromCallable(text =>
        {
            Thread.SpinWait(50);
            return true;
        });
        var validator = new ThreadedValidator(innerValidator);
        var document = new Document("test");

        var tasks = new List<Task>();

        // Act - mix of sync and async calls
        var ct = TestContext.Current.CancellationToken;
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < 50; j++)
                {
                    if (j % 2 == 0)
                    {
                        validator.Validate(document);
                        Interlocked.Increment(ref syncCount);
                    }
                    else
                    {
                        await validator.ValidateAsync(document);
                        Interlocked.Increment(ref asyncCount);
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(250, syncCount);
        Assert.Equal(250, asyncCount);
    }

    [Fact]
    public async Task ConcurrentStressTest_NoDataCorruption()
    {
        // Arrange - each thread validates different content and checks position
        var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
        var validator = new ThreadedValidator(ValidatorBase.FromCallable(
            text => false,
            moveCursorToEnd: true
        ));

        var tasks = new List<Task>();

        // Act - each thread validates different length strings
        var ct = TestContext.Current.CancellationToken;
        for (int thread = 0; thread < 10; thread++)
        {
            var expectedLength = thread + 1;
            var text = new string('x', expectedLength);

            tasks.Add(Task.Run(async () =>
            {
                for (int op = 0; op < 100; op++)
                {
                    var document = new Document(text);
                    try
                    {
                        await validator.ValidateAsync(document);
                    }
                    catch (ValidationError ex)
                    {
                        if (ex.CursorPosition != expectedLength)
                        {
                            errors.Add($"Expected {expectedLength}, got {ex.CursorPosition}");
                        }
                    }
                }
            }, ct));
        }

        await Task.WhenAll(tasks);

        // Assert - no data corruption
        Assert.Empty(errors);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Validator_Property_ReturnsWrappedValidator()
    {
        // Arrange
        var innerValidator = new DummyValidator();
        var validator = new ThreadedValidator(innerValidator);

        // Assert
        Assert.Same(innerValidator, validator.Validator);
    }

    #endregion

    #region Type Tests

    [Fact]
    public void ThreadedValidator_ImplementsIValidator()
    {
        var validator = new ThreadedValidator(new DummyValidator());

        Assert.IsAssignableFrom<IValidator>(validator);
    }

    [Fact]
    public void ThreadedValidator_ExtendsValidatorBase()
    {
        var validator = new ThreadedValidator(new DummyValidator());

        Assert.IsAssignableFrom<ValidatorBase>(validator);
    }

    [Fact]
    public void ThreadedValidator_IsSealed()
    {
        Assert.True(typeof(ThreadedValidator).IsSealed);
    }

    #endregion

    #region Composition Tests

    [Fact]
    public async Task ValidateAsync_NestedThreadedValidators()
    {
        // Arrange - threaded wrapping another threaded
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var inner = new ThreadedValidator(innerValidator);
        var outer = new ThreadedValidator(inner);
        var document = new Document("");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await outer.ValidateAsync(document));
        Assert.Equal("Cannot be empty", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_ThreadedWrappingConditional()
    {
        // Arrange
        var filterState = true;
        var innerValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var conditional = new ConditionalValidator(innerValidator, () => filterState);
        var threaded = new ThreadedValidator(conditional);
        var document = new Document("");

        // Act & Assert - filter true
        await Assert.ThrowsAsync<ValidationError>(async () =>
            await threaded.ValidateAsync(document));

        // Change filter
        filterState = false;

        // No exception
        await threaded.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_ThreadedWrappingDynamic()
    {
        // Arrange
        IValidator? currentValidator = ValidatorBase.FromCallable(text => text.Length > 0, "Cannot be empty");
        var dynamic = new DynamicValidator(() => currentValidator);
        var threaded = new ThreadedValidator(dynamic);
        var document = new Document("");

        // Act & Assert - with validator
        await Assert.ThrowsAsync<ValidationError>(async () =>
            await threaded.ValidateAsync(document));

        // Set to null
        currentValidator = null;

        // No exception
        await threaded.ValidateAsync(document);
    }

    #endregion
}
