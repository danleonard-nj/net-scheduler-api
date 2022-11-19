namespace NetScheduler.Clients.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class TaskClientException : Exception
{
    public TaskClientException()
    {
    }

    public TaskClientException(string? message) : base(message)
    {
    }

    public TaskClientException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected TaskClientException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}