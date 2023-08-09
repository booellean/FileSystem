/**
Assignment - File System:

For an in memory file system, we will design two custom classes "FileSystem" and "User" that our OS can instantiate on mount or when an end user is interacting with a CLI or GUI. Within the "FileSystem" class, we will make use of protected classes that cannot be accessed outside of our parent class (encapsulation). The first is an abstract class called "Node" that will serve as our base for all files and directories. The "Node" class contains the following properties:

1. Name - a string that serves as a unique identifier
2. Permissions - a Hashmap (Dictionary) that keeps track of read, write, and delete permissions (similar to Unix's read, write, execute).

From this we create two new classes from our Abstract Class "File" and "Directory". The "File" class adds the following properties:

1. Extension - a string that identifies the type of file
2. Data - the data belonging to the file (note I am using an array of integers since I am not familiar with OS systems).

The "Directory" class adds the following properties:

1. Nodes - a Hashmap (Dictionary) of any child Files or Directories. The hashmap uses the Name or Name + Extension proprties as unique identifiers to track the Node
2. OrderedDirectories - an ordered array of all child directories
3. OrderedFiles - an ordered array of all child files

The "FileSystem" class will control how a user interfaces with these Files and Directories (see methods), and an end user will not be able to call CRUD operations from the "Directory" or "File" objects directly. It has the following properties:

1. Root - The root directory as Directory object, assigned during instantiation.
2. CWD - the Current Working Directory as Directory object. During instantiation it's the same as Root, but changes as a user traverses through directories.
3. CurrentUser - The current user as User Object. The user's _id value is used to check against permissions in the application.

The "Node" class contains methods to check if the current user has the ability to Read, Write, or Update the Node object. The "FileSystem" methods demonstrate how these checks are made before executing certain operations.

The "Directory" holds the boatload for Data Structures and Algorithms. The Directory system is a Tree made up of a Hashmaps (dictionaries). The Root Node (root directory: see line 39) is a "Directory" object, and the branches are made up of Files and Directories. Directories can continue branching but Files cannot. This is expressed in the "Directory" property "Nodes" (see line 194). This all happens during mounting when the "FileSystem" creates all "Directory" and "File" objects (see lines 55 - 111) using a Queue. Whenever a new "Directory" is created, the object is sent to the Queue with it's associated child data (psuedocode here), and will continue branching until there is no more data associated with directories.

However, the "Directory" class also contains the "OrderedDirectories" and "OrderedFiles" properties that are used to easily list out Node information for the user in the "ListChildren" method (see line 330). These are Min Heaps made up of sorted arrays containing Node keys. Whenever a "Node" is added or deleted, a Binary Search is performed in the "GetIndex" method (see line 223) to find the deletion or insertion point, and then a modified Insertion Sort (using C#'s array copying abilities) is performed to update the index position of all following array items. This adds a bit of an extra load on Insertion and Deletion, but minimizes workload of outputting all Node children information to the Console.

*/

using System;
using System.Collections;
using System.Collections.Generic;

public class FileSystem {
    protected Directory Root;
    protected Directory CWD;
    protected User CurrentUser;

