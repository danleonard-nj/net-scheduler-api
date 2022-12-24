namespace NetScheduler.Services.Events.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class InvalidEventConfigurationException : Exception
{
    public InvalidEventConfigurationException()
    {
    }

    public InvalidEventConfigurationException(string? message) : base(message)
    {
    }

    public InvalidEventConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidEventConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}