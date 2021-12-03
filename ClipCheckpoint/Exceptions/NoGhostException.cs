using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BigBang1112.ClipCheckpoint.Exceptions;

public class NoGhostException : Exception
{
    public NoGhostException() : base(message: "No ghost was found.")
    {

    }

    public NoGhostException(string? message) : base(message)
    {

    }

    public NoGhostException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}
