namespace ClipCheckpoint.Exceptions;

[Serializable]
public class CheckpointIsMinusOneException : Exception
{
	public CheckpointIsMinusOneException() : this("The checkpoint time is not valid and the process cannot continue.") { }
	public CheckpointIsMinusOneException(string message) : base(message) { }
	public CheckpointIsMinusOneException(string message, Exception inner) : base(message, inner) { }
	protected CheckpointIsMinusOneException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
