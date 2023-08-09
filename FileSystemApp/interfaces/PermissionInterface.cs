namespace FileSystemApp;

interface IPermissions
{
    // CRUDX
    bool HasPermission(int index);
    string GetPermissionsString();
}