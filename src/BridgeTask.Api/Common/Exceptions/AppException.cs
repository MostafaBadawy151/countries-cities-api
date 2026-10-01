namespace BridgeTask.Api.Common.Exceptions;

/// Base type for expected, client-facing errors mapped to an HTTP status code.
public abstract class AppException(string message, int statusCode, string title) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;
}

public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} with id '{key}' was not found.", StatusCodes.Status404NotFound, "Resource not found");

public sealed class ConflictException(string message)
    : AppException(message, StatusCodes.Status409Conflict, "Conflict");
