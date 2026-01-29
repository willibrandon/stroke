namespace Stroke.Tests.Contrib.RegularLanguages;

using Stroke.Contrib.RegularLanguages;
using Stroke.Core;
using Stroke.Validation;
using Xunit;

/// <summary>
/// Tests for GrammarValidator.
/// </summary>
public class GrammarValidatorTests
{
    [Fact]
    public void Constructor_NullGrammar_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GrammarValidator(null!));
    }

    [Fact]
    public void Constructor_NullValidators_UsesEmptyDictionary()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var validator = new GrammarValidator(grammar, null);

        Assert.NotNull(validator);
    }

    [Fact]
    public void Validate_NullDocument_ThrowsArgumentNullException()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var validator = new GrammarValidator(grammar);

        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }

    [Fact]
    public void Validate_ValidInput_NoException()
    {
        var grammar = Grammar.Compile(@"hello");
        var validator = new GrammarValidator(grammar);
        var document = new Document("hello");

        // Should not throw
        validator.Validate(document);
    }

    [Fact]
    public void Validate_InvalidInput_ThrowsValidationError()
    {
        var grammar = Grammar.Compile(@"hello");
        var validator = new GrammarValidator(grammar);
        var document = new Document("xyz");

        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Invalid command", ex.Message);
        Assert.Equal(3, ex.CursorPosition); // End of input
    }

    [Fact]
    public void Validate_PartialMatch_ThrowsValidationError()
    {
        var grammar = Grammar.Compile(@"hello world");
        var validator = new GrammarValidator(grammar);
        var document = new Document("hello");

        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        Assert.Equal("Invalid command", ex.Message);
    }

    [Fact]
    public void Validate_WithPerVariableValidator_CallsValidator()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<num>\d+)");
        var validators = new Dictionary<string, IValidator>
        {
            ["num"] = new RangeValidator(1, 100)
        };
        var validator = new GrammarValidator(grammar, validators);

        // Valid: 50 is in range
        validator.Validate(new Document("cmd 50"));

        // Invalid: 200 is out of range
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(new Document("cmd 200")));
        Assert.Contains("range", ex.Message.ToLower());
    }

    [Fact]
    public void Validate_PerVariableError_AdjustsCursorPosition()
    {
        var grammar = Grammar.Compile(@"prefix\s(?P<value>\w+)");
        var validators = new Dictionary<string, IValidator>
        {
            ["value"] = new PositionValidator(2) // Throw error at position 2
        };
        var validator = new GrammarValidator(grammar, validators);
        var document = new Document("prefix test");

        var ex = Assert.Throws<ValidationError>(() => validator.Validate(document));
        // "prefix " is 7 chars, error at position 2 in value
        // So total position should be 7 + 2 = 9
        Assert.Equal(9, ex.CursorPosition);
    }

    [Fact]
    public void Validate_MultipleVariables_ValidatesAll()
    {
        var grammar = Grammar.Compile(@"(?P<a>\d+)\s(?P<b>\d+)");
        var validators = new Dictionary<string, IValidator>
        {
            ["a"] = new RangeValidator(1, 10),
            ["b"] = new RangeValidator(1, 10)
        };
        var validator = new GrammarValidator(grammar, validators);

        // Both valid
        validator.Validate(new Document("5 5"));

        // First invalid
        var ex = Assert.Throws<ValidationError>(() => validator.Validate(new Document("50 5")));
        Assert.Contains("range", ex.Message.ToLower());
    }

    [Fact]
    public void Validate_NoValidatorForVariable_SkipsValidation()
    {
        var grammar = Grammar.Compile(@"(?P<cmd>\w+)");
        var validators = new Dictionary<string, IValidator>
        {
            // No validator for "cmd"
        };
        var validator = new GrammarValidator(grammar, validators);

        // Should not throw
        validator.Validate(new Document("anything"));
    }

    [Fact]
    public async Task ValidateAsync_ValidInput_NoException()
    {
        var grammar = Grammar.Compile(@"hello");
        var validator = new GrammarValidator(grammar);
        var document = new Document("hello");

        // Should not throw
        await validator.ValidateAsync(document);
    }

    [Fact]
    public async Task ValidateAsync_InvalidInput_ThrowsValidationError()
    {
        var grammar = Grammar.Compile(@"hello");
        var validator = new GrammarValidator(grammar);
        var document = new Document("xyz");

        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(document));
        Assert.Equal("Invalid command", ex.Message);
    }

    [Fact]
    public async Task ValidateAsync_WithPerVariableValidator_CallsValidatorAsync()
    {
        var grammar = Grammar.Compile(@"cmd\s(?P<num>\d+)");
        var validators = new Dictionary<string, IValidator>
        {
            ["num"] = new RangeValidator(1, 100)
        };
        var validator = new GrammarValidator(grammar, validators);

        // Valid
        await validator.ValidateAsync(new Document("cmd 50"));

        // Invalid
        var ex = await Assert.ThrowsAsync<ValidationError>(async () =>
            await validator.ValidateAsync(new Document("cmd 200")));
        Assert.Contains("range", ex.Message.ToLower());
    }

    [Fact]
    public void Validate_NonValidationErrorException_Propagates()
    {
        var grammar = Grammar.Compile(@"(?P<value>\w+)");
        var validators = new Dictionary<string, IValidator>
        {
            ["value"] = new ThrowingValidator()
        };
        var validator = new GrammarValidator(grammar, validators);

        Assert.Throws<InvalidOperationException>(() =>
            validator.Validate(new Document("test")));
    }

    /// <summary>
    /// Validator that checks if a numeric value is in a range.
    /// </summary>
    private class RangeValidator : IValidator
    {
        private readonly int _min;
        private readonly int _max;

        public RangeValidator(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public void Validate(Document document)
        {
            if (int.TryParse(document.Text, out var value))
            {
                if (value < _min || value > _max)
                {
                    throw new ValidationError(0, $"Value must be in range {_min}-{_max}");
                }
            }
            else
            {
                throw new ValidationError(0, "Invalid number");
            }
        }

        public ValueTask ValidateAsync(Document document)
        {
            Validate(document);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Validator that always throws at a specific position.
    /// </summary>
    private class PositionValidator : IValidator
    {
        private readonly int _position;

        public PositionValidator(int position)
        {
            _position = position;
        }

        public void Validate(Document document)
        {
            throw new ValidationError(_position, "Error at position");
        }

        public ValueTask ValidateAsync(Document document)
        {
            Validate(document);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Validator that throws a non-ValidationError exception.
    /// </summary>
    private class ThrowingValidator : IValidator
    {
        public void Validate(Document document)
        {
            throw new InvalidOperationException("Internal error");
        }

        public ValueTask ValidateAsync(Document document)
        {
            throw new InvalidOperationException("Internal error");
        }
    }
}
