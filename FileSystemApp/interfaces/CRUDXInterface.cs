namespace FileSystemApp;


interface ICRUDX
{
    // CRUDX
    void CanCreate(Node node);
    void CanRead(Node node);
    void CanUpdate(Node node);
    void CanDelete(Node? node);
    void CanExecute(Node node);

}