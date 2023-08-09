namespace FileSystemApp;

class RelocateCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public RelocateCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a directory change
    public bool Execute(string[] arguments, ref string CWD, ref int userId)
    {
        // TODO: more thorough error checking
        if (arguments.Length != 2) {
            throw new ArgumentException("Please enter the proper amount of arguments.");
        }

        string targetDirectory = Helpers.ConformDirectory(arguments[1], CWD);

        // This will throw an error if the directory wasn't found
        Receiver.ChangeDirectory(targetDirectory, userId);
        CWD = targetDirectory;
        
        return true;
    }
}