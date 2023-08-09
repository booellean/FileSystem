# Assignment 3 - Design Principles: SOLID

Code is run using dotnet 7. I have not tested in versions above.

An Adapter Class `DiskAdapter` has been added. This class is abstract and will handle reading, writing, and updating data on a disk outside of the `FileSystem` class Objects. Currently, the disk adapter being used is `FakeDiskAdapter` which reads a JSON file `fake_dat.json` and updates it when interfacing with the filesystem. There is a backup file `backupDebug.json` for testing purposes. `DiskNodeData`, `FakeUser`, and `FakeGroups` are debug classes that work with the DiskAdapter, but `DiskNodeData` is also used in `FileSystem` when mounting new nodes. This class will need updating since it's dependent on how disk data will be received later in the project, but `FakeUser` and `FakeGroup` only exist because of how the JSON is deserialized and will not be used later.

A new class has been added to handle Permissons called `CRUDX`. This helps to conform how permissions are utilized throughout classes, but it also serves to handle common methods such as reading a permission index, converting permisssion strings into readable boolean arrays, and converting those arrays back to permission strings.

Node classes now have an integer array property called `LocalAddress`. This address points to the location of the data on the psuedo disk. The array is a series of indexes that can be read as "levels". For example, and empty array will point to Root. An array with value [1], would point to the second item on Root's nodes array. An array with value [1,2] would point to the third item in the second item of Root's nodes array, and etc. When items are moved or deleted. When the `DiskAdapter` moves, deltes, or adds items, it will return new integer arrays to be updated to the node's property.

There was an error in the original `DeleteNode` and `AddNode` methods of the `Node` class where the key names were not actually maintening a sorted array. This error has been fixed.

Other minor changes have been made throughout the `FileSystem`, `User`, and `Group` classes to better work with the disk adapter.

Finally, the `FileSystemCommand` class has a new method called `Login` that is called at startup. This method can login any users recognized by the `FileSystem` class. To test this app as root user, the user name is `admin` and the password is `admin`.
*/