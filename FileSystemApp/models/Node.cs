namespace FileSystemApp;
using System.Text.Json.Serialization;

abstract class Node : IUnique, IGroups
{
    [JsonPropertyName("id")]
    public int Id { get; }
    [JsonPropertyName("name")]
    public string Name { get; }
    [JsonPropertyName("userPermissions")]
    public Dictionary<string, string>? UserPermissions { get; }
    protected Dictionary<int, CRUDX> Permissions = new Dictionary<int, CRUDX>();
    [JsonPropertyName("groups")]
    public Group[] Groups { get; }

    [JsonConstructor]
    public Node(int id, string name, Group[] groups, Dictionary<string, string>? userPermissions)
    {
        Id = id;
        Name = name;
        Groups = groups;
        UserPermissions = userPermissions;

        if (UserPermissions != null) {
            foreach(KeyValuePair<string, string> entry in UserPermissions)
            {
                Permissions.Add(int.Parse(entry.Key), new CRUDX(entry.Value));
            }
        }
    }

    // Common functions amongst files and directories
    public abstract string DataType();

    public abstract string GetKey();
    public abstract string ConformPermissions(string permissions);


    // Directory specific Functions. Files will throw a not implemented exception
    public abstract Node GetChildNode(string nodeKey);
    public abstract bool NodeIsMounted();
    public abstract void SetNodeMounted();
    public abstract void AddNode(Node node);
    public abstract void DeleteNode(Node node);

    public bool IsFile()
    {
        if (String.Compare(DataType(), "file") == 0) return true;
        return false;
    }

    public void UpdatePermissions(int userId, string permissions)
    {
        if (permissions.Length != 5) {
            throw new ArgumentException("Invalid length of permissions passed. Must be at least 5.");
        }

        // This makes sure executable permissions are turned off for non-files and on for files
        ConformPermissions(permissions);

        Permissions.Remove(userId);
        Permissions.Add(userId, new CRUDX(permissions));
    }

    public bool PermissionsHaveUser(int userId)
    {
        return Permissions.ContainsKey(userId);
    }

    public bool UserHasPermission(int index, int userId)
    {
        // If the Node's Permissions property has a custom set for the user
        if (PermissionsHaveUser(userId)) {
            return Permissions[userId].HasPermission(index);
        }

        // Otherwise no way
        return false;
    }

    public string GetUserPermissionsString(int userId)
    {
        // If the Node's Permissions property has a custom set for the user
        if (PermissionsHaveUser(userId)) {
            return Permissions[userId].GetPermissionsString();
        }

        // Otherwise no way
        return "00000";
    }

    // For files and dirctories, "IsRoot" is checking if it is only root
    public bool IsRoot()
    {
        return Groups.Length == 1 && Groups[0].Name == "root";
    }
}
