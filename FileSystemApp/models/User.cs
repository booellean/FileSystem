namespace FileSystemApp;

class User: IUnique, IGroups, ICRUDX
{
    public int Id { get; }
    public string Name { get; }
    public Group[] Groups { get; }
    private string? Password;

    public User(int id, string name, Group[] groups, string? password = null)
    {
        Id = id;
        Name = name;
        Groups = groups;
        Password = password;
    }

    public bool IsRoot()
    {
        for (int ii = 0; ii < Groups.Length; ii++) {
            Group group = Groups[ii];

            if (group.IsRootGroup()) return true;
        }

        return false;
    }

    public void HasElevatedStatus(User user)
    {
        // If this object user is not root but passed user is...
        if (user.IsRoot() && !IsRoot()) {
            // throw error...
            throw new ArgumentException("Insufficient permissions to update permissions for this user.");
        }   
    }

    public bool IsPasswordProtected()
    {
        return Password != null;
    }

    public bool PasswordMatched(string input)
    {
        return Password != null && Password.Equals(input);
    }

    public Group? SharedGroup(Group[] nodeGroups) {
        Group? highestRankedGroup = null;
        // Hashtable to verify that group was found without endless looping;
        Dictionary<int, int> passed = new Dictionary<int, int>();

        for (int ii = 0; ii < Groups.Length; ii++) {
            int userGroupId = Groups[ii].Id;
            passed.Add(userGroupId, ii);
        }

        for (int ii = 0; ii < nodeGroups.Length; ii++) {
            int nodeGroupId = nodeGroups[ii].Id;

            if (passed.ContainsKey(nodeGroupId)) {
                int index = passed[nodeGroupId];
                Group sharedGroup = Groups[index];

                if (
                    highestRankedGroup == null || 
                    sharedGroup.Weight < highestRankedGroup.Weight
                ) {
                    highestRankedGroup = sharedGroup;
                }
            }
        }

        // return null or Group
        return highestRankedGroup;
    }

    // List Directory Permissions
    public void ListDirectoryPermissions(Directory directory)
    {
        // Write a Table Header
        Console.WriteLine("\n| {0,-20} | {1,20} |", "Permissions (CRUDX)", "Items");
        Console.WriteLine("-----------------------------------------------");

        // List Directories...
        WriteChildrenContentsToConsole(directory, directory.GetOrderedDirectories());
        // List Files...
        WriteChildrenContentsToConsole(directory, directory.GetOrderedFiles());
        
        // Give us some reading space
        Console.WriteLine("");
    }

    
    private void WriteChildrenContentsToConsole(Directory directory, string[] array)
    {
        for(int ii = 0; ii < array.Length; ii++)
        {
            string key = array[ii];
            Node child = directory.GetChildNode(key);
            string perms = GetUserPermissionString(child);
            Console.WriteLine("| {0,-20} | {1,20} |", perms, key);
        }
    }

    // TODO change
    public string GetUserPermissionString(Node node)
    {
        // If the Node's Permissions property has a custom set for the user
        if (node.PermissionsHaveUser(Id)) {
            return node.GetUserPermissionsString(Id);
        }

        // Root will override any group except for custom permissions set to the user
        if (IsRoot()) {
            return node.ConformPermissions("11111");
        }

        // Otherwise check if the user shares a group with the Node
        Group? sharedGroup = SharedGroup(node.Groups); 
        if (sharedGroup != null) {
            return sharedGroup.Permissions.GetPermissionsString();
        }

        // Otherwise no way
        return "00000";
    }

    // CRUDX
    public void CanCreate(Node node)
    {
        if (!HasPermission(0, node)) throw new ArgumentException("You are not permitted to create here.");
    }

    public void CanRead(Node node)
    {
        if (!HasPermission(1, node)) throw new ArgumentException("You are not permitted to read that {0}.", node.DataType());
    }

    public void CanUpdate(Node node)
    {
        if (!HasPermission(2, node)) throw new ArgumentException("You are not permitted to update this {0}.", node.DataType());
    }

    public void CanDelete(Node? node)
    {
        // If a user tried to delete a directory that doesn't exist (usually root)
        if ( node == null ) throw new ArgumentException("Cannot Delete the root directory.");
    
        // If a user is trying to delete a directory with nodes in it
        if (String.Compare(node.DataType(), "directory") == 0 && ((Directory)node).Nodes.Count > 0)
        {
            throw new ArgumentException("Cannot delete a directory with files in it.");
        }

        // Otherwise if they just don't have this permission...
        if (!HasPermission(3, node)) throw new ArgumentException("You are not permitted to delete this {0}.", node.DataType());
    }

    public void CanExecute(Node node)
    {
        // If the Node is not a file but we are checking the eXecutable permission, always return false
        if (!node.IsFile()) throw new ArgumentException("You cannot \"execute\" a directory.");

        if (!HasPermission(4, node)) throw new ArgumentException("You are not permitted to execute this file.");
    }

    private bool HasPermission(int index, Node node)
    {
        // If the Node's Permissions property has a custom set for the user
        if (node.PermissionsHaveUser(Id)) return node.UserHasPermission(index, Id);

        // Root will override any group except for custom permissions set to the user
        if (IsRoot()) return true;

        // Otherwise check if the user shares a group with the Node
        Group? sharedGroup = SharedGroup(node.Groups); 
        if (sharedGroup != null) {
            return sharedGroup.Permissions.HasPermission(index);
        }

        // Otherwise no way
        return false;
    }
}