    // The Mounting
    public FileSystem(User user, int[] drive)
    {
        // When we instantiate this class, we are going to parse the data and
        // assign our properties here. I do not know a lot about how a drive boots up,
        // so this is all pseudo code.

        // When the Filesystem instantiates, it will do so with a default user.
        CurrentUser = user;

        // parse data... get name and permission info
        // Instantiate our first directory as root...
        string name = "root";
        Dictionary<int, bool[]> permissions = new Dictionary<int, bool[]>();
        permissions.Add(user._id, new bool[3] { true, true, true });
        Root = new Directory(name, permissions);
        CWD = Root;

        // fake data to parse
        // This will serve as a way to "parse" the rest of our disk data into readable files and directories
        Queue datas = new Queue();
        datas.Enqueue(0);
        datas.Enqueue(1);
        datas.Enqueue(0);
        datas.Enqueue(1);
        datas.Enqueue(1);
        datas.Enqueue(0);
        datas.Enqueue(0);
        datas.Enqueue(1);

        // Create a Queue to assist in Directory and File instantiation
        Queue directories = new Queue();
        directories.Enqueue(Root);

        // While we still have directories to work through...
        while(directories.Count > 0 && datas.Count > 0) {
            // Get our Node...
            Directory current = directories.Dequeue();

            // parse data...
            // I'm doing this in a fake way since I don't now how to pase Date from an OS
            for(int ii = 0; ii < 2; ii++) {
                int data = datas.Dequeue();
                Node child = null;

                // fake data. Random string borrowed from stack overflow
                // https://stackoverflow.com/questions/1572733/generate-random-string
                Random r = new Random();
                string name = new String(Enumerable.Range(0, length).Select(n => (Char)(r.Next(32, 127))).ToArray());
                Dictionary<int, int> permissions = new Dictionary<int, int>();
                // Normally we would add these for every user we can
                permissions.Add(user._id, new bool[3] { true, true, true });

                // If our data is a directory...
                if (data == 0) {
                    child = new Directory(name, permissions);
                    // Add this directory to parse now;
                    directories.Enqueue(child);
                }

                // if our data is a file...
                if(data == 1) {
                    child = new File(name, permissions, [0,1], ".rtf");
                }

                // Add the data as a child of our current directory;
                current.AddNode(child);
            }
        }

    }

    protected abstract class Node
    {
        protected string Name;
        protected Dictionary<int, bool[]> Permissions;

        public Node(string name, Dictionary<int, bool[]> permissions)
        {
            Name = name;
            Permissions = permissions;
        }

        public static string DataType()
        {
            return "node";
        }

        public static string GetName()
        {
            return Name;
        }

        public static void UpdatePermissions(int _id, bool[] permissions)
        {
            if (!Permissions.ContainsKey(CurrentUser._id)) {
                return false;
            }

            return Permissions[_id] = permissions;
        }

        private static bool HasPermission(int index)
        {
            if (!Permissions.ContainsKey(CurrentUser._id)) {
                return false;
            }

            return Permissions[CurrentUser._id][index];
        }

        public static bool CanRead()
        {
            return HasPermission(0);
        }

        public static bool CanWrite()
        {
            return HasPermission(1);
        }

        public static bool CanUpdate()
        {
            return HasPermission(2);
        }
    }

    protected class File : Node
    {
        public string Extension;
        protected int[] Data;

        public File(string name, Dictionary<int, int> permissions) :
        this(string name, Dictionary<int, int> permissions, int[] data, string extension)
        { }

        public File(string name, Dictionary<int, int> permissions, int[] data, string extension) :
        base(name, permissions)
        {
            Data = data;
            Extension = extension;
        }

        public override string DataType()
        {
            return "file";
        } 
    }

    protected class Directory : Node
    {
        public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
        protected string[] OrderedDirectories = new string[] {};
        protected string[] OrderedFiles  = new string[] {};

        public Directory(string name, Dictionary<int, int> permissions) :
        this(string name, Dictionary<int, int> permissions)
        { }

        public Directory(string name, Dictionary<int, int> permissions) :
        base(name, permissions)
        { }

        public override string DataType()
        {
            return "directory";
        }

        private static string GetKey(Node node)
        {
            string key = node.GetName();

            if(node.DataType() == "file") {
                key += node.Extension;
            }

            return key;
        }

        // Binary search to get index of insertion or deletion point
        public static int GetIndex(string[] array, string key)
        {
            int low = 0;
            int high = array.Length - 1;
            int index = 0;

            while (low <= high) {
                index = low + ((high - low) / 2);

                if (array[index] > key) {
                    high = index - 1;
                } else if (array[index] < key) {
                    low = index + 1;
                } else {
                    return index;
                }
            }

            // If array was already empty, we insert at the first position
            return index;
        }

