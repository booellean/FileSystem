namespace FileSystemApp;
using System.Text.Json.Serialization;

class File : Node
{
    [JsonPropertyName("extension")]
    public string Extension;

    [JsonConstructor]
    public File(int id, string name, Group[] groups, int[] address, string extension, Dictionary<string, string>? userPermissions = null) :
    base(id, name, groups, userPermissions)
    {
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

    public override Node GetChildNode(string key)
    {
        // throw Not implemented error...
        throw new NotImplementedException("A file is not a directory. Please correct your path.");
    }

    public override bool NodeIsMounted()
    {
        // A File is always considered mounted
        return true;
    }

    public override void SetNodeMounted()
    {
        // throw Not implemented error...
        throw new NotImplementedException("Only a directory can be mounted.");
    }

    public override void AddNode(Node node)
    {
        // throw Not implemented error...
        throw new NotImplementedException("A file cannot \"house\" another file or directory.");
    }

    public override void DeleteNode(Node node)
    {
        // throw Not implemented error...
        throw new NotImplementedException("A file does not contain another file or directory.");
    }
}
