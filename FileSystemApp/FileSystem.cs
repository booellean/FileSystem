namespace FileSystemApp;

// Handles Updating Nodes, Users, and Groups
// TODO: Create a revert backup system for failed disk data moves
class FileSystem
{
    // Unique instances across all instantiations
    protected static Directory Root;
    protected static Group[] Groups;
    protected static User[] Users;
    protected static User? RootUser;
    private static FileSystem _instance;
    // DEBUG: This is fake data to login and write files/directories. Delete when reading from an actual disk
    private static DiskAdapter _disk;

    // The Mounting
    private FileSystem()
    {
        // DEBUG: Instantiate our disk adapter
        _disk = new FakeDiskAdapter();

        // Instantiate fake groups and users
        Groups = _disk.GetGroups();
        Users = _disk.GetUsers();
        RootUser = Array.Find(Users, user => user.Name.Equals("admin"));

        // Safety check
        if (RootUser == null) throw new Exception("Root user does not exist");

        // Mount Root
        int[] rootDiskAddress = Array.Empty<int>();
        Root = (Directory)_disk.MountDisk(rootDiskAddress);

        // Add Children;
        _disk.MountDiskChildren(Root);
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

    public int LoginUser(int requestingUserId)
    {
        string? input = Console.ReadLine();

        if (input != null) {
            if (input.Equals("cancel")) {
                Console.WriteLine("Cancelling Login Attempt.\n");
                return requestingUserId;
            }

            // Try to find if a user with a name matching the input exists
            User? probableUser = Array.Find(Users, user => user.Name.Equals(input));;

            if (probableUser == null) {
                Console.WriteLine("No User was found by that name. Please try again.\n");
                return LoginUser(requestingUserId);
            }

            if (probableUser.IsPasswordProtected()) {
                Console.WriteLine("This User is password protected. Please enter a password.\n");
                return VerifyUser(probableUser, requestingUserId);
            }

            return probableUser.Id;
        }

        return requestingUserId;
    }

    public int VerifyUser(User loginUser, int requestingUserId, int attempts = 1)
    {
        string? input = Console.ReadLine();

        if (loginUser.PasswordMatched(input)) return loginUser.Id;

        if (attempts > 2) {
            Console.WriteLine("Too many login attempts. Cancelling...\n");
            return requestingUserId;
        }

        return VerifyUser(loginUser, requestingUserId, ++attempts);
    }

    private User GetUser(int userId)
    {
        User? user = Array.Find(Users, user => user.Id == userId);

        if (user == null) throw new ArgumentException("No user was found with that id");
        
        return user;
    }

    private (Node, Directory?) GetNode(string directoryPath, User currentUser)
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
            _disk.MountDiskChildren(workingNode);
            // Get the next child if mounting passes
            workingNode = workingNode.GetChildNode(key);
            // Check for Read Permissions before continuing
            currentUser.CanRead(workingNode);
        }

        // One last mount of our found node
        _disk.MountDiskChildren(workingNode);

        return (workingNode, parent);
    }
            
    public string ChangeDirectory(string directoryPath, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node directory, _) = GetNode(directoryPath, currentUser);

        // This will verify it is a directory and throw an error if it is not
        // Plus extra caching!!!
        _disk.MountDiskChildren(directory);

        Console.WriteLine("Directory was changed.");

        // If a valid node was found, we will return the directoryPath back as valid
        return directoryPath;
    }

    // CRUD Directories
    // Create
    public void CreateNode(string nodeName, string directoryPath, int currentUserId)
    {
        // TODO: fix this so we can create a file outside of CWD
        if (nodeName.Contains('/')) {
            // throw error...
            throw new ArgumentException("Cannot create directories recursively.");
        }

        User currentUser = GetUser(currentUserId);
        (Node CWD, _) = GetNode(directoryPath, currentUser);

        // Check Read
        currentUser.CanCreate(CWD);

        Node? node = null;

        // TODO: If catch block goes off, revert the node Value
        try {
            int[] locationAddress = _disk.AddData(nodeName, CWD.LocalAddress, currentUser.Groups);
            node = _disk.MountDisk(locationAddress);
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
    public void ReadNode(string directoryPath, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node directory, _) = GetNode(directoryPath, currentUser);

        // Check read
        currentUser.CanRead(directory);
        // List children
        currentUser.ListDirectoryPermissions((Directory)directory);
    }

    // Update
    public void MoveNode(string nodeName, string targetDirectory, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node fileOrDirectory, Directory? parent) = GetNode(nodeName, currentUser);
        (Node destination, _) = GetNode(targetDirectory, currentUser);

        // Check user can create here
        currentUser.CanCreate(destination);
        // Check user can delete from there
        currentUser.CanDelete(fileOrDirectory);

        // TODO: If catch block goes off, revert the node Value
        try {
            if (parent != null) {
                // Get our node's new location and the old location's new nodes
                int[] locationAddress = _disk.MoveData(fileOrDirectory.LocalAddress, destination.LocalAddress);

                // Delete the node from the old directory and update child addresses
                parent.DeleteNode(fileOrDirectory);
                _disk.UpdateDiskMount(parent);

                // Assign the address to the new location, then add it to the destinatino node
                fileOrDirectory.LocalAddress = locationAddress;
                destination.AddNode(fileOrDirectory);

                // Successful!
                Console.WriteLine("File or Directory was moved successfully");
            }
        } catch(Exception error) {
            Console.WriteLine(error.Message);
        }
    }

    public void UpdateNode(string permissions, string nodeName, int currentUserId, int? targetUserId = null)
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
        (Node node, _) = GetNode(nodeName, currentUser);

        // Check can update
        currentUser.CanUpdate(node);

        _disk.UpdateData(node.LocalAddress, permissions, targetUser.Id.ToString());
        node.UpdatePermissions(targetUser.Id, permissions);
        Console.WriteLine("Successfully updated permissions.");
    }

    // Delete
    public void DeleteNode(string nodeName, int currentUserId)
    {
        User currentUser = GetUser(currentUserId);
        (Node fileOrDirectory, Directory? parent) = GetNode(nodeName, currentUser);

        // Check that they can delete from the parent or the node itself
        currentUser.CanDelete(fileOrDirectory);

        // TODO: error check
        if (parent != null && _disk.DeleteData(fileOrDirectory.LocalAddress)) {
            // This cannot be null, the check above verifies that
            parent.DeleteNode(fileOrDirectory);
            _disk.UpdateDiskMount(parent);
        }

        Console.WriteLine("File or Directory was successfully deleted.");
    }

}
