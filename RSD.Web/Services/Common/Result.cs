namespace RSD.Web.Services.Common;

public readonly record struct Unit
{
    public static readonly Unit Value = default;
}

public record Result<T>(bool Ok, T? Value, string Error)
{
    public static Result<T> Success(T value) => new(true, value, "");
    public static Result<T> Failure(string error) => new(false, default, error);
}

public static class Result
{
    public static Result<Unit> Ok() => Result<Unit>.Success(Unit.Value);
    public static Result<Unit> Fail(string error) => Result<Unit>.Failure(error);
    public static Result<T> Ok<T>(T value) => Result<T>.Success(value);
    public static Result<T> Fail<T>(string error) => Result<T>.Failure(error);
}
