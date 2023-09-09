// Initializes the application. Will Handle communication between the end user and FileSystem

using System;
using System.Collections;
using System.Collections.Generic;

namespace FileSystemApp;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Application...");

        InputListener fileSystem = new InputListener();

        Console.WriteLine("Application Startup Successful!\n");

        Console.WriteLine("You may exit the application at any time by typing \"exit\".");

        Console.WriteLine("Before you get started, please login. (Hint: you can enter as a guest by typing \"guest\").\n");
        fileSystem.InitiateLogin();
    }

    protected class InputListener
    {
        // Flexible assignment from instance to instance, but uses the same unique instantiations
        protected static string CWD = "/";
        protected static int UserId = -1;
        protected static string AuthToken = "";
        protected FileSystem System = FileSystem.Instance;
        protected Dictionary<string, ICommand> Commands;
        
        public InputListener() {
            Commands = new Dictionary<string, ICommand>() {
                { "list", new ListCommand(System) },
                { "move", new MoveCommand(System) },
                { "relocate", new RelocateCommand(System) },
                { "create", new CreateCommand(System) },
                { "delete", new DeleteCommand(System) },
                { "update", new UpdateCommand(System) },
                { "login", new LoginCommand(System) },
                { "exit", new ExitCommand(System) },
            };
        }

        public void InitiateLogin()
        {
            // Run the login command
            Commands["login"].Execute(Array.Empty<string>(), ref CWD, ref UserId, ref AuthToken);

            // A valid user was not found. Exit
            if (UserId == -1) return;

            // Call "FinalizeSetup" so Singleton has at least the root mounted when 1 user logs in
            System.FinalizeSetup(AuthToken);
            Listen();
        }

        // Starts a command line listening operation
        public void Listen()
        {
            bool listening = true;
            string requestValidCommand = "Please enter a valid command.";

            while (listening) {
                Console.Write(CWD + " >  ");
                string? command = Console.ReadLine();

                // Verify there was input
                if (command == null) {
                    Console.WriteLine(requestValidCommand);
                }


                // Get command and arguments
                string[] arguments = ((string)command).Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                if (arguments.Length < 1) {
                    Console.WriteLine(requestValidCommand);
                }

                try {
                    // The command key, listed in the Commands Property
                    string commandKey = arguments[0];

                    // See if command is valid based on Commands property
                    if (Commands.ContainsKey(commandKey)) {
                        listening = Commands[commandKey].Execute(arguments, ref CWD, ref UserId, ref AuthToken);
                    } else {
                        Console.WriteLine(requestValidCommand);
                    }

                } catch(Exception error) {
                    Console.WriteLine(error.Message);
                }
            }
        }
    }
}