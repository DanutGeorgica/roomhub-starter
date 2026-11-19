namespace RoomHub.Api.Domain;

public sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public Error? Error { get; }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);
}

public sealed class Result
{
    private Result()
    {
        IsSuccess = true;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static Result Success() => new();

    public static Result Failure(Error error) => new(error);
}
