namespace FileSystemApp;

class LoginCommand: ICommand
{
    public FileSystem Receiver { get; }

    // Pass the FileSystem as a receiver for commands
    public LoginCommand(FileSystem receiver) {
        Receiver = receiver;
    }
    // Execute a login request
    public bool Execute(string[] arguments, ref string CWD, ref int userId, ref string authToken)
    {
        Console.WriteLine("Please Enter your login information");
        (int id, string token) = Receiver.LoginUser();
        userId = id;
        authToken = token;

        return true;
    }
}