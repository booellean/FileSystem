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
    // Test Url
    private string LocationUrl = "http://localhost";
    private JsonSerializerOptions Options = new JsonSerializerOptions
    {
        IncludeFields = true,
    };

    public ApiDiskAdapter() : base()
    {
        // Default Default Headers
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
            Message? message = JsonSerializer.Deserialize<Message>(token);
            throw new PasswordNeededException(message != null ? message.message : "UnKnown Error");
        } else {
            HttpRequestMessage webRequestUser = new HttpRequestMessage(HttpMethod.Get, LocationUrl + "/api/user");
            SetAuthorization(token);

            response = Client.Send(webRequestUser);
            reader = new StreamReader(response.Content.ReadAsStream());

            int userId = int.Parse(MakeGetApiCallAndReturn(token, "/api/user"));
            return (userId, token);
        }
    }

    public override User[] GetUsers(string authToken)
    {
        string jsonString = MakeGetApiCallAndReturn(authToken, "/api/users");
        User[]? users = JsonSerializer.Deserialize<User[]>(jsonString) ?? throw new Exception("Unknown Error.");
        return users;
    }

    public override Group[] GetGroups(string authToken)
    {
        string jsonString = MakeGetApiCallAndReturn(authToken, "/api/groups");
        Group[]? groups = JsonSerializer.Deserialize<Group[]>(jsonString) ?? throw new Exception("Unknown Error.");
        return groups;
    }

    public override Node AddData(string authToken, string nodeName, string nodeType, int parentId)
    {
        string path = "/api/node/create/" + parentId;
        bool isFile = nodeType.Equals("file") || nodeType.Equals("f");
        
        string body = "{\"name\": \"";

        if (isFile) {
            (string extension, string name) = GetFileParts(nodeName);
            body += name + "\", \"extension\": \"" + extension + "\", \"data\": \".";
        } else {
            body += nodeName;
        }

        body += "\"}";

        string jsonString = MakePostApiCallAndReturn(authToken, path, body);

        Node? node;
        
        // TODO: alter casting of Node Type to infer Directory or File... Not enough time
        if (isFile) {
            node = JsonSerializer.Deserialize<File>(jsonString, Options) ?? throw new Exception("Unknown Error.");
        } else {
            node = JsonSerializer.Deserialize<Directory>(jsonString, Options) ?? throw new Exception("Unknown Error.");
        }

        return node;
    }
    public override void DeleteData(string authToken, Node node)
    {
        string path = "/api/node/delete/" + node.DataType() + "/" + node.Id;

        // If the server errors out, the call will throw the error message to the frontend user
        MakeGetApiCallAndReturn(authToken, path);
    }

    public override void MoveData(string authToken, Node targetNode, int destinationId)
    {
        string path = "/api/node/move/" + destinationId + "/" + targetNode.DataType() + "/" + targetNode.Id;

        // If the server errors out, the call will throw the error message to the frontend user
        MakeGetApiCallAndReturn(authToken, path);
    }

    public override void UpdateData(string authToken, Node node, string permissions, string userId)
    {
        if(permissions.Length < 4 || permissions.Length > 5)
        {
            // throw error...
            throw new ArgumentException("Please enter a valid shorthand for permissions. Example 111 allows for read, write, and update.");
        }

        string path = "/api/node/update/" + node.DataType() + "/" + node.Id + "/" + permissions + "/" + userId;

        // If the server errors out, the call will throw the error message to the frontend user
        MakeGetApiCallAndReturn(authToken, path);
    }

    public override Node MountDisk(string authToken)
    {
        string path = "/api/node/mount";

        string nodeString = MakeGetApiCallAndReturn(authToken, path);

        Directory? node = JsonSerializer.Deserialize<Directory>(nodeString) ?? throw new Exception("Unknown Error.");
        return node;
    }

    public override void MountDiskChildren(string authToken, Node workingDirectory)
    {
        if (!workingDirectory.NodeIsMounted()) {
            string nodeChildrenString = MakeGetApiCallAndReturn(authToken, "/api/node/read/directory/" + workingDirectory.Id);
            DirectoryChildren? nodes = JsonSerializer.Deserialize<DirectoryChildren>(nodeChildrenString, Options) ?? throw new Exception("Unknown Error.");

            // Loop through children and add to Directory
            for (int ii = 0; ii < nodes.files.Length; ii++)
            {
                File file = nodes.files[ii];
                workingDirectory.AddNode(file);
            }

            for (int ii = 0; ii < nodes.directories.Length; ii++)
            {
                Directory directory = nodes.directories[ii];
                workingDirectory.AddNode(directory);
            }

            workingDirectory.SetNodeMounted();
        }
    }

    private (string, string) GetFileParts(string nodeName)
    {
        string[] fileParts = nodeName.Split('.');

        if (fileParts.Length < 2) return ("", nodeName);

        int lastIndex = fileParts.Length - 1;
        string extension = fileParts[lastIndex];
        string name = fileParts[0];

        for (int ii = 1; ii < lastIndex; ii++) {
            name += "." + fileParts[ii];
        }

        return (extension, name);
    }

    private string MakeGetApiCallAndReturn(string authToken, string path)
    {
        HttpRequestMessage webRequest = new HttpRequestMessage(HttpMethod.Get, LocationUrl + path);
        SetAuthorization(authToken);

        HttpResponseMessage response = Client.Send(webRequest);

        StreamReader reader = new StreamReader(response.Content.ReadAsStream());

        string output = reader.ReadToEnd();

        if (!response.IsSuccessStatusCode) {
            Message? message = JsonSerializer.Deserialize<Message>(output);
            throw new PasswordNeededException(message != null ? message.message : "UnKnown Error");
        }

        return output;
    }

    private string MakePostApiCallAndReturn(string authToken, string path, string body)
    {
        HttpRequestMessage webRequestToken = new HttpRequestMessage(HttpMethod.Post, LocationUrl + path){
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        SetAuthorization(authToken);

        HttpResponseMessage response = Client.Send(webRequestToken);
        StreamReader reader = new StreamReader(response.Content.ReadAsStream());

        string output = reader.ReadToEnd();

        if (!response.IsSuccessStatusCode) {
            Message? message = JsonSerializer.Deserialize<Message>(output);
            throw new PasswordNeededException(message != null ? message.message : "UnKnown Error");
        }

        return output;
    }

    private void SetAuthorization(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
