namespace FileSystemApp;

abstract class DiskAdapter
{
    public DiskAdapter() {}

    // Items that Focus on the external disk
    public abstract int AddData(string authToken, string nodeName, int parentId, Group[] groups);
    public abstract bool DeleteData(string authToken, int locationId);
    public abstract void MoveData(string authToken, int targetNodeId, int destinationId);
    public abstract void UpdateData(string authToken, int locationId, string permissions, string userId);
    
    // Items that effect the FileSystem Data directly
    public abstract User[] GetUsers(string authToken);
    public abstract Group[] GetGroups(string authToken);

    public abstract Node MountDisk(string authToken, int locationId = 0);
    public abstract void MountDiskChildren(string authToken, Node workingDirectory);
    public abstract void UpdateDiskMount(string authToken, Node workingDirectory);

    public abstract (int, string) LoginUser(string username, string? password = "");

}
