namespace NetScheduler.Services.Identity.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class IdentityServiceException : Exception
{
    public IdentityServiceException()
    {
    }

    public IdentityServiceException(string? message)
        : base(message)
    {
    }

    public IdentityServiceException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected IdentityServiceException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}