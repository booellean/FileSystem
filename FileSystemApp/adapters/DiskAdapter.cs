namespace FileSystemApp;

abstract class DiskAdapter
{
    public DiskAdapter() {}

    // Items that Focus on the external disk
    public abstract Node AddData(string authToken, string nodeName, string nodeType, int parentId);
    public abstract void DeleteData(string authToken, Node node);
    public abstract void MoveData(string authToken, Node targetNode, int destinationId);
    public abstract void UpdateData(string authToken, Node node, string permissions, string userId);
    
    // Items that effect the FileSystem Data directly
    public abstract User[] GetUsers(string authToken);
    public abstract Group[] GetGroups(string authToken);

    public abstract Node MountDisk(string authToken);
    public abstract void MountDiskChildren(string authToken, Node workingDirectory);

    public abstract (int, string) LoginUser(string username, string password = "");

}
