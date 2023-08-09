namespace FileSystemApp;

class CRUDX : IPermissions
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