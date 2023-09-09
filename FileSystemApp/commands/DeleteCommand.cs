namespace FileSystemApp;

class DeleteCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public DeleteCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a file or directory deletion
    public bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken)
    {
        // TODO: more thorough error checking
        if (arguments.Length != 2) {
            throw new ArgumentException("Please enter the proper amount of arguments.");
        }

        string nodeName = Helpers.ConformDirectory(arguments[1], CWD);

        // This will throw an error if the directory wasn't found
        Receiver.DeleteNode(authToken, nodeName, userId);

        return true;
    }
}