namespace FileSystemApp;

class ListCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public ListCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a read of the current working directory
    public bool Execute(string[] arguments, ref string CWD, ref int userId)
    {
        Receiver.ReadNode(CWD, userId);
        
        return true;
    }
}