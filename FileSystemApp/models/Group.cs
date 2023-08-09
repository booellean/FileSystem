namespace FileSystemApp;

class Group: IUnique, IPermissions
{
    public int Id { get; }
    public string Name { get; }
    public int Weight { get; }
    public CRUDX Permissions { get; }

    public Group (int id, string name, int weight, string permissions)
    {
        Id = id;
        Name = name;
        Weight = weight;
        Permissions = new CRUDX(permissions);
    }

    public bool IsRootGroup()
    {
        return Name.Equals("root");
    }

    public bool HasPermission(int index)
    {
        return Permissions.HasPermission(index);
    }

    public string GetPermissionsString()
    {
        return Permissions.GetPermissionsString();
    }
}