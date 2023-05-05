namespace GbxToolAPI.CLI;

[Serializable]
public class ConsoleFailException : Exception
{
    public ConsoleFailException() { }
    public ConsoleFailException(string message) : base(message) { }
    public ConsoleFailException(string message, Exception inner) : base(message, inner) { }
    protected ConsoleFailException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
