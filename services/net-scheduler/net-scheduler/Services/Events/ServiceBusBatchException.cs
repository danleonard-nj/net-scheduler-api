namespace NetScheduler.Services.Events;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class ServiceBusBatchException : Exception
{
    public ServiceBusBatchException()
    {
    }

    public ServiceBusBatchException(string? message) : base(message)
    {
    }

    public ServiceBusBatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ServiceBusBatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}