        public static void AddNode(Node node)
        {
            string key = GetKey(node);

            if (Nodes.ContainsKey(key)) {
                // throw error...
                return;
            }

            // Add to our children
            Nodes.Add(key, node);

            // Add key to our min heap using a binary search and insertion
            string[] orderedArray;
            if (node.DataType == "file") {
                orderedArray = OrderedFiles;
            } else {
                orderedArray = OrderedDirectories;
            }
            
            int index = GetIndex(orderedArray, key);
            string[] newArray = new string[orderedArray.Count + 1];
            string nextKey = key;

            // Assign original values to new array
            // A nice shortcut to resize and add elements to our array
            Array.Copy( orderedArray, 0, newArray, 0, orderedArray.Count);

            // Add new value and reassign next elements
            for(int ii = index; ii < newArray.Length; ii++) {
                string currentKey = newArray[ii];
                newArray[ii] = nextKey;
                nextKey = currentKey;
            }

            // Reassign property
            if (node.DataType == "file") {
                OrderedFiles = newArray;
            } else {
                OrderedDirectories = newArray;
            }
        }

        public static void DeleteNode(Node node)
        {
            string key = GetKey(node);

            if (!Nodes.ContainsKey(key)) {
                // throw error...
                return;
            }

            Nodes.remove(key);

            // Remove key from our min heap using a binary search and insertion
            string[] orderedArray;
            if (node.DataType == "file") {
                orderedArray = OrderedFiles;
            } else {
                orderedArray = OrderedDirectories;
            }
            
            int index = GetIndex(orderedArray, key);
            string[] newArray = new string[orderedArray.Count - 1];

            // Assign original values to new array
            // A nice shortcut to resize and copy our array
            Array.Copy( orderedArray, 0, newArray, 0, orderedArray.Count - 1);

            // From the end, reassign values that were deleted
            // Not the most efficient, would refactor if I had time
            for(int ii = newArray.Length - 1; ii >= index; ii--) {
                newArray[ii] = orderedArray[ii - 1];
            }

            // Reassign property
            if (node.DataType == "file") {
                OrderedFiles = newArray;
            } else {
                OrderedDirectories = newArray;
            }
        }


        // TODO: Actually list other Node properties
        public static void ListChildren()
        {
            // List Directories...
            Console.WriteLine("Directories...");
            for(int ii = 0; ii < OrderedDirectories.Length; ii++)
            {
                string key = OrderedDirectories[ii];
                Console.WriteLine(Nodes[key]);
            }

            // List Files...
            Console.WriteLine("Files...");
            for(int ii = 0; ii < OrderedFiles.Length; ii++)
            {
                string key = OrderedFiles[ii];
                Console.WriteLine(Nodes[key]);
            }
        }
    }

    public static void ChangeUser(User user)
    {
        // Ask for password...
        CurrentUser = user;
    }

    private static (string, bool) GetDirectoryInformation(string directoryPath)
    {
        bool isRoot = directoryPath[0] == "/";

        if(isRoot) {
            directoryPath = directoryPath.Substring(1, directoryPath.Length - 1);
        }

        // parse string, expected values /example/path
        string[] pathKeys = directoryPath.Split('/');

        return (pathKeys, isRoot);
    }

    private static (Node, Directory) GetNode(string directoryPath)
    {
        if (directoryPath == "") return CWD;
        (string[] pathKeys, bool isRoot) = GetDirectoryInformation(string directoryPath);

        // if path doesn't start at "/", start in CWD
        Node workingNode = CWD;
        Directory parent = null;

        // if / is first, start at root
        if (isRoot) {
            workingNode = root;
        }

        for(int ii = 0; ii < pathKeys.Length; ii++)
        {
            if(workingNode.DataType() == "file")
            {
                // throw error...
                throw new ArgumentException("A file cannot contain a directory.");
            }

            string key = pathKeys[ii];

            if(!workingNode.Nodes.ContainsKey(key)) {
                // throw an error...
                throw new ArgumentException("Please correct your file path.");
            }

            parent = workingNode;
            workingNode = workingNode.Nodes[key];

            if(!workingNode.CanRead())
            {
                // throw error...
                throw new ArgumentException("You are not permitted to read that directory");
            }
        }

        return (workingNode, parent);
    }

    public static void ChangeDirectory(string directoryPath)
    {
        (Node directory) = GetNode(directoryPath);
        if(directory.DataType() == "file")
        {
            // throw error...
            throw new ArgumentException("Cannot change directory to a file destination.");
        }

        // Permissions check in GetNode function
        CWD = directory;
    }

