using System.Runtime.Serialization;

namespace BigBang1112.ClipCheckpoint.Exceptions;

public class NoCheckpointsException : Exception
{
    public NoCheckpointsException() : base(message: "No checkpoints available in ghost.")
    {

    }

    public NoCheckpointsException(string? message) : base(message)
    {

    }

    public NoCheckpointsException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}
