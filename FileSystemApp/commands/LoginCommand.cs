namespace FileSystemApp;

class LoginCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public LoginCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a login request
    public bool Execute(string[] arguments, ref string CWD, ref int userId)
    {
        Console.WriteLine("Please Enter your login information");
        userId = Receiver.LoginUser(userId);

        return true;
    }
}