namespace GbxToolAPI;

public record TextFile(string Text, string? FileName = null)
{    
    public static explicit operator string(TextFile textFile)
    {
        return textFile.Text;
    }
}
