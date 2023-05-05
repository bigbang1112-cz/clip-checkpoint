namespace GbxToolAPI.CLI;

public class NadeoIni
{
    public required string UserDataDir { get; init; }

    public static NadeoIni Parse(string nadeoIniFilePath)
    {
        var anyOfDirs = false;
        var userSubDir = "TmForever";
        var userDir = default(string);

        foreach (var line in File.ReadLines(nadeoIniFilePath))
        {
            if (line.Length == 0 || line[0] is '#' or ';' or '[')
            {
                continue;
            }

            if (line.StartsWith("UserSubDir="))
            {
                userSubDir = line[11..];
                anyOfDirs = true;
            }

            if (line.StartsWith("UserDir="))
            {
                userDir = line[8..];
                anyOfDirs = true;
            }
        }

        if (!anyOfDirs)
        {
            throw new Exception("No UserSubDir or UserDir found.");
        }

        var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        string userDataDir;

        if (!string.IsNullOrWhiteSpace(userDir))
        {
            userDataDir = userDir.Replace("{userdocs}", myDocs);
        }
        else if (!string.IsNullOrWhiteSpace(userSubDir))
        {
            userDataDir = Path.Combine(myDocs, userSubDir);
        }
        else
        {
            throw new Exception("Nadeo.ini contains invalid UserDir/UserSubDir combination.");
        }

        return new NadeoIni
        {
            UserDataDir = userDataDir
        };
    }
}