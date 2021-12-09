using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigBang1112.ClipCheckpoint.Exceptions;

public class CheckpointIsMinusOneException : Exception
{
    public CheckpointIsMinusOneException() : base(message: "The checkpoint time is not valid and the process cannot continue.")
    {

    }

    public CheckpointIsMinusOneException(string? message) : base(message)
    {

    }

    public CheckpointIsMinusOneException(string? message, Exception? innerException) : base(message, innerException)
    {

    }
}
