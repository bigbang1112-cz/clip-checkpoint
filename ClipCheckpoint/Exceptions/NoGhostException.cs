namespace ClipCheckpoint.Exceptions;

[Serializable]
public class NoGhostException : Exception
{
    public NoGhostException() : this("No ghost was found.") { }
    public NoGhostException(string message) : base(message) { }
    public NoGhostException(string message, Exception inner) : base(message, inner) { }
    protected NoGhostException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
