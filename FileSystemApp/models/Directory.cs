namespace FileSystemApp;
using System.Text.Json.Serialization;

class Directory : Node
{
    private bool Mounted = false;
    public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();
    protected string[] OrderedDirectories = Array.Empty<string>();
    protected string[] OrderedFiles  = Array.Empty<string>();

    [JsonConstructor]
    public Directory(int id, string name, Group[] groups, Dictionary<string, string>? userPermissions = null) :
    base(id, name, groups, userPermissions)
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

    public override Node GetChildNode(string key)
    {
        if (Nodes.ContainsKey(key)) return Nodes[key];
        // throw error...
        throw new ArgumentException("The item \"{0}\" was not found in the directory.", key);
    }

    public override bool NodeIsMounted()
    {
        return Mounted;
    }

    public override void SetNodeMounted()
    {
        Mounted = true;
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

    public override void AddNode(Node node)
    {
        string key = node.GetKey();

        if (Nodes.ContainsKey(key)) {
            // throw error...
            throw new ArgumentException("The file or directory was not found in the directory {0}.", key);
        }

        // Add to our children
        Nodes.Add(key, node);

        // Add key to our min heap using a binary search and insertion
        if (node.IsFile()) {
            OrderedFiles = AddToArray(key, OrderedFiles);
        } else {
            OrderedDirectories = AddToArray(key, OrderedDirectories);
        }
    }

    public override void DeleteNode(Node node)
    {
        // Verify this is not a node with children
        if (!node.IsFile() && ((Directory)node).Nodes.Count > 0) {
            // throw error...
            throw new ArgumentException("Cannot delete a directory with files in it.");
        }

        // Update object
        string key = node.GetKey();

        if (!Nodes.ContainsKey(key)) {
            // throw error...
            throw new ArgumentException("The file or directory was not found in the directory {0}.", key);
        }

        // Delete from Hash
        Nodes.Remove(key);

        // Remove key from our min heap using a binary search and insertion
        if (node.IsFile()) {
            OrderedFiles = DeleteFromArray(key, OrderedFiles);
        } else {
            OrderedDirectories = DeleteFromArray(key, OrderedDirectories);
        }

    }

    private string[] DeleteFromArray(string key, string[] orderedArray)
    {
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

        return newArray;
    }

    private string[] AddToArray(string key, string[] orderedArray)
    {
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

        return newArray;
    }

    public string[] GetOrderedDirectories()
    {
        return OrderedDirectories;
    }

    public string[] GetOrderedFiles()
    {
        return OrderedFiles;
    }
}