namespace NetScheduler.Services.Schedules.Exceptions;

using System;
using System.Runtime.Serialization;

internal class ScheduleNotFoundException : Exception
{
    public ScheduleNotFoundException()
    {
    }

    public ScheduleNotFoundException(string? message)
        : base(message)
    {
    }

    public ScheduleNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected ScheduleNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}