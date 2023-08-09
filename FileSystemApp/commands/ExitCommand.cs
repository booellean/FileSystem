namespace FileSystemApp;

class ExitCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public ExitCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Exit the File System
    public bool Execute(string[] arguments, ref string CWD, ref int userId)
    {
        Console.WriteLine("Thank you! Goodbye.");
        
        return false;
    }
}