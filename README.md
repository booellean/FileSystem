# Assignment 1 - File System:

For an in memory file system, we will design two custom classes `FileSystem` and `User` that our OS can instantiate on mount or when an end user is interacting with a CLI or GUI. Within the `FileSystem` class, we will make use of protected classes that cannot be accessed outside of our parent class (encapsulation). The first is an abstract class called `Node` that will serve as our base for all files and directories. The `Node` class contains the following properties:

1. `Name` - a string that serves as a unique identifier
2. `Permissions` - a Hashmap (Dictionary) that keeps track of read, write, and delete permissions (similar to Unix's read, write, execute).

From this we create two new classes from our Abstract Class `File` and `Directory`. The `File` class adds the following properties:

1. `Extension` - a string that identifies the type of file
2. `Data` - the data belonging to the file (note I am using an array of integers since I am not familiar with OS systems).

The `Directory` class adds the following properties:

1. `Nodes` - a Hashmap (Dictionary) of any child Files or Directories. The hashmap uses the Name or Name + Extension proprties as unique identifiers to track the Node
2. `OrderedDirectories` - an ordered array of all child directories
3. `OrderedFiles` - an ordered array of all child files

The `FileSystem` class will control how a user interfaces with these Files and Directories (see methods), and an end user will not be able to call CRUD operations from the `Directory` or `File` objects directly. It has the following properties:

1. `Root` - The root directory as `Directory` object, assigned during instantiation.
2. `CWD` - the Current Working Directory as `Directory` object. During instantiation it's the same as `Root`, but changes as a user traverses through directories.
3. `CurrentUser` - The current user as `User` Object. The user's `_id` value is used to check against permissions in the application.

The `Node` class contains methods to check if the current user has the ability to Read, Write, or Update the `Node` object. The `FileSystem` methods demonstrate how these checks are made before executing certain operations.

The `Directory` holds the boatload for Data Structures and Algorithms. The Directory system is a Tree made up of a Hashmaps (dictionaries). The `Root` Node (root directory: see line 39) is a `Directory` object, and the branches are made up of Files and Directories. Directories can continue branching but Files cannot. This is expressed in the `Directory` property `Nodes` (see line 194). This all happens during mounting when the `FileSystem` creates all `Directory` and `File` objects (see lines 55 - 111) using a Queue. Whenever a new `Directory` is created, the object is sent to the Queue with it's associated child data (psuedocode here), and will continue branching until there is no more data associated with directories.

However, the `Directory` class also contains the `OrderedDirectories` and `OrderedFiles` properties that are used to easily list out Node information for the user in the `ListChildren` method (see line 330). These are Min Heaps made up of sorted arrays containing Node keys. Whenever a `Node` is added or deleted, a Binary Search is performed in the `GetIndex` method (see line 223) to find the deletion or insertion point, and then a modified Insertion Sort (using C#'s array copying abilities) is performed to update the index position of all following array items. This adds a bit of an extra load on Insertion and Deletion, but minimizes workload of outputting all Node children information to the Console.

*/