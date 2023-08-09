/**
Assignment 2 - Design Principles: Design Patterns

Code has been refactored to run in a command line interface with dotnet. My version is a little outdated and is using dotnet 7.

Per previous feedback, a new class "Group" has been added. Currently "User" and "Node" classes have "Groups", and this controls the default permissions for a given user and a given file or directory. Currently, the "Group" just acts as a catch all, and more complex permission mapping will be added later when I can incorporate SOLID.

Much of my previous code already incorporated Encapsulation, Inheritance and Polymorphism. I've updated the code to be in a proper working state an added an extra layer of encapsulation by adding the "FileSystemCommand" class.

"FileSystem" class is now nested in "FileSystemCommand" and is a Singleton. All "Directory" and "File" objects will now be protected from duplication thanks to that encapsulation.

Properties "CWD" and "CurrentUser" have been moved to "FileSystemCommand" class since these can change depending on who's instantiated the app. However, since "User" and "Directory" classes are only accessible in the "FileSystem" class, the properties have been changed to "string CWD" and "int _userId". Methods in the "FileSystem" class and nested classes have been updated to account for this.

Class "Node" methods "DataType" and "GetKey" are now abstract and fully utilize polymorphism with C# standards. They must be explicitly defined in extended classes. There was some general clean up. Node permission check methods have been updated to CRUDX though not all checks have been incorporated yet. Node "HasPermission" method has been updated to compare against both custom permissions and shared groups with current user.

There wasn't really a need to incorporate any more design patterns other than Singleton. Arguabley, the "DataType()" method in the "Node" class is similar to "Strategy" but as is, it just serves as a convenient "if" check of what the "Node" Type is. "UpdatePermissions" in the derived classes "File" and "Directory" could be updated to use the "Strategy" Design pattern, but there are so many similarities between how they udpate that doing so would go against DRY. Perhaps once real data starts being used, there will be more need to flesh out these designs.
*/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FileSystemApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Application...");

        FileSystemCommand fileSystem = new FileSystemCommand();

        Console.WriteLine("Application Startup Successful!\n");

        Console.WriteLine("You may exit the application at any time by typing \"exit\".");
        fileSystem.Listen();
    }

    protected class FileSystemCommand
    {
        // Flexible assignment from instance to instance, but uses the same unique instantiations
        protected static string CWD = "/";
        protected static int _userId;
        protected FileSystem System = FileSystem.Instance;
        

        // TODO: create a way to read what logged in user is starting the application on boot
        public FileSystemCommand(int userId = 0)
        {
            _userId = userId;
        }

        private void ChangeUser(int userId)
        {
            // TODO: protect changing root levels
            // This check will throw an error if the user doesn't exist
            System.GetUser(userId);
            _userId = userId;
        }

        // Starts a command line listening operation
        public void Listen()
        {
            Console.Write(CWD + " >");
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

    protected class FileSystem
    {
        // Unique instances across all instantiations
        protected static Directory Root;
        protected static Group[] Groups;
        protected static User[] Users;
        private static FileSystem _instance;    

        // The Mounting
        // TODO: only mount root and it's children on instantiation, we don't need to recursively mount everything

        // TODO: convert Node methods into Strategy methods, see C# documentation for abstract methods

        private FileSystem()
        {
            // When we instantiate this class, we are going to parse the data and
            // assign our properties here. I do not know a lot about how a drive boots up,
            // so this is all pseudo code.
            // TODO: create an id incrementer

            // Unique Instance
            Group RootGroup = new Group(1, "root");
            Groups = new Group[1] { RootGroup };

            // TODO: create a real root user
            Users = new User[1] { new User(2, "root", Groups) };

            // Root and all Nodes should be a single instance
            Root = new Directory("root", Groups);

            // Add Children;
            AddDirectoryChildren(Root);
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

        // TODO: convert to a real method
        private void AddDirectoryChildren(Directory workingDirectory)
        {
            // fake data to parse
            // This will serve as a way to "parse" the rest of our disk data into readable files and directories
            Queue<int> datas = DebugCreateFakeDataQueue();

            // Create a Queue to assist in Directory and File instantiation
            Queue<Directory> directories = new Queue<Directory>();
            directories.Enqueue(workingDirectory);

            // While we still have directories to work through...
            while(directories.Count > 0 && datas.Count > 0) {
                // Get our Node...
                Directory current = directories.Dequeue();

                // parse data...
                // I'm doing this in a fake way since I don't now how to parse Date from an OS yet
                for(int ii = 0; ii < 4; ii++) {
                    int data = datas.Dequeue();
                    Node? child = null;

                    // fake data name
                    string name = DebugRandomStringName();

                    // If our data is a directory...
                    if (data == 0) {
                        Directory newDirectory = new Directory(name, Groups);
                        // Add this directory to parse now;
                        directories.Enqueue(newDirectory);
                        child = newDirectory;
                    }

                    // if our data is a file...
                    if(data == 1) {
                        child = new File(name, Groups, new int[2] { 0, 1 }, "rtf");
                    }

                    // Add the data as a child of our current directory;
                    if (child != null) {
                        current.AddNode(child);
                    }
                    
                }
            }
        }

        // TODO: Get actual user by id
        public User GetUser(int userId)
        {
            // TODO: error handle
            return Users[0];
        }

        // TODO: remove all Debugging helpers for fake data below
        // fake data. Random string borrowed from stack overflow
        // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        // Answer by dtb
        private string DebugRandomStringName()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstubwxyz";
            return new string(Enumerable.Repeat(chars, 7)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Queue for fake directories and files. Every directory will have 4 node children
        private Queue<int> DebugCreateFakeDataQueue()
        {
            Queue<int> datas = new Queue<int>();
            // Root directory, 2 directories, 2 files
            datas.Enqueue(0);
            datas.Enqueue(1);
            datas.Enqueue(0);
            datas.Enqueue(1);
            // Root Child 1, 2 directories, 2 files
            datas.Enqueue(0);
            datas.Enqueue(1);
            datas.Enqueue(0);
            datas.Enqueue(1);
            // Root Child 2, 1 directory, 3 files
            datas.Enqueue(1);
            datas.Enqueue(1);
            datas.Enqueue(0);
            datas.Enqueue(1);
            // Root Child 1 child 1, 3 directories, 1 file
            datas.Enqueue(1);
            datas.Enqueue(0);
            datas.Enqueue(0);
            datas.Enqueue(0);
            // Root Child 1 child 2, 3 directories, 1 file
            datas.Enqueue(1);
            datas.Enqueue(0);
            datas.Enqueue(0);
            datas.Enqueue(0);
            // Root Child 2 child 1, 4 file
            datas.Enqueue(1);
            datas.Enqueue(1);
            datas.Enqueue(1);
            datas.Enqueue(1);

            return datas;
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
            // TODO: Change the hash checks in our methods, if the name doesn't exist, then first see if the directory does indeed exist and then make it in our Node
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

            Node node;

            // TODO: real way to check if there's an extension
            if (nodeName.Contains(".")) {
                string[] nodeNameArray = nodeName.Split(".");
                node = new File(nodeNameArray[0], Groups, new int[] {}, nodeNameArray[1]);
            } else {
                node = new Directory(nodeName, Groups);
            }

            ((Directory)CWD).AddNode(node);
            Console.WriteLine("Successfully created item.");
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
                ((Directory)directory).ListChildren();
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

            // May have to cast this before I can add
            ((Directory)destination).AddNode(fileOrDirectory);
            if (parent != null) parent.DeleteNode(fileOrDirectory);

            Console.WriteLine("File or Directory was moved successfully");
        }

        public void UpdateNode(string permissions, string nodeName, int currentUserId, int? targetUserId = null)
        {
            User currentUser = GetUser(currentUserId);
            User user = currentUser;

            if (targetUserId != null) {
                user = GetUser((int)targetUserId);
            }

            // TODO, account for files and directories having different executable permissions
            if(permissions.Length > 4 || permissions.Length > 5)
            {
                // throw error...
                throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
            }

            if (user.isRoot() && !currentUser.isRoot()) {
                // throw error...
                throw new ArgumentException("Insufficient permissions to update permissions for this user.");
            }
            
            (Node node, _) = GetNode(nodeName, currentUser);

            if(!node.CanUpdate(currentUser)) {
            // throw error...
                throw new ArgumentException("Insufficient permissions to update permission.");
            }

            bool[] newPermissions = new bool[5];

            for(int ii = 0; ii < permissions.Length; ii++)
            {
                if (permissions[ii] == '1') {
                    newPermissions[ii] = true;
                } else if (permissions[ii] != '0') {
                    // throw error...
                    throw new ArgumentException("Invalid character shorthand for permissions.");
                } 
            }

            node.UpdatePermissions(user._id, newPermissions);
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

            parent.DeleteNode(fileOrDirectory);
            Console.WriteLine("Successfully deleted item.");
        }

        // This exists outside of the filesystem and will be handled by the OS itself
        public class User
        {
            // _id value is same as index in user array
            public int _id { get; }
            public string Name { get; }
            public Group[] Groups { get; }

            public User(int id, string name, Group[] groups)
            {
                _id = id;
                Name = name;
                Groups = groups;
            }

            public bool isRoot()
            {
                for (int ii = 0; ii < Groups.Length; ii++) {
                    Group group = Groups[ii];

                    if (String.Compare(group.Name, "root") == 0) 
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool SharesGroup(Group[] nodeGroups) {
                // Hashtable to verify that group was found without endless looping;
                Dictionary<int, bool> found = new Dictionary<int, bool>();

                // Loop through User's groups
                for (int ii = 0; ii < Groups.Length; ii++) {
                    // Add to found hash if it doesn't exist
                    if (found.ContainsKey(Groups[ii]._id)) return true;
                    found.Add(Groups[ii]._id, true);

                    // Loop through Node's groups
                    for (int jj = 0; jj < nodeGroups.Length; jj++) {
                        if (found.ContainsKey(nodeGroups[jj]._id)) return true;
                        found.Add(nodeGroups[jj]._id, true);
                    }
                }

                // Group was not found
                return false;
            }
        }

        // TODO: default groups...
        // TODO: give groups weight/priority
        // TODO: give groups default permissions for a file or a directory
        public class Group
        {
            public int _id { get; }
            public string Name { get; }

            public Group (int id, string name)
            {
                _id = id;
                Name = name;
            }
        }

        public abstract class Node
        {
            protected string Name;
            protected Dictionary<int, bool[]> Permissions = new Dictionary<int, bool[]>();
            public Group[] Groups { get; }

            public Node(string name, Group[] groups)
            {
                Name = name;
                Groups = groups;
            }

            public abstract string DataType();

            public abstract string GetKey();

            public bool IsFile()
            {
                if (String.Compare(DataType(), "file") == 0) return true;
                return false;
            }

            public string GetName()
            {
                return Name;
            }

            // TODO: refactor Permissions with Groups
            public void UpdatePermissions(int _id, bool[] permissions)
            {
                if (permissions.Length != 5) {
                    throw new ArgumentException("Invalid array of permissions passed.");
                }

                // Always make sure executable permission is turned off for non-files and on for files
                if (IsFile()) {
                    permissions[4] = true;
                } else {
                    permissions[4] = false;
                }

                if (Permissions.ContainsKey(_id)) Permissions.Remove(_id);

                Permissions.Add(_id, permissions);
            }

            private bool HasPermission(int index, User currentUser)
            {
                // Throw error if the permission index is beyond the array size
                if (index < 0 || index > 4) {
                    throw new ArgumentException("This permission does not exist.");
                }

                // If the Node is not a file but we are checking the eXecutable permission, always return false
                if (!IsFile() && index == 4) {
                    return false;
                }

                // If the Node's Permissions property has a custom set for the user
                if (Permissions.ContainsKey(currentUser._id)) {
                    return Permissions[currentUser._id][index];
                }

                // TODO: fix
                // Root will override any group except for custom permissions set to the user
                if (currentUser.isRoot()) {
                    return true;
                }

                // TODO: refactor to have default permissions against groups
                // Otherwise check if the user shares a group with the Node
                return currentUser.SharesGroup(Groups);
                
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

            public File(string name, Group[] groups, int[] address, string extension) :
            base(name, groups)
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
        }
        

        protected class Directory : Node
        {
            public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
            protected string[] OrderedDirectories = new string[] {};
            protected string[] OrderedFiles  = new string[] {};

            public Directory(string name, Group[] groups) :
            base(name, groups)
            { }

            public override string DataType()
            {
                return "directory";
            }

            public override string GetKey()
            {
                return Name;
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
                int index = GetIndex(newArray, key);

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

            public void DeleteNode(Node node)
            {
                string key = node.GetKey();

                if (!Nodes.ContainsKey(key)) {
                    // throw error...
                    throw new ArgumentException("The file or directory was not found in the directory {0}.", key);
                }

                Nodes.Remove(key);

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


            // TODO: Actually list other Node properties
            public void ListChildren()
            {
                // List Directories...
                Console.WriteLine("Directories...");
                for(int ii = 0; ii < OrderedDirectories.Length; ii++)
                {
                    string key = OrderedDirectories[ii];
                    Console.WriteLine(key);
                }

                // List Files...
                Console.WriteLine("Files...");
                for(int ii = 0; ii < OrderedFiles.Length; ii++)
                {
                    string key = OrderedFiles[ii];
                    Console.WriteLine(key);
                }
            }
        }
    }
}