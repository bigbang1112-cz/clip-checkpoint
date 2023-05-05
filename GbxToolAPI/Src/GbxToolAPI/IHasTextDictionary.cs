namespace GbxToolAPI;

public interface IHasTextDictionary<T> where T : ITextDictionary
{
    T Dictionary { get; }
}
