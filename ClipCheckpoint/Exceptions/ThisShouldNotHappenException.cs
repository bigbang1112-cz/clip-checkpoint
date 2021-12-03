using System.Runtime.Serialization;

namespace BigBang1112.ClipCheckpoint.Exceptions;

public class ThisShouldNotHappenException : Exception
{
    public ThisShouldNotHappenException() : base(message: "This should not happen.")
    {

    }

    public ThisShouldNotHappenException(string? message) : base(message)
    {

    }

    public ThisShouldNotHappenException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}
