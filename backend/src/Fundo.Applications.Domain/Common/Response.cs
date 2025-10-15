namespace Fundo.Applications.Domain.Common;

using System.Collections.Generic;

public class Response<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    private Response(T data, bool isSuccess, string message, int statusCode = 200, List<string> errors = null)
    {
        Data = data;
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
        Errors = errors ?? new List<string>();
    }

    public static Response<T> Success(T data, string message = "")
    {
        return new Response<T>(data, true, message);
    }

    public static Response<T> Fail(string message = "Bad Request", int statusCode = 400)
    {
        return new Response<T>(default(T)!, false, message, statusCode);
    }

    public static Response<T> Fail(List<string> errors, string message = "Bad Request", int statusCode = 400)
    {
        return new Response<T>(default(T)!, false, message, statusCode, errors);
    }

    public static Response<T> Created(T data, string message = "")
    {
        return new Response<T>(data, true, message, 201);
    }

    public static Response<T> NoContent(string message = "No Content")
    {
        return new Response<T>(default(T)!, true, message, 204);
    }

    public static Response<T> Unauthorized(string message = "Unauthorized")
    {
        return new Response<T>(default(T)!, false, message, 401);
    }

    public static Response<T> Forbidden(string message = "Forbidden")
    {
        return new Response<T>(default(T)!, false, message, 403);
    }

    public static Response<T> NotFound(string message = "Not Found")
    {
        return new Response<T>(default(T)!, false, message, 404);
    }

    public static Response<T> Conflict(string message = "Conflict")
    {
        return new Response<T>(default(T)!, false, message, 409);
    }

    public static Response<T> UnprocessableEntity(string message = "Unprocessable Entity")
    {
        return new Response<T>(default(T)!, false, message, 422);
    }

    public static Response<T> Error(string message = "Internal Server Error")
    {
        return new Response<T>(default(T)!, false, message, 500);
    }
}