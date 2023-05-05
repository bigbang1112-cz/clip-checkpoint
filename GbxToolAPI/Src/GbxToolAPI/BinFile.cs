namespace GbxToolAPI;

public record BinFile(byte[] Data, string? FileName = null)
{
    public static explicit operator byte[](BinFile binFile)
    {
        return binFile.Data;
    }
}
