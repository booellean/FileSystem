namespace FileSystemApp;

class MoveCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public MoveCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a file or directory move to a new location
    public bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken)
    {
        // TODO: more thorough error checking
        if (arguments.Length != 3) {
            throw new ArgumentException("Please enter the proper amount of arguments.");
        }

        string movingNode = Helpers.ConformDirectory(arguments[1], CWD);
        string targetDirectory = Helpers.ConformDirectory(arguments[2], CWD);

        Receiver.MoveNode(authToken, movingNode, targetDirectory, userId);
        
        return true;
    }
}