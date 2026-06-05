namespace OficinaMecanica.Application.Common;

public enum ResultErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);
    public static Result<T> Validation(string error) => new(false, default, error, ResultErrorType.Validation);
    public static Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
    public static Result<T> Conflict(string error) => new(false, default, error, ResultErrorType.Conflict);
    public static Result<T> Unauthorized(string error) => new(false, default, error, ResultErrorType.Unauthorized);
    public static Result<T> Fail(ResultErrorType errorType, string? error) => new(false, default, error, errorType);
}
