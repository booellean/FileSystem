/**
Assignment 3 - Design Principles: SOLID

Code is run using dotnet 7. Have not tested in versions above.

An Adapter Class "DiskAdapter" has been added. This class is abstract and will handle reading, writing, and updating data on a disk outside of the "FileSystem" class Objects. Currently, the disk adapter being used is "FakeDiskAdapter" which reads a JSON file "fake_dat.json" and updates it when interfacing with the filesystem. There is a backup file "backupDebug.json" for testing purposes. "DiskNodeData", "FakeUser", and "FakeGroups" are debug classes that work with the DiskAdapter, but "DiskNodeData" is also used in "FileSystem" when mounting new nodes. This class will need updating since it's dependent on how disk data will be received later in the project, but "FakeUser" and "FakeGroup" only exist because of how the JSON is deserialized and will not be used later.

A new class has been added to handle Permissons called "CRUDX". This helps to conform how permissions are utilized throughout classes, but it also serves to handle common methods such as reading a permission index, converting permisssion strings into readable boolean arrays, and converting those arrays back to permission strings.

Node classes now have an integer array property called "LocalAddress". This address points to the location of the data on the psuedo disk. The array is a series of indexes that can be read as "levels". For example, and empty array will point to Root. An array with value [1], would point to the second item on Root's nodes array. An array with value [1,2] would point to the third item in the second item of Root's nodes array, and etc. When items are moved or deleted. When the "DiskAdapter" moves, deltes, or adds items, it will return new integer arrays to be updated to the node's property.

There was an error in the original "DeleteNode" and "AddNode" methods of the "Node" class where the key names were not actually maintening a sorted array. This error has been fixed.

Other minor changes have been made throughout the "FileSystem", "User", and "Group" classes to better work with the disk adapter.

Finally, the "FileSystemCommand" class has a new method called "Login" that is called at startup. This method can login any users recognized by the "FileSystem" class. To test this app as root user, the user name is "admin" and the password is "admin".
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;

namespace FileSystemApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Application...");

        FileSystemCommand fileSystem = new FileSystemCommand();

        Console.WriteLine("Application Startup Successful!\n");

        Console.WriteLine("You may exit the application at any time by typing \"exit\".");

        Console.WriteLine("Before you get started, please login. (Hint: you can enter as a guest by typing \"guest\").\n");
        fileSystem.Login();
    }

    protected class FileSystemCommand
    {
        // Flexible assignment from instance to instance, but uses the same unique instantiations
        protected static string CWD = "/";
        protected static int _userId = -1;
        protected FileSystem System = FileSystem.Instance;
        
        public FileSystemCommand() {}

        public void Login()
        {
            string? command = Console.ReadLine();

            if (command != null) {
                switch (command) {
                    case "exit":
                        Console.WriteLine("Thank you! Goodbye.");
                        return;
                    case "cancel":
                        if (_userId > -1) {
                            Console.WriteLine("Cancelling Login Attempt.\n");
                            Listen();
                            return;
                        } else {
                            Console.WriteLine("Thank you! Goodbye.");
                            return;
                        }
                    default:
                        int userScenario = System.FindUser(command);

                        if (userScenario == -1) {
                            Console.WriteLine("No User was found by that name. Please try again.\n");
                        } else if(userScenario == -2) {
                            Console.WriteLine("This User is password protected. Please enter a password.\n");
                            VerifyPassword(command);
                            return;
                        } else {
                            _userId = userScenario;
                            Listen();
                            return;
                        }
                        break;
                }

                Login();

            } else {
                Console.WriteLine("Please login before or cancel.\n");
            }
        }

        public void VerifyPassword(string userName)
        {
            string? passwordInput = Console.ReadLine();

            if (passwordInput != null) {
                int probableId = System.VerifyUser(userName, passwordInput);
                if (probableId > -1) {
                    _userId = probableId;
                    Listen();
                    return;
                } 
                Console.WriteLine("The passwords did not match. Please try again.\n");
                VerifyPassword(userName);
                return;
            } else {
                Console.WriteLine("Thank you! Goodbye.");
                return;
            }
        }

        // Starts a command line listening operation
        public void Listen()
        {
            Console.Write(CWD + " >  ");
            string? command = Console.ReadLine();

            if (command == null) {
                Console.WriteLine("Please enter a valid command.");
                Listen();
            }

            string[] arguments = ((string)command).Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

            if (arguments.Length < 1) {
                Console.WriteLine("Please enter a valid command.");
                Listen();
            }

            try {
                string movingNode = "";
                string targetDirectory = "";
                string nodeName = "";

                switch (arguments[0])
                {
                    case "list":
                        System.ReadNode(CWD, _userId);
                        break;

                    case "mv":
                        if (arguments.Length != 3) {
                            throw new ArgumentException("Please enter the proper amount of arguments.");
                        }

                        movingNode = System.ConformDirectory(arguments[1], CWD);
                        targetDirectory = System.ConformDirectory(arguments[2], CWD);

                        System.MoveNode(movingNode, targetDirectory, _userId);
                        break;

                    case "cd":
                        if (arguments.Length != 2) {
                            throw new ArgumentException("Please enter the proper amount of arguments.");
                        }

                        targetDirectory = System.ConformDirectory(arguments[1], CWD);

                        // This will throw an error if the directory wasn't found
                        System.ChangeDirectory(targetDirectory, _userId);
                        CWD = targetDirectory;
                        break;

                    case "create":
                        if (!(arguments.Length > 1 && arguments.Length < 4)) {
                            throw new ArgumentException("Please enter the proper amount of arguments.");
                        }

                        nodeName = arguments[1];
                        targetDirectory = CWD;

                        if (arguments.Length == 3) targetDirectory = System.ConformDirectory(arguments[2], CWD);

                        // This will throw an error if the directory wasn't found
                        System.CreateNode(nodeName, targetDirectory, _userId);
                        break;

                    case "delete":
                        if (arguments.Length != 2) {
                            throw new ArgumentException("Please enter the proper amount of arguments.");
                        }

                        nodeName = System.ConformDirectory(arguments[1], CWD);

                        // This will throw an error if the directory wasn't found
                        System.DeleteNode(nodeName, _userId);
                        break;

                    case "perm":
                        // TODO: allow current user to update for other user
                        if (arguments.Length != 3) {
                            throw new ArgumentException("Please enter the proper amount of arguments.");
                        }

                        string permissions = arguments[1];
                        nodeName = System.ConformDirectory(arguments[2], CWD);

                        // This will throw an error if the directory wasn't found
                        System.UpdateNode(permissions, nodeName, _userId);
                        break;

                    case "exit":
                        Console.WriteLine("Thank you! Goodbye.");
                        return;

                    case "login":
                        // TODO: add a way to pass in default name and password check
                        Console.WriteLine("Please Enter your login information");
                        Login();
                        return;

                    default:
                        Console.WriteLine("Please enter a valid command.");
                        break;
                }
            } catch(Exception error) {
                Console.WriteLine(error.Message);
            }


            Listen();
        }

    }

    protected abstract class DiskAdapter
    {
        public DiskAdapter() {}

        public abstract User[] GetUsers();
        public abstract Group[] GetGroups();
        public abstract DiskNodeData GetDiskNode(int[] locationAddress);
        public abstract int[] AddData(string nodeName, int[] parentAddress, int[] groups);
        public abstract DiskNodeData[] DeleteData(int[] locationAddress);
        public abstract (int[], DiskNodeData[]) MoveData(int[] targetNodeAddress, int[] destinationAddress);
        public abstract void UpdateData(int[] locationAddress, string permissions, string userId);
    }

    protected sealed class FakeDiskAdapter : DiskAdapter
    {
        private Data _debugData;

        public FakeDiskAdapter() : base()
        {
            // When we instantiate this class, we are going to parse the data and
            // assign our properties here. I do not know a lot about how a drive boots up,
            // so this is all pseudo code.
            // Parse the file for fake data
            string jsonString = System.IO.File.ReadAllText("fake_dat.json");
            _debugData = JsonSerializer.Deserialize<Data>(jsonString);

            if (_debugData == null) {
                throw new Exception("Problems parsing the JSON for fake data testing. Please adjust.");
            }
        }

        public override User[] GetUsers() {
            User[] users = new User[_debugData.Users.Length];

            // Assign Fake Users
            for (int ii = 0; ii < _debugData.Users.Length; ii++) {
                FakeUser buildData = _debugData.Users[ii];
                int[] newUserGroups = buildData.Groups;

                // Create the new users
                User newUser = new User(buildData._id, buildData.Name, newUserGroups, buildData.Password);
                users[ii] = newUser;
            }

            return users;
        }

        public override Group[] GetGroups() {
            Group[] groups = new Group[_debugData.Groups.Length];

            // Assign Fake Groups
            for (int ii = 0; ii < _debugData.Groups.Length; ii++) {
                FakeGroup buildData = _debugData.Groups[ii];
                Group newGroup = new Group(buildData._id, buildData.Name, buildData.Weight, buildData.Permissions);
                groups[ii] = newGroup;
            }

            return groups;
        }

        public override int[] AddData(string nodeName, int[] parentAddress, int[] groups) {
            DiskNodeData ParentDirectory = GetDiskNode(parentAddress);
            DiskNodeData newDiskNode;

            if (IsFile(nodeName)) {
                string[] fileParts = FileParts(nodeName);
                newDiskNode  = new DiskNodeData{
                    name = fileParts[0],
                    groups = groups,
                    address = new int[2] {1,0},
                    extension = fileParts[1],
                };
            } else {
                newDiskNode  = new DiskNodeData{
                    name = nodeName,
                    groups = groups,
                    nodes = Array.Empty<DiskNodeData>(),
                };
            }

            return AppendNodeToParent(newDiskNode, ParentDirectory, parentAddress);

        }
        public override DiskNodeData[] DeleteData(int[] locationAddress)
        {
            // Get the Parent by counting 1 less index point;
            int lastArrayIndex = locationAddress.Length - 1;
            int[] parentAddress = GetParentAddress(locationAddress);
            int lastLocationIndex = locationAddress[lastArrayIndex];
            DiskNodeData ParentLocation = GetDiskNode(parentAddress);

            if (ParentLocation.nodes != null && lastLocationIndex < ParentLocation.nodes.Length) {
                DiskNodeData[] newNodes = new DiskNodeData[ParentLocation.nodes.Length - 1];
                Array.Copy(ParentLocation.nodes, 0, newNodes, 0, ParentLocation.nodes.Length - 1);

                // Remove that node address and replace
                for(int ii = newNodes.Length - 1; ii >= lastLocationIndex; ii--) {
                    newNodes[ii] = ParentLocation.nodes[ii + 1];
                }

                // Reassign nodes
                ParentLocation.nodes = newNodes;
                SaveFile();
                // Return nodes to assign new locations
                return newNodes;

            } else {
                throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
            }
        }
        public override (int[], DiskNodeData[]) MoveData(int[] targetNodeAddress, int[] destinationAddress)
        {
            DiskNodeData TargetNode = GetDiskNode(targetNodeAddress);
            DiskNodeData DestinationLocation = GetDiskNode(destinationAddress);

            if (DestinationLocation.extension != null) {
                throw new ArgumentException("The site destination was a file. Please correct.");
            }

            // First, delete the target node
            int[] newAddress = AppendNodeToParent(TargetNode, DestinationLocation, destinationAddress);
            DiskNodeData[] newNodes = DeleteData(targetNodeAddress);

            SaveFile();
            return (newAddress, newNodes);
        }

        public override void UpdateData(int[] locationAddress, string permissions, string userId)
        {
            DiskNodeData TargetNode = GetDiskNode(locationAddress);

            if (TargetNode.user_permissions == null) {
                TargetNode.user_permissions = new Dictionary<string, string>();
            }
            
            TargetNode.user_permissions.Remove(userId);
            TargetNode.user_permissions.Add(userId, permissions);

            SaveFile();
        }

        public override DiskNodeData GetDiskNode(int[] locationAddress)
        {
            // Get the current Fake Data location of current Node
            DiskNodeData requestedDisk = _debugData.Root;

            for (int ii = 0; ii < locationAddress.Length; ii++) {
                int index = locationAddress[ii];

                if (requestedDisk.nodes == null || index < 0 || index >= requestedDisk.nodes.Length) {
                    throw new ArgumentException("There was an error with the test data. Please fix.");
                }

                requestedDisk = requestedDisk.nodes[index];
            }

            return requestedDisk;
        }

        // TODO: come up with a real check for this
        private bool IsFile(string nodeName)
        {
            return nodeName.Contains(".");
        }

        // TODO: a clean way to do this
        private string[] FileParts(string nodeName)
        {
            return new string[2] { nodeName.Substring(0, nodeName.LastIndexOf(".")), nodeName.Substring(nodeName.LastIndexOf(".") + 1)};
        }

        private int[] AppendNodeToParent(DiskNodeData newDiskNode, DiskNodeData ParentDirectory, int[] parentAddress)
        {
            if (ParentDirectory.nodes == null) {
                throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
            }

            // Create our new Node Array
            int lastNodeIndex = ParentDirectory.nodes.Length;
            DiskNodeData[] newNodes = new DiskNodeData[lastNodeIndex + 1];
            Array.Copy(ParentDirectory.nodes, 0, newNodes, 0, lastNodeIndex);

            // Get the Address location of our new node
            int lastAddressIndex = parentAddress.Length;
            int[] newChildAddress = new int[lastAddressIndex + 1];
            Array.Copy(parentAddress, 0, newChildAddress, 0, lastAddressIndex);
            newChildAddress[lastAddressIndex] = lastNodeIndex;

            // Assign the new child
            newNodes[lastNodeIndex] = newDiskNode;
            ParentDirectory.nodes = newNodes;

            SaveFile();

            return newChildAddress;
        }


        private int[] GetParentAddress(int[] localAddress)
        {
            // Get the Parent by counting 1 less index point;
            int lastArrayIndex = localAddress.Length - 1;
            int[] parentAddress = new int[lastArrayIndex];
            Array.Copy( localAddress, 0, parentAddress, 0, lastArrayIndex);
            return parentAddress;
        }

        private void SaveFile() {
            string newValueString = JsonSerializer.Serialize(_debugData);
            System.IO.File.WriteAllText("fake_dat.json", newValueString);
        }
    }

    protected class FileSystem
    {
        // Unique instances across all instantiations
        protected static Directory Root;
        protected static Group[] Groups;
        protected static User[] Users;
        // TODO: may delete, these may be helpful
        protected static User? RootUser;
        protected static Group? RootGroup;
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
            RootGroup = Array.Find(Groups, group => group.Name.Equals("root"));

            // Safety check
            if (RootUser == null) throw new Exception("Root user does not exist");
            if (RootUser == null) throw new Exception("Root group does not exist");

            // Mount Root
            int[] rootDiskAddress = Array.Empty<int>();
            Root = (Directory)MountNode(rootDiskAddress);

            // Add Children;
            MountDiskDirectoryChildren(Root);
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

        private Node MountNode(int[] locationAddress)
        {
            DiskNodeData buildData = _disk.GetDiskNode(locationAddress);
            Node newNode;

            if (buildData.extension == null) {
                newNode = new Directory(buildData.name, buildData.groups, locationAddress);
            } else {
                newNode = new File(buildData.name, buildData.groups, locationAddress, locationAddress, buildData.extension);
            }

            // If custom permissions were set, update that node using root
            if (buildData.user_permissions != null) {
                foreach (KeyValuePair<string, string> entry in buildData.user_permissions) {
                    try
                    {
                        int userId = Int32.Parse(entry.Key);
                        newNode.UpdatePermissions(userId, entry.Value);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("User Permissions were not formatted properly on the disk.");
                    }
                }
            }

            return newNode;
        }

        private void MountDiskDirectoryChildren(Directory workingDirectory)
        {
            if (!workingDirectory.WasMounted) {
                // Get the current Fake Data location of current Node
                DiskNodeData DataLocation = _disk.GetDiskNode(workingDirectory.LocalAddress);

                // If we found a directory in fake data, and that directory has nodes...
                if (DataLocation.nodes != null) {
                    // create a base to use for new fake data location
                    int lastLocIndex = workingDirectory.LocalAddress.Length;
                    int arrLocLength = lastLocIndex+1;

                    // Loop through children, map addresses and Mount Node
                    for (int ii = 0; ii < DataLocation.nodes.Length; ii++) {
                        // Set up the new disk location address
                        int[] baseLocation = new int[arrLocLength];
                        Array.Copy( workingDirectory.LocalAddress, 0, baseLocation, 0, lastLocIndex);
                        baseLocation[lastLocIndex] = ii;

                        // Assign custom permissions
                        Node node = MountNode(baseLocation);

                        // Add the node as a child
                        workingDirectory.AddNode(node);
                    }
                }

                workingDirectory.WasMounted = true;
            }
        }

        public int FindUser(string userName)
        {
            User? user = Array.Find(Users, user => user.Name.Equals(userName));

            if (user == null) return -1;
            if (user.HasPassword()) return -2;
            
            return user._id;
        }

        public int VerifyUser(string userName, string password)
        {
            User? user = Array.Find(Users, user => user.Name.Equals(userName));

            return user.PasswordMatched(password) ? user._id : -1;
        }

        private User GetUser(int userId)
        {
            User? user = Array.Find(Users, user => user._id == userId);

            if (user == null) throw new ArgumentException("No user was found with that id");
            
            return user;
        }

        private static Group GetGroup(int groupId)
        {
            Group? group = Array.Find(Groups, group => group._id == groupId);

            if (group == null) throw new ArgumentException("No group was found with that id");
            
            return group;
        }

        // Called before any CRUDX operations
        public string ConformDirectory(string input, string CWD)
        {
            if (input.Equals("")) return CWD;

            if (input[0] == '/') return input;

            return CWD + (CWD.Equals("/") ? "" : "/") + input;

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
                if (workingNode.IsFile()) {
                    // throw error...
                    throw new ArgumentException("A file cannot contain a directory.");
                }

                string key = pathKeys[ii];

                if (!((Directory)workingNode).Nodes.ContainsKey(key)) {
                    // throw an error...
                    throw new ArgumentException("Please correct your file path.");
                }

                parent = (Directory)workingNode;
                workingNode = ((Directory)workingNode).Nodes[key];

                if (!workingNode.CanRead(currentUser)) {
                    // throw error...
                    throw new ArgumentException("You are not permitted to read that directory");
                }

                // If the directory hasn't been mounted yet
                if (!workingNode.IsFile() && !((Directory)workingNode).WasMounted) {
                    MountDiskDirectoryChildren((Directory)workingNode);
                }
            }

            return (workingNode, parent);
        }
                
        public string ChangeDirectory(string directoryPath, int currentUserId)
        {
            User currentUser = GetUser(currentUserId);
            (Node directory, _) = GetNode(directoryPath, currentUser);

            if (directory.IsFile()) {
                // throw error...
                throw new ArgumentException("Cannot change directory to a file destination.");
            }

            Console.WriteLine("Directory was changed.");

            // If a valid node was found, we will return the directoryPath back as valid
            return directoryPath;
        }

        // CRUD Directories
        // Create
        public void CreateNode(string nodeName, string directoryPath, int currentUserId)
        {
            User currentUser = GetUser(currentUserId);
            (Node CWD, _) = GetNode(directoryPath, currentUser);

            if (nodeName.Contains("/")) {
                // throw error...
                throw new ArgumentException("Cannot create directories recursively.");
            }

            if (!CWD.CanCreate(currentUser)) {
                // throw error...
                throw new ArgumentException("Cannot write in this directory.");
            }

            Node? node = null;

            // TODO: If catch block goes off, revert the node Value
            try {
                int[] locationAddress = _disk.AddData(nodeName, CWD.LocalAddress, currentUser.Groups);
                node = MountNode(locationAddress);
                ((Directory)CWD).AddNode(node);
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

            if (directory.IsFile())
            {
                // throw error...
                throw new ArgumentException("Cannot read a file with the FileSystem Program.");
            }

            if (directory.CanRead(currentUser)) {
                ((Directory)directory).ListChildren(currentUser);
            } else {
                Console.WriteLine("Current User does not have access to this folder.");
            }
        }

        // Update
        public void MoveNode(string nodeName, string targetDirectory, int currentUserId)
        {
            User currentUser = GetUser(currentUserId);
            (Node fileOrDirectory, Directory? parent) = GetNode(nodeName, currentUser);
            (Node destination, _) = GetNode(targetDirectory, currentUser);

            if (destination.IsFile())
            {
                // throw error...
                throw new ArgumentException("Cannot move a directory or file to a file. Check your destination name.");
            }

            if (!destination.CanCreate(currentUser))
            {
                // throw error...
                throw new ArgumentException("Cannot write to destination directory.");
            }

            if (!fileOrDirectory.CanDelete(currentUser))
            {
                // throw error...
                throw new ArgumentException("Cannot update the destination of the file or directory.");
            }

            // TODO: If catch block goes off, revert the node Value
            try {
                if (parent != null) {
                    // Get our node's new location and the old location's new nodes
                    (int[] locationAddress, DiskNodeData[] newNodes) = _disk.MoveData(fileOrDirectory.LocalAddress, destination.LocalAddress);

                    // Delete the node from the old directory
                    parent.DeleteNode(fileOrDirectory, newNodes);

                    // Assign the address to the new location, then add it to the destinatino node
                    fileOrDirectory.LocalAddress = locationAddress;
                    ((Directory)destination).AddNode(fileOrDirectory);

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
            User user = currentUser;

            if (targetUserId != null) {
                user = GetUser((int)targetUserId);
            }

            // TODO, account for files and directories having different executable permissions
            if(permissions.Length < 4 || permissions.Length > 5)
            {
                // throw error...
                throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
            }

            if (user.IsRoot() && !currentUser.IsRoot()) {
                // throw error...
                throw new ArgumentException("Insufficient permissions to update permissions for this user.");
            }
            
            (Node node, _) = GetNode(nodeName, currentUser);

            if(!node.CanUpdate(currentUser)) {
            // throw error...
                throw new ArgumentException("Insufficient permissions to update permission.");
            }

            _disk.UpdateData(node.LocalAddress, permissions, user._id.ToString());
            node.UpdatePermissions(user._id, permissions);
            Console.WriteLine("Successfully updated permissions.");
        }

        // Delete
        public void DeleteNode(string nodeName, int currentUserId)
        {
            User currentUser = GetUser(currentUserId);
            (Node fileOrDirectory, Directory? parent) = GetNode(nodeName, currentUser);

            if (parent == null)
            {
                // They are trying to delete the root directory
                throw new ArgumentException("Cannot delete root directory");
            }

            if (String.Compare(fileOrDirectory.DataType(), "directory") == 0 && ((Directory)fileOrDirectory).Nodes.Count > 0)
            {
                // throw error...
                throw new ArgumentException("Cannot delete a directory with files in it.");
            }

            if (!fileOrDirectory.CanDelete(currentUser) || !parent.CanDelete(currentUser))
            {
                // throw error...
                throw new ArgumentException("Insufficient Permissions to delete file or directory.");
            }

            Console.WriteLine(fileOrDirectory.GetName());

            // TODO: error check
            DiskNodeData[] newNodes = _disk.DeleteData(fileOrDirectory.LocalAddress);
            parent.DeleteNode(fileOrDirectory, newNodes);

            Console.WriteLine("File or Directory was successfully deleted.");
        }

        public static Group? SharedGroup(int[] userGroups, int[] nodeGroups) {
            Group? highestRankedGroup = null;
            // Hashtable to verify that group was found without endless looping;
            Dictionary<int, bool> passed = new Dictionary<int, bool>();

            for (int ii = 0; ii < userGroups.Length; ii++) {
                int userGroupId = userGroups[ii];
                passed.Add(userGroupId, true);
            }

            for (int ii = 0; ii < nodeGroups.Length; ii++) {
                int nodeGroupId = nodeGroups[ii];

                if (passed.ContainsKey(nodeGroupId)) {
                    Group sharedGroup = GetGroup(nodeGroupId);
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

        public static bool IsRootGroupId(int groupId)
        {
            return groupId == RootGroup._id;
        }

    }

    protected class User
    {
        public int _id { get; }
        public string Name { get; }
        public int[] Groups { get; }
        private string? Password;

        public User(int id, string name, int[] groups, string? password = null)
        {
            _id = id;
            Name = name;
            Groups = groups;
            Password = password;
        }

        public bool IsRoot()
        {
            for (int ii = 0; ii < Groups.Length; ii++) {
                int groupId = Groups[ii];

                if (FileSystem.IsRootGroupId(groupId)) return true;
            }

            return false;
        }

        public bool HasPassword()
        {
            return Password != null;
        }

        public bool PasswordMatched(string input)
        {
            return Password.Equals(input);
        }
    }

    protected class Group
    {
        public int _id { get; }
        public string Name { get; }
        public int Weight { get; }
        public CRUDX Permissions { get; }

        public Group (int id, string name, int weight, string permissions)
        {
            _id = id;
            Name = name;
            Weight = weight;
            Permissions = new CRUDX(permissions);
        }
    }

    protected class CRUDX
    {
        public bool[] Values { get; }

        public CRUDX (string permissions)
        {
            bool[] standardizedPermissions = new bool[5];

            for(int ii = 0; ii < permissions.Length; ii++)
            {
                if (permissions[ii] == '1') {
                    standardizedPermissions[ii] = true;
                } else if (permissions[ii] != '0') {
                    // throw error...
                    throw new ArgumentException("Invalid character shorthand for permissions.");
                }
            }

            Values = standardizedPermissions;
        }

        public bool HasPermission(int index) {
            // Throw error if the permission index is beyond the array size
            if (index < 0 || index > 4) {
                throw new ArgumentException("This permission does not exist.");
            }

            return Values[index];
        }

        public string GetPermissionsString()
        {
            string perms = "";

            for (int ii = 0; ii < Values.Length; ii++) {
                perms += Values[ii] ? "1" : "0";
            }

            return perms;
        }

        // TODO: implement the ability to update just one value
        // public void UpdateIndex(int index, int value) {}
    }

    protected abstract class Node
    {
        protected string Name;
        protected Dictionary<int, CRUDX> Permissions = new Dictionary<int, CRUDX>();
        public int[] Groups { get; }
        // TODO: perhaps delete... This is the fake address on the disk
        public int[] LocalAddress;

        public Node(string name, int[] groups, int[] localAddress)
        {
            Name = name;
            Groups = groups;
            LocalAddress = localAddress;
        }

        public abstract string DataType();

        public abstract string GetKey();

        public abstract string ConformPermissions(string permissions);

        public bool IsFile()
        {
            if (String.Compare(DataType(), "file") == 0) return true;
            return false;
        }

        public string GetName()
        {
            return Name;
        }

        public string GetUserPermissionString(User currentUser)
        {
            // If the Node's Permissions property has a custom set for the user
            if (Permissions.ContainsKey(currentUser._id)) {
                return Permissions[currentUser._id].GetPermissionsString();
            }

            // Root will override any group except for custom permissions set to the user
            if (currentUser.IsRoot()) {
                return ConformPermissions("11111");
            }

            // Otherwise check if the user shares a group with the Node
            Group? sharedGroup = FileSystem.SharedGroup(currentUser.Groups, Groups); 
            if (sharedGroup != null) {
                return sharedGroup.Permissions.GetPermissionsString();
            }

            // Otherwise no way
            return "00000";
        }

        public void UpdatePermissions(int userId, string permissions)
        {
            if (permissions.Length != 5) {
                throw new ArgumentException("Invalid array of permissions passed.");
            }

            // This makes sure executable permissions are turned off for non-files and on for files
            ConformPermissions(permissions);

            Permissions.Remove(userId);
            Permissions.Add(userId, new CRUDX(permissions));
        }

        private bool HasPermission(int index, User currentUser)
        {
            // If the Node is not a file but we are checking the eXecutable permission, always return false
            if (!IsFile() && index == 4) {
                return false;
            }

            // If the Node's Permissions property has a custom set for the user
            if (Permissions.ContainsKey(currentUser._id)) {
                return Permissions[currentUser._id].HasPermission(index);
            }

            // Root will override any group except for custom permissions set to the user
            if (currentUser.IsRoot()) {
                return true;
            }

            // Otherwise check if the user shares a group with the Node
            Group? sharedGroup = FileSystem.SharedGroup(currentUser.Groups, Groups); 
            if (sharedGroup != null) {
                return sharedGroup.Permissions.HasPermission(index);
            }

            // Otherwise no way
            return false;
        }

        // CRUDX
        public bool CanCreate(User currentUser)
        {
            return HasPermission(0, currentUser);
        }

        public bool CanRead(User currentUser)
        {
            return HasPermission(1, currentUser);
        }

        public bool CanUpdate(User currentUser)
        {
            return HasPermission(2, currentUser);
        }

        public bool CanDelete(User currentUser)
        {
            return HasPermission(3, currentUser);
        }

        public bool CanExecute(User currentUser)
        {
            return HasPermission(4, currentUser);
        }
    }

    protected class File : Node
    {
        public string Extension;
        protected int[] Address;

        public File(string name, int[] groups, int[] localAddress, int[] address, string extension) :
        base(name, groups, localAddress)
        {
            Address = address;
            Extension = extension;
        }

        public override string DataType()
        {
            return "file";
        } 

        public override string GetKey()
        {
            return Name + "." + Extension;
        }

        public override string ConformPermissions(string permissions)
        {
            return permissions.Substring(0, permissions.Length - 1) + "1";
        }
    }
    

    protected class Directory : Node
    {
        public bool WasMounted { get; set; } = false;
        public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
        protected string[] OrderedDirectories = Array.Empty<string>();
        protected string[] OrderedFiles  = Array.Empty<string>();

        public Directory(string name, int[] groups, int[] localAddress) :
        base(name, groups, localAddress)
        { }

        public override string DataType()
        {
            return "directory";
        }

        public override string GetKey()
        {
            return Name;
        } 

        public override string ConformPermissions(string permissions)
        {
            return permissions.Substring(0, permissions.Length - 1) + "0";
        }

        // Binary search to get index of insertion or deletion point
        public int GetIndex(string[] array, string key)
        {
            int low = 0;
            int high = array.Length - 1;
            int index = 0;

            while (low <= high) {
                index = low + ((high - low) / 2);

                // if the current index has a higher alphabetic position
                if (String.Compare(array[index], key) == 1) {
                    high = index - 1;
                // if the current index has a lower alphabetic position
                } else if (String.Compare(array[index], key) == -1) {
                    low = index + 1;
                // if they are equal
                } else {
                    return index;
                }
            }

            // If array was already empty, we insert at the first position
            return index;
        }

        public void AddNode(Node node)
        {
            string key = node.GetKey();

            if (Nodes.ContainsKey(key)) {
                // throw error...
                throw new ArgumentException("The file or directory was not found in the directory {0}.", key);
            }

            // Add to our children
            Nodes.Add(key, node);

            // Add key to our min heap using a binary search and insertion
            string[] orderedArray;
            if (node.IsFile()) {
                orderedArray = OrderedFiles;
            } else {
                orderedArray = OrderedDirectories;
            }

            string[] newArray = new string[orderedArray.Length + 1];
            string nextKey = key;

            // Assign original values to new array
            // A nice shortcut to resize and add elements to our array
            Array.Copy( orderedArray, 0, newArray, 0, orderedArray.Length);
            int index = GetIndex(orderedArray, key);

            // Add new value and reassign next elements
            for(int ii = index; ii < newArray.Length; ii++) {
                string currentKey = newArray[ii];
                newArray[ii] = nextKey;
                nextKey = currentKey;
            }

            // Reassign property
            if (node.IsFile()) {
                OrderedFiles = newArray;
            } else {
                OrderedDirectories = newArray;
            }
        }

        public void DeleteNode(Node node, DiskNodeData[] newNodesByOrder)
        {
            // Update object
            string key = node.GetKey();

            if (!Nodes.ContainsKey(key)) {
                // throw error...
                throw new ArgumentException("The file or directory was not found in the directory {0}.", key);
            }

            Nodes.Remove(key);
            UpdateNodeAddresses(newNodesByOrder);

            // Remove key from our min heap using a binary search and insertion
            string[] orderedArray;
            if (node.IsFile()) {
                orderedArray = OrderedFiles;
            } else {
                orderedArray = OrderedDirectories;
            }
            
            int index = GetIndex(orderedArray, key);
            string[] newArray = new string[orderedArray.Length - 1];

            // Assign original values to new array
            // A nice shortcut to resize and copy our array
            Array.Copy( orderedArray, 0, newArray, 0, orderedArray.Length - 1);

            // From the end, reassign values that were deleted
            // Not the most efficient, would refactor if I had time
            for(int ii = newArray.Length - 1; ii >= index; ii--) {
                newArray[ii] = orderedArray[ii + 1];
            }

            // Reassign property
            if (node.IsFile()) {
                OrderedFiles = newArray;
            } else {
                OrderedDirectories = newArray;
            }

        }

        public void ListChildren(User currentUser)
        {
            // Write a Table Header
            Console.WriteLine("\n| {0,-20} | {1,20} |", "Permissions (CRUDX)", "Items");
            Console.WriteLine("-----------------------------------------------");

            // List Directories...
            WriteChildrenContentsToConsole(OrderedDirectories, currentUser);
            // List Files...
            WriteChildrenContentsToConsole(OrderedFiles, currentUser);
            
            // Give us some reading space
            Console.WriteLine("");
        }

        private void WriteChildrenContentsToConsole(string[] array, User currentUser)
        {
            for(int ii = 0; ii < array.Length; ii++)
            {
                string key = array[ii];
                string perms = Nodes[key].GetUserPermissionString(currentUser);
                Console.WriteLine("| {0,-20} | {1,20} |", perms, key);
            }
        }

        private void UpdateNodeAddresses(DiskNodeData[] newNodesByOrder)
        {
            // Get Last Index of Local Disk Address
            int lastIndex = LocalAddress.Length;

            for (int ii = 0; ii < newNodesByOrder.Length; ii++) {
                DiskNodeData currentDiskNodeData = newNodesByOrder[ii];
                string nodeKey = currentDiskNodeData.GetKey();
                Node currentNode = Nodes[nodeKey];
                currentNode.LocalAddress[lastIndex] = ii;

                Nodes.Remove(nodeKey);
                Nodes.Add(nodeKey, currentNode);
            }    
        }
    }

    // DEBUG: All classes below are for writing fake data to a json file for testing user sessions
    protected class Data
    {
        public DiskNodeData Root { get; set; }
        public FakeUser[] Users { get; set; }
        public FakeGroup[] Groups { get; set; }
    }

    public class DiskNodeData
    {
        public string name { get; set; }
        public int[] groups { get; set; }
        public string? extension { get; set; }
        public int[]? address { get; set; }
        public Dictionary<string, string>? user_permissions { get; set; }
        public DiskNodeData[]? nodes { get; set; }

        public string GetKey()
        {
            string keyName = this.name;
            if (this.extension != null) keyName += "." + this.extension;
            return keyName;
        }
    }

    public class FakeUser
    {
        public int _id { get; set; }
        public string Name { get; set; }
        public string? Password { get; set; }
        public int[] Groups { get; set; }
    }

    public class FakeGroup
    {
        public int _id { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
        public int Weight { get; set; }
    }
}