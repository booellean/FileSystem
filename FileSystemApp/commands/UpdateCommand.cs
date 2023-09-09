namespace FileSystemApp;

class UpdateCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public UpdateCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a file or directory permissions update
    public bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken)
    {
        // TODO: more thorough error checking
        if (arguments.Length != 3) {
            throw new ArgumentException("Please enter the proper amount of arguments.");
        }

        string permissions = arguments[1];
        string nodeName = Helpers.ConformDirectory(arguments[2], CWD);

        // This will throw an error if the directory wasn't found
        Receiver.UpdateNode(authToken, permissions, nodeName, userId);

        return true;
    }
}