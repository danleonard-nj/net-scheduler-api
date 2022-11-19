namespace NetScheduler.Services.Identity.Exceptions;

using System;
using System.Runtime.Serialization;

public class InvalidIdentityClientException : Exception
{
    public InvalidIdentityClientException()
    {
    }

    public InvalidIdentityClientException(string? message)
        : base(message)
    {
    }

    public InvalidIdentityClientException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected InvalidIdentityClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}