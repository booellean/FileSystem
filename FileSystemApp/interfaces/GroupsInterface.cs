namespace FileSystemApp;


interface IGroups
{
    Group[] Groups { get; }
    bool IsRoot();
}