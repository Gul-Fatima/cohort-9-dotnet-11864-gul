namespace TaskManagement.Core.Exceptions;

/// <summary>
/// A business-rule failure that should surface as a specific HTTP status.
/// A global exception filter catches this and returns
/// StatusCode(ex.StatusCode) with the { title, message } body the frontend expects.
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public string Title { get; }

    public ApiException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}
