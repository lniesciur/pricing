namespace Pricing.Shared.Domain;

public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
}

public class Result<T> : Result
{
    private Result(T value) : base(true, null) => Value = value;
    private Result(string error) : base(false, error) => Value = default;

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(value);
    public new static Result<T> Fail(string error) => new(error);
}
