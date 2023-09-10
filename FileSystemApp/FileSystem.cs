namespace FileSystemApp;

// Handles Updating Nodes, Users, and Groups
// TODO: Create a revert backup system for failed disk data moves
class FileSystem
{
    // Unique instances across all instantiations
    protected static Directory? Root = null;
    protected static Group[] Groups;
    protected static User[] Users;
    private static FileSystem _instance;
    // DEBUG: This is fake data to login and write files/directories. Delete when reading from an actual disk
    private static DiskAdapter _disk;

    // The Mounting
    private FileSystem()
    {
        // DEBUG: Instantiate our disk adapter
        _disk = new ApiDiskAdapter();
    }

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
        if (Root == null) {
            // Instantiate groups and users
            Groups = _disk.GetGroups(authToken);
            Users = _disk.GetUsers(authToken);

            // Mount Root
            Root = (Directory)_disk.MountDisk(authToken);

            // Add Children
            // _disk.MountDiskChildren(authToken, Root);
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
    public void CreateNode(string authToken, string nodeName, string directoryPath, int currentUserId)
    {
        // TODO: fix this so we can create a file outside of CWD
        if (nodeName.Contains('/')) {
            // throw error...
            throw new ArgumentException("Cannot create directories recursively.");
        }

        User currentUser = GetUser(currentUserId);
        (Node CWD, _) = GetNode(authToken, directoryPath, currentUser);

        // Check Read
        currentUser.CanCreate(CWD);

        Node? node = null;

        // TODO: If catch block goes off, revert the node Value
        try {
            int nodeId = _disk.AddData(authToken, nodeName, CWD.Id, currentUser.Groups);
            node = _disk.MountDisk(authToken, nodeId);
            CWD.AddNode(node);
            Console.WriteLine("Successfully created item.");
        } catch(Exception error) {
            if (node != null) {
                // If node was written, we want to make sure it wasn't added to the directory
            }
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

        // Check user can create here
        currentUser.CanCreate(destination);
        // Check user can delete from there
        currentUser.CanDelete(fileOrDirectory);

        // TODO: If catch block goes off, revert the node Value
        try {
            if (parent != null) {
                // Get our node's new location and the old location's new nodes
                _disk.MoveData(authToken, fileOrDirectory.Id, destination.Id);

                // Delete the node from the old directory and update child addresses
                parent.DeleteNode(fileOrDirectory);
                _disk.UpdateDiskMount(authToken, parent);

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

        _disk.UpdateData(authToken, node.Id, permissions, targetUser.Id.ToString());
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

        // TODO: error check
        if (parent != null && _disk.DeleteData(authToken, fileOrDirectory.Id)) {
            // This cannot be null, the check above verifies that
            parent.DeleteNode(fileOrDirectory);
            _disk.UpdateDiskMount(authToken, parent);
        }

        Console.WriteLine("File or Directory was successfully deleted.");
    }

}
