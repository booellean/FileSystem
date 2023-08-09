namespace FileSystemApp;

using System.Text.Json;

// Handles Updating Disk Data and returning that data cast as a user, node, or group
// Will also update directory node locations
sealed class FakeDiskAdapter : DiskAdapter
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
            Group[] newUserGroups = GetObjectGroups(buildData.Groups);

            // Create the new users
            User newUser = new User(buildData.Id, buildData.Name, newUserGroups, buildData.Password);
            users[ii] = newUser;
        }

        return users;
    }

    public override Group[] GetGroups() {
        Group[] groups = new Group[_debugData.Groups.Length];

        // Assign Fake Groups
        for (int ii = 0; ii < _debugData.Groups.Length; ii++) {
            FakeGroup buildData = _debugData.Groups[ii];
            Group newGroup = new Group(buildData.Id, buildData.Name, buildData.Weight, buildData.Permissions);
            groups[ii] = newGroup;
        }

        return groups;
    }

    public override int[] AddData(string nodeName, int[] parentAddress, Group[] groups) {
        DiskNodeData ParentDirectory = GetDiskNode(parentAddress);
        DiskNodeData newDiskNode;
        int[] groupIds = new int[groups.Length];

        // Converting Groups to array of group ids
        for (int ii = 0; ii < groups.Length; ii++) {
            groupIds[ii] = groups[ii].Id;
        }

        if (IsFile(nodeName)) {
            string[] fileParts = FileParts(nodeName);
            newDiskNode  = new DiskNodeData{
                Id = _debugData.NextNodeId,
                Name = fileParts[0],
                Groups = groupIds,
                Address = new int[2] {1,0},
                Extension = fileParts[1],
            };
        } else {
            newDiskNode  = new DiskNodeData{
                Id = _debugData.NextNodeId,
                Name = nodeName,
                Groups = groupIds,
                Nodes = Array.Empty<DiskNodeData>(),
            };
        }

        // Increment next node id
        ++_debugData.NextNodeId;
        return AppendNodeToParent(newDiskNode, ParentDirectory, parentAddress);

    }
    public override bool DeleteData(int[] locationAddress)
    {
        // Get the Parent by counting 1 less index point;
        int lastArrayIndex = locationAddress.Length - 1;
        int[] parentAddress = GetParentAddress(locationAddress);
        int lastLocationIndex = locationAddress[lastArrayIndex];
        DiskNodeData ParentLocation = GetDiskNode(parentAddress);

        if (ParentLocation.Nodes != null && lastLocationIndex < ParentLocation.Nodes.Length) {
            DiskNodeData[] newNodes = new DiskNodeData[ParentLocation.Nodes.Length - 1];
            Array.Copy(ParentLocation.Nodes, 0, newNodes, 0, ParentLocation.Nodes.Length - 1);

            // Remove that node address and replace
            for(int ii = newNodes.Length - 1; ii >= lastLocationIndex; ii--) {
                newNodes[ii] = ParentLocation.Nodes[ii + 1];
            }

            // Reassign nodes
            ParentLocation.Nodes = newNodes;
            SaveFile();
            // Return nodes to assign new locations
            return true;

        } else {
            throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
        }
    }
    public override int[] MoveData(int[] targetNodeAddress, int[] destinationAddress)
    {
        DiskNodeData TargetNode = GetDiskNode(targetNodeAddress);
        DiskNodeData DestinationLocation = GetDiskNode(destinationAddress);

        if (DestinationLocation.Extension != null) {
            throw new ArgumentException("The site destination was a file. Please correct.");
        }

        // First, delete the target node
        int[] newAddress = AppendNodeToParent(TargetNode, DestinationLocation, destinationAddress);
        DeleteData(targetNodeAddress);

        SaveFile();
        return newAddress;
    }

    public override void UpdateData(int[] locationAddress, string permissions, string userId)
    {
        if(permissions.Length < 4 || permissions.Length > 5)
        {
            // throw error...
            throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
        }

        DiskNodeData TargetNode = GetDiskNode(locationAddress);

        if (TargetNode.user_permissions == null) {
            TargetNode.user_permissions = new Dictionary<string, string>();
        }
        
        TargetNode.user_permissions.Remove(userId);
        TargetNode.user_permissions.Add(userId, permissions);

        SaveFile();
    }

    private DiskNodeData GetDiskNode(int[] locationAddress)
    {
        // Get the current Fake Data location of current Node
        DiskNodeData requestedDisk = _debugData.Root;

        for (int ii = 0; ii < locationAddress.Length; ii++) {
            int index = locationAddress[ii];

            if (requestedDisk.Nodes == null || index < 0 || index >= requestedDisk.Nodes.Length) {
                throw new ArgumentException("There was an error with the test data. Please fix.");
            }

            requestedDisk = requestedDisk.Nodes[index];
        }

        return requestedDisk;
    }

    public override Node MountDisk(int[] locationAddress)
    {
        DiskNodeData buildData = GetDiskNode(locationAddress);
        Node newNode;
        Group[] newNodeGroups = GetObjectGroups(buildData.Groups);

        if (buildData.Extension == null) {
            newNode = new Directory(buildData.Id, buildData.Name, newNodeGroups, locationAddress);
        } else {
            newNode = new File(buildData.Id, buildData.Name, newNodeGroups, locationAddress, locationAddress, buildData.Extension);
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

    public override void MountDiskChildren(Node workingDirectory)
    {
        // This is Mounting for the first time
        if (!workingDirectory.NodeIsMounted()) {
            // Get the current Fake Data location of current Node
            DiskNodeData DataLocation = GetDiskNode(workingDirectory.LocalAddress);
            // create a base to use for new fake data location
            int lastLocIndex = workingDirectory.LocalAddress.Length;
            int arrLocLength = lastLocIndex+1;

            // Loop through children, map addresses and Mount Node
            if (DataLocation.Nodes != null) {
                for (int ii = 0; ii < DataLocation.Nodes.Length; ii++) {
                    // Set up the new disk location address
                    int[] baseLocation = new int[arrLocLength];
                    Array.Copy( workingDirectory.LocalAddress, 0, baseLocation, 0, lastLocIndex);
                    baseLocation[lastLocIndex] = ii;

                    // Assign custom permissions
                    Node node = MountDisk(baseLocation);

                    // Add the node as a child
                    workingDirectory.AddNode(node);
                }

                workingDirectory.SetNodeMounted();
            }
        }
    }

    public override void UpdateDiskMount(Node workingDirectory)
    {
        // Check if it was mounted to be safe
        if (!workingDirectory.NodeIsMounted()) {
            MountDiskChildren(workingDirectory);
        } else  {
            // Get the current Fake Data location of current Node
            DiskNodeData DataLocation = GetDiskNode(workingDirectory.LocalAddress);
            // create a base to use for new fake data location
            int lastLocIndex = workingDirectory.LocalAddress.Length;

            if (DataLocation.Nodes != null) {
            // Loop through children, map addresses, and udpate existing node children
                for (int ii = 0; ii < DataLocation.Nodes.Length; ii++) {
                    // Get the Node Child of our Directory
                    DiskNodeData currentDiskNode = DataLocation.Nodes[ii];
                    string nodeKey = currentDiskNode.GetKey();

                    // Update Address
                    Node currentChildNode = workingDirectory.GetChildNode(nodeKey);
                    currentChildNode.LocalAddress[lastLocIndex] = ii;

                    ((Directory)workingDirectory).Nodes.Remove(nodeKey);
                    ((Directory)workingDirectory).Nodes.Add(nodeKey, currentChildNode);
                }
            }
        }
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

    // TODO: better way of doing this...
    private Group[] GetObjectGroups(int[] groupIds)
    {
        Group[] AllGroups = GetGroups();
        Group[] NewGroups = new Group[groupIds.Length];

        for (int ii = 0; ii < NewGroups.Length; ii++) {
            Group? group = Array.Find(AllGroups, group => group.Id == groupIds[ii]);

            if (group != null) NewGroups[ii] = group;
        }

        return NewGroups;
    }

    private int[] AppendNodeToParent(DiskNodeData newDiskNode, DiskNodeData ParentDirectory, int[] parentAddress)
    {
        if (ParentDirectory.Nodes == null) {
            throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
        }

        // Create our new Node Array
        int lastNodeIndex = ParentDirectory.Nodes.Length;
        DiskNodeData[] newNodes = new DiskNodeData[lastNodeIndex + 1];
        Array.Copy(ParentDirectory.Nodes, 0, newNodes, 0, lastNodeIndex);

        // Get the Address location of our new node
        int lastAddressIndex = parentAddress.Length;
        int[] newChildAddress = new int[lastAddressIndex + 1];
        Array.Copy(parentAddress, 0, newChildAddress, 0, lastAddressIndex);
        newChildAddress[lastAddressIndex] = lastNodeIndex;

        // Assign the new child
        newNodes[lastNodeIndex] = newDiskNode;
        ParentDirectory.Nodes = newNodes;

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

        // DEBUG: All classes below are for writing fake data to a json file for testing user sessions
    protected class Data
    {
        public DiskNodeData Root { get; set; }
        public FakeUser[] Users { get; set; }
        public FakeGroup[] Groups { get; set; }
        public int NextNodeId { get; set; }
        public int NextGroupId { get; set; }
        public int NextUserId { get; set; }
    }

    public class DiskNodeData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int[] Groups { get; set; }
        public string? Extension { get; set; }
        public int[]? Address { get; set; }
        public Dictionary<string, string>? user_permissions { get; set; }
        public DiskNodeData[]? Nodes { get; set; }

        public string GetKey()
        {
            string keyName = this.Name;
            if (this.Extension != null) keyName += "." + this.Extension;
            return keyName;
        }
    }

    public class FakeUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Password { get; set; }
        public int[] Groups { get; set; }
    }

    public class FakeGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
        public int Weight { get; set; }
    }
}