    // CRUD Directories
    // Create
    public static void CreateNode(string name)
    {
        if (name.Contains("/")) {
            // throw error...
            throw new ArgumentException("Cannot create directories recursively.");
        }

        if (!CWD.CanWrite()) {
            // throw error...
            throw new ArgumentException("Cannot write in this directory.");
        }

        Node node;
        Dictionary<int, bool[]> permissions = new Dictionary<int, bool[]>();
        permissions.Add(user._id, new bool[3] { CWD.CanRead(), CWD.CanWrite(), CWD.CanUpdate() });

        // TODO: real way to check if there's an extension
        if (name.Contains(".")) {
            string[] nameArray = name.Split(".");
            node = new File(nameArray[0], permissions, int[], nameArray[1]);
        } else {
            node = new Directory(name, permissions);
        }

        CWD.AddNode(node);
    }

    // Read
    public static void ReadNode(string directoryPath = "")
    {
        (Node directory) = GetNode(directoryPath);

        if(directory.DataType() == "file")
        {
            // throw error...
            throw new ArgumentException("Cannot read a file with the FileSystem Program.");
        }

        if (directory.CanRead()) {
            directory.ListChildren();
        }
    }

    // Update
    public static void MoveNode(string nodeName, string workingDirectory)
    {
        (Node fileOrDirectory, Directory parent) = GetNode(nodeName);
        (Node destination) = GetNode(workingDirectory);

        if(destination.DataType() == "file")
        {
            // throw error...
            throw new ArgumentException("Cannot move a directory or file to a file. Check your destination name.");
        }

        if(!destination.CanWrite())
        {
            // throw error...
            throw new ArgumentException("Cannot write to destination directory.");
        }

        if(!fileOrDirectory.CanWrite())
        {
            // throw error...
            throw new ArgumentException("Cannot update the destination of the file or directory.");
        }

        // May have to cast this before I can add
        destination.AddNode(fileOrDirectory);
        parent.DeleteNode(fileOrDirectory);
    }

    public static void UpdateNode(string permissions, string nodeName = "", User user = null)
    {
        if (user == null) {
            user = CurrentUser;
        }

        if(permissions.Length != 3)
        {
            // throw error...
            throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
        }

        if (user.isRoot() && !CurrentUser.isRoot()) {
            // throw error...
            throw new ArgumentException("Insufficient permissions to update permissions for this user.");
        }
        
        (Node node) = GetNode(nodeName);

        if(!node.CanUpdate()) {
           // throw error...
            throw new ArgumentException("Insufficient permissions to update permissions.");
        }

        bool[] newPermissions = new bool[3];

        for(int ii = 0; ii < permissions.Length; ii++)
        {
            if (permissions[ii] == "1") {
                newPermissions[ii] = true;
            } else if (permissions[ii] != "0") {
                // throw error...
                throw new ArgumentException("Invalid character shorthand for permissions.");
            } 
        }

        node.UpdatePermissions(user._id, newPermissions);
    }

    // Delete
    public static void DeleteNode(string nodeName = "")
    {
        (Node fileOrDirectory, Directory parent) = GetNode(nodeName);

        if(parent == null)
        {
            // They are trying to delete the root directory
            throw new ArgumentException("Cannot delete root directory");
        }

        if(fileOrDirectory.DataType() == "directory" && fileOrDirectory.Nodes.Count > 0)
        {
            // throw error...
            throw new ArgumentException("Cannot delete a directory with files in it.");
        }

        if(!fileOrDirectory.CanWrite() || !parent.CanWrite())
        {
            // throw error...
            throw new ArgumentException("Insufficient Permissions to delete file or directory.");
        }

        parent.DeleteNode(fileOrDirectory);
        // delete from memory
        fileOrDirectory = null;
    }
}

// This exists outside of the filesystem and will be handled by the OS itself
public class User
{
    // _id value is same as index in user array
    public int _id { get; };
    protected string Name;
    protected bool _root;

    public User(int id, string name, bool isRootUser = false)
    {
        _id = id;
        Name = name;
        _root = isRootUser;
    }

    public static bool isRoot()
    {
        return _root;
    }
}