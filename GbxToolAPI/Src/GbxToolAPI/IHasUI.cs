namespace GbxToolAPI;

public interface IHasUI
{
    Task LoadAsync(CancellationToken cancellationToken = default);
}
