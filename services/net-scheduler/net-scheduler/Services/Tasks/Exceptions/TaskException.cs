namespace NetScheduler.Services.Tasks.Exceptions;

using System;
using System.Runtime.Serialization;

internal class TaskException : Exception
{
    public TaskException()
    {
    }

    public TaskException(string? message)
        : base(message)
    {
    }

    public TaskException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected TaskException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}