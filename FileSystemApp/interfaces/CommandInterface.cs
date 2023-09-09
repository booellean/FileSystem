namespace FileSystemApp;

interface ICommand
{
    FileSystem Receiver { get; }
    bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken);
}