namespace NetScheduler.Services.Tasks;

using System;
using System.Runtime.Serialization;

[Serializable]
internal class InvalidTaskException : Exception
{
    public InvalidTaskException()
    {
    }

    public InvalidTaskException(string? message) : base(message)
    {
    }

    public InvalidTaskException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected InvalidTaskException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}