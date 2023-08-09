# Assignment 4 - Application Compilation (Components/Architecture)

This branch is for assignment 4. If you were trying to view another assignment, please checkout the `main` branch of this repository to read instructions.

## Instructions to Run

To run the app, double click the shortcut `/FileSystemApp.exe`. **If the shortcut did not use a relative path**, open the executable in `/publish/FileSystemApp.exe`. Code can be reviewed in the `/FileSystemApp/` directory.

The app will prompt you to log in. There are two users you can log in with.
1. `admin` -
    - user has near unlimited privileges.
    - password is also admin.
2. `guest`
    - user has limited privileges

Merely type the user name to start the session. If a password is required, a new prompt will ask for it.

Once logged in, you can chose from the following commands:

## List of Acceptable Commands

1. `list` - lists all files and directories in the current working directory (including the current user access)
1. `move {file or directory} {destination directory}` - moves a file in the current working directory to a new location. Start the destination with "/" to reference root directory.
1. `relocate {directory}` - change the current working directory. Can be a relative or absolute path.
1. `create {file or directory}` - creates a file or directory in the current working directory. Currently does not create recursively.
1. `delete {file or directory}` - deletes a file or empty directory. Accepts relative and absolute paths.
1. `update {permission string} {file or directory}` - udpates the permissions of the file or directory. The permission string shorthand is 5 digits long (CRUDX) with 1s for permission granted and 0s for permissions denied. For example, a string of "11101" allows for all CRUDX permissions EXCEPT delete.
1. `login` - change the login user. You will be prompted with a new input for user name. Type "cancel" to cancel the login and return to the previous session.
1. `exit` - exits the application. The command prompt will close.

## Behind the scenes
The fake data is located in a file called `/publish/fake_dat.json`. This file is updated with every change and will persist from session to session.

Since the data is fake, I did not work too hard at creating clean casting models for the JSON. The JSON casts can be found in `/adapters/FakeDiskAdapter.cs`.

## Changes from Last Assignment

This branch has major over haul of the code to better fit SOLID principles per this comment:

"Open/Closed Principle: Most classes are not designed with easy extensibility in mind Heavy use of conditional logic rather than polymorphism Interface Segregation Principle: Interfaces are not really leveraged to segment APIs. What I mean by that is There is little use of interfaces to define the outward public APIs for the classes. The classes expose their implementation details directly rather than behind interfaces."

The class `FileSystemCommand` has been renamed to `InputListener` and is the only class that still lives in `Program.cs`. All other classes have been separated into folders based on their logic. `FileSystem.cs` is the only class that still lives in the root directory.

`InputListener` has had all it's logic (originally switch statements) separated into the Command design. These Commands are found in `/commands`. The Reciever for these commands is the `FileSystem` class.

Interfaces have been created for this application found in the `/interfaces` folder, however, please note that interfaces in C# work a bit different than Java, especially when abstract classes are in use. Because of this, some logic in the `/models/Node` file that would be in an interface in Java remains in the abstract class due to implementation. A major interface that was added is `IUnique`, which defines that classes will hold unique properties `Name` and `Id`.

The `Node`, `File`, and `Directory` classes have had major overhauls to reduce the amount of conditional logic found in `FileSystem` and `FakeDiskAdapter` (Reminder, `File` and `Directory` class extend the abstract class `Node`). The `Node` class now has abstract methods in place for all Directory methods, and the `File` class will throw a `NotImplementedException` if it is called. This allows for more readable code with all necessary logic living within the class itself rather than the `FileSystem` or other classes.

Similarly, the `User` and `Node` relationships have been decoupled. Before, there were methods in each that would depend on the other class, creating circular dependency. Now, the `User` class depends on the `Node` class, and the `Node` class is independent. This includes the original CRUDX method designs. Originally, it was set up as methods `CanCreate`, `CanRead`, etc. in Node that would take a `User` object as a parameter. Now they reside in the `User` class and defined in the `CRUDXInterface`. They use a `Node` object as the paramter and it reads more semantically, i.e. `user.CanRead(node)`. Furthermore, these methods no longer return a boolean, rather if a `User` cannot employ that permission then a generic `Exception` is thrown. This is because these checks were always expected to stop operations, so it didn't make sense to use conditional logic in the `FileSystem` class to do so.

`User` method `HasPassword` has been renamed to `IsPasswordProtected` to read better.

Methods `MountNode` has been renamed to `MountDisk` and `MountDiskDirectoryChildren` to `MountDiskChildren`. These methods have also been removed from the `FileSystem` class to the `DiskAdapter` class. Now, the `DiskAdapter` class is dependent on `Node`, `User`, and `Group` classes; and the `FileSystem` class is dependent on `Node`, `User`, `Group`, and `DiskAdapter` classes, once again removing circular dependency in `DiskAdapter`.
