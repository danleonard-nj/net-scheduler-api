namespace NetScheduler.Services.Events.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class EventBatchDispatchException : Exception
{
    public EventBatchDispatchException()
    {
    }

    public EventBatchDispatchException(string? message) : base(message)
    {
    }

    public EventBatchDispatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected EventBatchDispatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}