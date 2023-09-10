using System.Text.Json.Serialization;

namespace FileSystemApp;

class Group: IUnique, IPermissions
{
    [JsonConstructor]
    public Group (int id, string name, int weight, string permissionString)
    {
        Id = id;
        Name = name;
        Weight = weight;
        PermissionString = permissionString;
        Permissions = new CRUDX(PermissionString);
    }

    [JsonPropertyName("id")]
    public int Id { get; }
    [JsonPropertyName("name")]
    public string Name { get; }
    [JsonPropertyName("weight")]
    public int Weight { get; }
    [JsonPropertyName("permissionString")]
    public string PermissionString { get; }
    public CRUDX Permissions { get; }

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