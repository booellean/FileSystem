namespace FileSystemApp;

// Handles Updating Nodes, Users, and Groups
// TODO: Create a revert backup system for failed disk data moves
class FileSystem
{
    // Unique instances across all instantiations
    protected static Directory Root = new Directory(-1, "Null", new Group[0], null);
    protected static Group[] Groups = new Group[0];
    protected static User[] Users = new User[0];
    private static FileSystem _instance;
    private static DiskAdapter _disk = new ApiDiskAdapter();

    // Our Singleton!
    public static FileSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new FileSystem();

            return _instance;
        }
    }

    public (int, string) LoginUser()
    {
        string? input = Console.ReadLine();

        if (input != null) {
            if (input.Equals("cancel")) {
                Console.WriteLine("Cancelling Login Attempt.\n");
                return (-1,"");
            }

            try {
                return _disk.LoginUser(input);
            } catch(PasswordNeededException error) {
                Console.WriteLine(error.Message);
                return LoginUserWithPassword(input);
            } catch(Exception error) {
                Console.WriteLine(error.ToString());
            }
            
        }

        return (-1,"");
    }

    private (int, string) LoginUserWithPassword(string username, int attempts = 3)
    {
        if (attempts < 1) throw new Exception("Password attempt failed. Unauthorized.");

        string? password = Console.ReadLine();
        try {
            return _disk.LoginUser(username, password);
        } catch(PasswordNeededException error) {
            Console.WriteLine(error.Message);
        }

        attempts--;
        return LoginUserWithPassword(username, attempts);
    }

    public void FinalizeSetup(string authToken)
    {
        if (Root.Id == -1) {
            // Instantiate groups and users
            Groups = _disk.GetGroups(authToken);
            Users = _disk.GetUsers(authToken);

            // Mount Root
            Root = (Directory)_disk.MountDisk(authToken);

            // Add Children
            _disk.MountDiskChildren(authToken, Root);
        }
    }

    private User GetUser(int userId)
    {
        User? user = Array.Find(Users, user => user.Id == userId);

        if (user == null) throw new ArgumentException("No user was found with that id");
        
        return user;
    }

    private (Node, Directory?) GetNode(string authToken, string directoryPath, User currentUser)
    {
        // If the string is just root name, return the root
        if (directoryPath.Equals("/")) return (Root, null);

        // Get path names of nodes
        string[] pathKeys = directoryPath.Split(new[]{'/'}, StringSplitOptions.RemoveEmptyEntries);

        // We will always start with our root node
        Node workingNode = Root;
        Directory? parent = null;

        for(int ii = 0; ii < pathKeys.Length; ii++)
        {
            string key = pathKeys[ii];
            parent = (Directory)workingNode;

            // Make sure directory disk has been mounted
            _disk.MountDiskChildren(authToken, workingNode);
            // Get the next child if mounting passes
            workingNode = workingNode.GetChildNode(key);
            // Check for Read Permissions before continuing
            currentUser.CanRead(workingNode);
        }

        // One last mount of our found node
        _disk.MountDiskChildren(authToken, workingNode);

        return (workingNode, parent);
    }
            
    public string ChangeDirectory(string authToken, string directoryPath, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node directory, _) = GetNode(authToken, directoryPath, currentUser);

        // This will verify it is a directory and throw an error if it is not
        // Plus extra caching!!!
        _disk.MountDiskChildren(authToken, directory);

        Console.WriteLine("Directory was changed.");

        // If a valid node was found, we will return the directoryPath back as valid
        return directoryPath;
    }

    // CRUD Directories
    // Create
    public void CreateNode(string authToken, string nodeName, string nodeType, string directoryPath, int currentUserId)
    {
        if (nodeName.Contains('/')) {
            // throw error...
            throw new ArgumentException("Cannot create directories recursively.");
        }

        User currentUser = GetUser(currentUserId);
        (Node CWD, _) = GetNode(authToken, directoryPath, currentUser);

        // Check Read
        currentUser.CanCreate(CWD);

        try {
            Node node = _disk.AddData(authToken, nodeName, nodeType, CWD.Id);
            CWD.AddNode(node);
            Console.WriteLine("Successfully created item.");
        } catch(Exception error) {
            Console.WriteLine(error.Message);
        }
    }

    // Read
    public void ReadNode(string authToken, string directoryPath, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node directory, _) = GetNode(authToken, directoryPath, currentUser);

        // Check read
        currentUser.CanRead(directory);
        // List children
        currentUser.ListDirectoryPermissions((Directory)directory);
    }

    // Update
    public void MoveNode(string authToken, string nodeName, string targetDirectory, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node fileOrDirectory, Directory? parent) = GetNode(authToken, nodeName, currentUser);
        (Node destination, _) = GetNode(authToken, targetDirectory, currentUser);

        // TODO: If catch block goes off, revert the node Value
        try {
            if (parent != null) {
                // Checks before we actually make our calls
                // Check user can create in wanted destination
                currentUser.CanCreate(destination);
                // Check user can delete from current location
                currentUser.CanUpdate(parent);
                // Check user can update the node
                currentUser.CanUpdate(fileOrDirectory);

                // Get our node's new location and the old location's new nodes
                _disk.MoveData(authToken, fileOrDirectory, destination.Id);

                // Delete the node from the old directory and update child addresses
                parent.DeleteNode(fileOrDirectory);

                // Assign the address to the new location, then add it to the destinatino node
                destination.AddNode(fileOrDirectory);

                // Successful!
                Console.WriteLine("File or Directory was moved successfully");
            }
        } catch(Exception error) {
            Console.WriteLine(error.Message);
        }
    }

    public void UpdateNode(string authToken, string permissions, string nodeName, int currentUserId, int? targetUserId = null)
    {
        User currentUser = GetUser(currentUserId);
        User targetUser = currentUser;

        // Assign user if target is different than current
        if (targetUserId != null) {
            targetUser = GetUser((int)targetUserId);
        }

        // Check if current user has proper permissions compared to target user
        currentUser.HasElevatedStatus(targetUser);

        // Get Node
        (Node node, _) = GetNode(authToken, nodeName, currentUser);

        // Check can update
        currentUser.CanUpdate(node);

        _disk.UpdateData(authToken, node, permissions, targetUser.Id.ToString());
        node.UpdatePermissions(targetUser.Id, permissions);
        Console.WriteLine("Successfully updated permissions.");
    }

    // Delete
    public void DeleteNode(string authToken, string nodeName, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node fileOrDirectory, Directory? parent) = GetNode(authToken, nodeName, currentUser);

        // Check that they can delete from the parent or the node itself
        currentUser.CanDelete(fileOrDirectory);

        if (parent != null) {
            _disk.DeleteData(authToken, fileOrDirectory);
            // This cannot be null, the check above verifies that
            parent.DeleteNode(fileOrDirectory);
        }

        Console.WriteLine("File or Directory was successfully deleted.");
    }

}
