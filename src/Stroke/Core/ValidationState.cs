namespace Stroke.Core;

/// <summary>
/// The validation state of a buffer.
/// </summary>
public enum ValidationState
{
    /// <summary>Input is valid.</summary>
    Valid,

    /// <summary>Input is invalid.</summary>
    Invalid,

    /// <summary>Not yet validated.</summary>
    Unknown
}
