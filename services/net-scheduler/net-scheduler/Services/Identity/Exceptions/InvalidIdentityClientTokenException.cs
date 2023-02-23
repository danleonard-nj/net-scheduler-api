namespace NetScheduler.Services.Identity.Exceptions;

using System;
using System.Runtime.Serialization;

public class InvalidIdentityClientTokenException : Exception
{
    public InvalidIdentityClientTokenException()
    {
    }

    public InvalidIdentityClientTokenException(string? message)
        : base(message)
    {
    }

    public InvalidIdentityClientTokenException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected InvalidIdentityClientTokenException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}