# Assignment 7 - Application Compilation (Complete)

This branch is for assignment 6. If you were trying to view another assignment, please checkout the `main` branch of this repository to read instructions.

## Instructions to Run

To run the app, open the executable in `/publish/FileSystemApp.exe`. Code can be reviewed in the `/FileSystemApp/` directory.

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
1. `create {file/f|directory/d} {name} {target directory?}` - creates a file or directory in the current working directory. Currently does not create recursively.
1. `delete {file or directory}` - deletes a file or empty directory. Accepts relative and absolute paths.
1. `update {permission string} {file or directory}` - udpates the permissions of the file or directory. The permission string shorthand is 5 digits long (CRUDX) with 1s for permission granted and 0s for permissions denied. For example, a string of "11101" allows for all CRUDX permissions EXCEPT delete.
1. `login` - change the login user. You will be prompted with a new input for user name. Type "cancel" to cancel the login and return to the previous session.
1. `exit` - exits the application. The command prompt will close.

## Behind the scenes
The fake data is located in a file called `/publish/fake_dat.json`. This file is updated with every change and will persist from session to session.

Since the data is fake, I did not work too hard at creating clean casting models for the JSON. The JSON casts can be found in `/FileSystemApp/adapters/FakeDiskAdapter.cs`.

## Changes from Last Assignment

This version of the app is connected directly to the [API](https://github.com/booellean/FileSystemAPI) that persists files and directories online. The url is [http://43.231.234.79/](http://43.231.234.79/). Be warned, there is no SSL certificate setup.

There is a new adapter called `ApiDiskAdapter.cs` to connect the cli interface with the webapp. This adapter makes api calls and manages data sent back to keep data in sync with what's stored in the cloud. Models have been updated and created to cast returned JSON to fit the models of this application.

The `DiskAdapter` abstract class now takes `Node` objects as parameters instead of just ids. When I was originally designing this, I figured I should pass the data along like I would an API call, but upon further development, I realized that didn't make sense. Ideally, the more adapters could be written where they could write data to a local machine as well, so what was passed to it was relaxed.

Because locations are managed by the cloud app, the `localAddress` and `address` variables on the `Node` models have been removed. Models have also been updated to work with .Net's JSON casting.

Because an authtoken is needed to work with an API, the primary `Program.cs` file has been updated to house this token and pass it to commands which in turn pass it to the `FileSystem` app, which is used in the `ApiDiskAdapter` to make calls. The idea is that the CLI is a user interface that could have many instances while the `FileSystem` and `ApiDiskAdapter` work as a single source of truth, so any identifiable tokens should remain in that user interface. If I had more time, I would modify the `DiskAdapter` class so the authToken parameter is optional, allowing for other types of disk writing later.
