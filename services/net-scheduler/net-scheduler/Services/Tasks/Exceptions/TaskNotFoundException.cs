namespace NetScheduler.Services.Tasks.Exceptions;

using System;
using System.Runtime.Serialization;

public class TaskNotFoundException : Exception
{
    public TaskNotFoundException()
    {
    }

    public TaskNotFoundException(string? message)
        : base(message)
    {
    }

    public TaskNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected TaskNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}