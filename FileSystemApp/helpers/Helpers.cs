namespace FileSystemApp;

class Helpers
{
    public static string ConformDirectory(string input, string CWD)
    {
        if (input.Equals("")) return CWD;

        if (input[0] == '/') return input;

        return CWD + (CWD.Equals("/") ? "" : "/") + input;

    }
}