namespace FileSystemApp;

using System.IO;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

// Handles Updating Disk Data and returning that data cast as a user, node, or group
// Will also update directory node locations
sealed class ApiDiskAdapter : DiskAdapter
{
    private HttpClient Client = new HttpClient();
    // private string LocationUrl = "http://43.231.234.79";
    private string LocationUrl = "http://localhost";

    public ApiDiskAdapter() : base()
    {
        // Default Default Headers
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public class Test {
        public string Name;
        public string Password;
    }

    public override (int, string) LoginUser(string username, string password = "")
    {
        // Authorize a Login
        string body = "{\"name\": \""+username+"\", \"password\": \""+password+"\" }";
        HttpRequestMessage webRequestToken = new HttpRequestMessage(HttpMethod.Post, LocationUrl + "/api/auth/login"){
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response = Client.Send(webRequestToken);
        StreamReader reader = new StreamReader(response.Content.ReadAsStream());
        string token = reader.ReadToEnd();

        if (response.StatusCode == HttpStatusCode.MethodNotAllowed) {
            Message message = JsonSerializer.Deserialize<Message>(token);
            throw new PasswordNeededException(message?.message);
        } else {
            HttpRequestMessage webRequestUser = new HttpRequestMessage(HttpMethod.Get, LocationUrl + "/api/user");
            SetAuthorization(token);

            response = Client.Send(webRequestUser);
            reader = new StreamReader(response.Content.ReadAsStream());

            int userId = int.Parse(reader.ReadToEnd());
            return (userId, token);
        }
    }

    public override User[] GetUsers(string authToken)
    {
        HttpRequestMessage webRequest = new HttpRequestMessage(HttpMethod.Get, LocationUrl + "/api/users");
        SetAuthorization(authToken);

        HttpResponseMessage response = Client.Send(webRequest);
        StreamReader reader = new StreamReader(response.Content.ReadAsStream());

        string jsonString = reader.ReadToEnd();
        Console.WriteLine(jsonString);
        User[]? users = JsonSerializer.Deserialize<User[]>(jsonString);

        if(users != null) {
            for (int i = 0; i < users.Length; i++) {
                Console.WriteLine(users[i].Id);
                for(int j = 0; j < users[i].Groups.Length; j++) {
                    Console.WriteLine("Group");
                    Console.WriteLine(users[i].Groups[j].Name);
                }
            }
        }


        if (users == null) throw new Exception("Unknown Error.");
        
        return users;
    }

    public override Group[] GetGroups(string authToken) {
        HttpRequestMessage webRequest = new HttpRequestMessage(HttpMethod.Get, LocationUrl + "/api/groups");
        SetAuthorization(authToken);

        HttpResponseMessage response = Client.Send(webRequest);
        StreamReader reader = new StreamReader(response.Content.ReadAsStream());

        string jsonString = reader.ReadToEnd();
        Group[]? groups = JsonSerializer.Deserialize<Group[]>(jsonString);

        if (groups == null) throw new Exception("Unknown Error.");
        
        return groups;
    }

    public override int AddData(string authToken, string nodeName, int parentId, Group[] groups) {
        // DiskNodeData ParentDirectory = GetDiskNode(parentAddress);
        // DiskNodeData newDiskNode;
        // int[] groupIds = new int[groups.Length];

        // // Converting Groups to array of group ids
        // for (int ii = 0; ii < groups.Length; ii++) {
        //     groupIds[ii] = groups[ii].Id;
        // }

        // if (IsFile(nodeName)) {
        //     string[] fileParts = FileParts(nodeName);
        //     newDiskNode  = new DiskNodeData{
        //         Id = _debugData.NextNodeId,
        //         Name = fileParts[0],
        //         Groups = groupIds,
        //         Address = new int[2] {1,0},
        //         Extension = fileParts[1],
        //     };
        // } else {
        //     newDiskNode  = new DiskNodeData{
        //         Id = _debugData.NextNodeId,
        //         Name = nodeName,
        //         Groups = groupIds,
        //         Nodes = Array.Empty<DiskNodeData>(),
        //     };
        // }

        // // Increment next node id
        // ++_debugData.NextNodeId;
        // return AppendNodeToParent(newDiskNode, ParentDirectory, parentAddress);
        // TODO:::
        return 1;
    }
    public override bool DeleteData(string authToken, int locationId)
    {
        // // Get the Parent by counting 1 less index point;
        // int lastArrayIndex = locationId.Length - 1;
        // int parentId = GetParentAddress(locationId);
        // int lastLocationIndex = locationId[lastArrayIndex];
        // DiskNodeData ParentLocation = GetDiskNode(parentAddress);

        // if (ParentLocation.Nodes != null && lastLocationIndex < ParentLocation.Nodes.Length) {
        //     DiskNodeData[] newNodes = new DiskNodeData[ParentLocation.Nodes.Length - 1];
        //     Array.Copy(ParentLocation.Nodes, 0, newNodes, 0, ParentLocation.Nodes.Length - 1);

        //     // Remove that node address and replace
        //     for(int ii = newNodes.Length - 1; ii >= lastLocationIndex; ii--) {
        //         newNodes[ii] = ParentLocation.Nodes[ii + 1];
        //     }

        //     // Reassign nodes
        //     ParentLocation.Nodes = newNodes;
        //     SaveFile();
        //     // Return nodes to assign new locations
        //     return true;

        // } else {
        //     throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
        // }
        // TODO:::
        return false;
    }
    public override void MoveData(string authToken, int targetNodeId, int destinationId)
    {
        // DiskNodeData TargetNode = GetDiskNode(targetNodeAddress);
        // DiskNodeData DestinationLocation = GetDiskNode(destinationAddress);

        // if (DestinationLocation.Extension != null) {
        //     throw new ArgumentException("The site destination was a file. Please correct.");
        // }

        // // First, delete the target node
        // int[] newAddress = AppendNodeToParent(TargetNode, DestinationLocation, destinationAddress);
        // DeleteData(targetNodeAddress);

        // SaveFile();
        // return newAddress;
        // TODO:::
    }

    public override void UpdateData(string authToken, int locationId, string permissions, string userId)
    {
        // if(permissions.Length < 4 || permissions.Length > 5)
        // {
        //     // throw error...
        //     throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
        // }

        // DiskNodeData TargetNode = GetDiskNode(locationId);

        // if (TargetNode.user_permissions == null) {
        //     TargetNode.user_permissions = new Dictionary<string, string>();
        // }
        
        // TargetNode.user_permissions.Remove(userId);
        // TargetNode.user_permissions.Add(userId, permissions);

        // SaveFile();
        // TODO:::
    }

    public override Node MountDisk(string authToken, int locationId = 0)
    {
        // Node newNode;

        // if (locationId == 0) {
        //     // get root node
        // } else {
            
        // }
        
        // Group[] newNodeGroups = GetObjectGroups(buildData.Groups);

        // if (buildData.Extension == null) {
        //     newNode = new Directory(buildData.Id, buildData.Name, newNodeGroups, locationId);
        // } else {
        //     newNode = new File(buildData.Id, buildData.Name, newNodeGroups, locationId, locationId, buildData.Extension);
        // }

        // // If custom permissions were set, update that node using root
        // if (buildData.user_permissions != null) {
        //     foreach (KeyValuePair<string, string> entry in buildData.user_permissions) {
        //         try
        //         {
        //             int userId = Int32.Parse(entry.Key);
        //             newNode.UpdatePermissions(userId, entry.Value);
        //         }
        //         catch (FormatException)
        //         {
        //             Console.WriteLine("User Permissions were not formatted properly on the disk.");
        //         }
        //     }
        // }

        // return newNode;
        // TODO:::
        return new Directory(1, "fake", new Group[2]);
    }

    public override void MountDiskChildren(string authToken, Node workingDirectory)
    {
        // // This is Mounting for the first time
        // if (!workingDirectory.NodeIsMounted()) {
        //     // Get the current Fake Data location of current Node
        //     DiskNodeData DataLocation = GetDiskNode(workingDirectory.LocalAddress);
        //     // create a base to use for new fake data location
        //     int lastLocIndex = workingDirectory.LocalAddress.Length;
        //     int arrLocLength = lastLocIndex+1;

        //     // Loop through children, map addresses and Mount Node
        //     if (DataLocation.Nodes != null) {
        //         for (int ii = 0; ii < DataLocation.Nodes.Length; ii++) {
        //             // Set up the new disk location address
        //             int[] baseLocation = new int[arrLocLength];
        //             Array.Copy( workingDirectory.LocalAddress, 0, baseLocation, 0, lastLocIndex);
        //             baseLocation[lastLocIndex] = ii;

        //             // Assign custom permissions
        //             Node node = MountDisk(baseLocation);

        //             // Add the node as a child
        //             workingDirectory.AddNode(node);
        //         }

        //         workingDirectory.SetNodeMounted();
        //     }
        // }
        // TODO:::
    }

    public override void UpdateDiskMount(string authToken, Node workingDirectory)
    {
        // // Check if it was mounted to be safe
        // if (!workingDirectory.NodeIsMounted()) {
        //     MountDiskChildren(workingDirectory);
        // } else  {
        //     // Get the current Fake Data location of current Node
        //     DiskNodeData DataLocation = GetDiskNode(workingDirectory.LocalAddress);
        //     // create a base to use for new fake data location
        //     int lastLocIndex = workingDirectory.LocalAddress.Length;

        //     if (DataLocation.Nodes != null) {
        //     // Loop through children, map addresses, and udpate existing node children
        //         for (int ii = 0; ii < DataLocation.Nodes.Length; ii++) {
        //             // Get the Node Child of our Directory
        //             DiskNodeData currentDiskNode = DataLocation.Nodes[ii];
        //             string nodeKey = currentDiskNode.GetKey();

        //             // Update Address
        //             Node currentChildNode = workingDirectory.GetChildNode(nodeKey);
        //             currentChildNode.LocalAddress[lastLocIndex] = ii;

        //             ((Directory)workingDirectory).Nodes.Remove(nodeKey);
        //             ((Directory)workingDirectory).Nodes.Add(nodeKey, currentChildNode);
        //         }
        //     }
        // }
        // TODO:::
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

    // private int[] AppendNodeToParent(DiskNodeData newDiskNode, DiskNodeData ParentDirectory, int parentId)
    // {
    //     if (ParentDirectory.Nodes == null) {
    //         throw new ArgumentException("The fake data's nodes do not contain that index. Please check file");
    //     }

    //     // Create our new Node Array
    //     int lastNodeIndex = ParentDirectory.Nodes.Length;
    //     DiskNodeData[] newNodes = new DiskNodeData[lastNodeIndex + 1];
    //     Array.Copy(ParentDirectory.Nodes, 0, newNodes, 0, lastNodeIndex);

    //     // Get the Address location of our new node
    //     int lastAddressIndex = parentAddress.Length;
    //     int[] newChildAddress = new int[lastAddressIndex + 1];
    //     Array.Copy(parentAddress, 0, newChildAddress, 0, lastAddressIndex);
    //     newChildAddress[lastAddressIndex] = lastNodeIndex;

    //     // Assign the new child
    //     newNodes[lastNodeIndex] = newDiskNode;
    //     ParentDirectory.Nodes = newNodes;

    //     SaveFile();

    //     return newChildAddress;
    // }

    private void SetAuthorization(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
