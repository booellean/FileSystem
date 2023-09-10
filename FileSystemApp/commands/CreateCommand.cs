namespace FileSystemApp;

class CreateCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public CreateCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a file or directory creation
    public bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken)
    {
        // TODO: more thorough error checking
        if (arguments.Length < 3 || arguments.Length > 4) {
            throw new ArgumentException("Please enter the proper amount of arguments.");
        }

        string nodeName = arguments[2];
        string nodeType = arguments[1];
        string targetDirectory = CWD;

        if (arguments.Length == 4) targetDirectory = Helpers.ConformDirectory(arguments[2], CWD);

        // This will throw an error if the directory wasn't found
        Receiver.CreateNode(authToken, nodeName, nodeType, targetDirectory, userId);

        return true;
    }
}