namespace FileSystemApp;

abstract class DiskAdapter
{
    public DiskAdapter() {}

    // Items that Focus on the external disk
    public abstract int[] AddData(string nodeName, int[] parentAddress, Group[] groups);
    public abstract bool DeleteData(int[] locationAddress);
    public abstract int[] MoveData(int[] targetNodeAddress, int[] destinationAddress);
    public abstract void UpdateData(int[] locationAddress, string permissions, string userId);
    
    // Items that effect the FileSystem Data directly
    public abstract User[] GetUsers();
    public abstract Group[] GetGroups();

    public abstract Node MountDisk(int[] locationAddress);
    public abstract void MountDiskChildren(Node workingDirectory);
    public abstract void UpdateDiskMount(Node workingDirectory);

}
