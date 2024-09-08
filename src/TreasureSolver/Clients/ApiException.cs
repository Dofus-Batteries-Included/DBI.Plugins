using System;
using System.Collections.Generic;

namespace DofusBatteriesIncluded.Plugins.TreasureSolver.Clients;

public class ApiException : Exception
{
    public int StatusCode { get; private set; }

    public string Response { get; }

    public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; private set; }

    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, Exception innerException) : base(
        message + "\n\nStatus: " + statusCode + "\nResponse: \n" + (response == null ? "(null)" : response[..(response.Length >= 512 ? 512 : response.Length)]),
        innerException
    )
    {
        StatusCode = statusCode;
        Response = response;
        Headers = headers;
    }

    public override string ToString() => $"HTTP Response: \n\n{Response}\n\n{base.ToString()}";
}

public class ApiException<TResult> : ApiException
{
    public TResult Result { get; private set; }

    public ApiException(string message, int statusCode, string response, IReadOnlyDictionary<string, IEnumerable<string>> headers, TResult result, Exception innerException) : base(
        message,
        statusCode,
        response,
        headers,
        innerException
    )
    {
        Result = result;
    }
}
