namespace ClipCheckpoint.Exceptions;

[Serializable]
public class NoCheckpointsException : Exception
{
	public NoCheckpointsException() : this("No checkpoints available in ghost.") { }
	public NoCheckpointsException(string message) : base(message) { }
	public NoCheckpointsException(string message, Exception inner) : base(message, inner) { }
	protected NoCheckpointsException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
