namespace FileSystemApp;

public class PasswordNeededException : Exception
{
    public PasswordNeededException() : base() { }
    public PasswordNeededException(string message) : base(message) { }
    public PasswordNeededException(string message, Exception inner) : base(message, inner) { }
}