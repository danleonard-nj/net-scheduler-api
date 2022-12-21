namespace NetScheduler.Services.Schedules;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class InvalidScheduleException : Exception
{
    public InvalidScheduleException()
    {
    }

    public InvalidScheduleException(string? message) : base(message)
    {
    }

    public InvalidScheduleException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidScheduleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}