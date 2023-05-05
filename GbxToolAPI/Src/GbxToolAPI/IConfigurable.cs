namespace GbxToolAPI;

public interface IConfigurable<TConfig> where TConfig : Config
{
    TConfig Config { get; set; }
}
