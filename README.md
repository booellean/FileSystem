# Assignment 2 - Design Principles: Design Patterns

Code has been refactored to run in a command line interface with dotnet. My version is a little outdated and is using dotnet 7.

Per previous feedback, a new class `Group` has been added. Currently `User` and `Node` classes have `Groups`, and this controls the default permissions for a given user and a given file or directory. Currently, the `Group` just acts as a catch all, and more complex permission mapping will be added later when I can incorporate SOLID.

Much of my previous code already incorporated Encapsulation, Inheritance and Polymorphism. I've updated the code to be in a proper working state an added an extra layer of encapsulation by adding the `FileSystemCommand` class.

`FileSystem` class is now nested in `FileSystemCommand` and is a Singleton. All `Directory` and `File` objects will now be protected from duplication thanks to that encapsulation.

Properties `CWD` and `CurrentUser` have been moved to `FileSystemCommand` class since these can change depending on who's instantiated the app. However, since `User` and `Directory` classes are only accessible in the `FileSystem` class, the properties have been changed to `string CWD` and `int _userId`. Methods in the `FileSystem` class and nested classes have been updated to account for this.

Class `Node` methods `DataType` and `GetKey` are now abstract and fully utilize polymorphism with C# standards. They must be explicitly defined in extended classes. There was some general clean up. Node permission check methods have been updated to CRUDX though not all checks have been incorporated yet. Node `HasPermission` method has been updated to compare against both custom permissions and shared groups with current user.

There wasn't really a need to incorporate any more design patterns other than Singleton. Arguabley, the `DataType()` method in the `Node` class is similar to a Strategy design, but as is, it just serves as a convenient "if" check of what the `Node` Type is. `UpdatePermissions` in the derived classes `File` and `Directory` could be updated to use the Strategy Design pattern, but there are so many similarities between how they udpate that doing so would go against DRY. Perhaps once real data starts being used, there will be more need to flesh out these designs.
