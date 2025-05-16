using System.Diagnostics;
using System.Text.Json;

namespace DeskSwap;

public static class Program
{

    public static void Main(string[] args)
    {
        var configFile = File.ReadAllText("config.json");
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(configFile);
        if (config == null)
        {
            Console.WriteLine("[ERROR] config.json is not valid.");
            Console.ReadKey();
            return;
        }

        var firstRun = config["firstRun"] == "true";

        Console.Title = "DeskSwap";
        Utilities.WriteHeader();
        if (!Utilities.IsAdministrator())
        {
            Utilities.RestartAsAdmin();
        }

        if (firstRun)
        {
            Directory.CreateDirectory(Paths.ProfilesPath);

            Console.WriteLine("\t\tHi there! This is your first time using DeskSwap.");
            Console.WriteLine("\t\tTo get started, we'll need to make sure you have everything set up.\n");
            Commands.Setup();
            Console.WriteLine("\t\tYou can use `load` to load this profile later.");
            Console.WriteLine("\t\tClick any key to continue...\n\t\t");
            Console.ReadKey();
            config["firstRun"] = "false";
            File.WriteAllText("config.json", JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }

        Utilities.WriteHeader();
        Console.WriteLine("\t\tWelcome to DeskSwap.");
        Console.WriteLine("\t\tif you are not sure how to start, use `help` (:\n");

        //Main loop
        while (true)
        {
            Console.Write("\t\t> ");

            string input = Console.ReadLine() ?? string.Empty;
            Console.Write("\n");
            Utilities.WriteHeader();
            switch (input)
            {
                case "help":
                    Console.WriteLine("\t\t Commands  | Description");
                    Console.WriteLine("\t\t --------- + ----------------------------------------------");
                    Console.WriteLine("\t\t- dump     | Create temp dump of your current desktop state.");
                    Console.WriteLine("\t\t- restore  | Restore your desktop state from the last temp dump.");
                    Console.WriteLine("\t\t- profile  | Create a new profile based on the current desktop");
                    Console.WriteLine("\t\t- edit     | Edit a profile");
                    Console.WriteLine("\t\t- help     | Show this help message.");
                    Console.WriteLine("\t\t- list     | List all profiles.");
                    Console.WriteLine("\t\t- load     | Load a profile.");
                    Console.WriteLine("\t\t- delete   | Delete a profile.");
                    Console.WriteLine("\t\t- path     | Show the path to the profiles folder.");
                    Console.WriteLine("\t\t- clear    | Clear the console.");
                    Console.WriteLine("\t\t- about    | Show information about DeskSwap.");
                    Console.WriteLine("\t\t- exit     | Exit the program.");
                    Console.Write("\n");
                    break;
                
                case "setup":
                    Commands.Setup();
                    return;

                case "list":
                    Commands.ListProfiles();
                    break;

                case "profile":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name > ");
                    var name = Console.ReadLine() ?? string.Empty;
                    Console.WriteLine("\t\tPlease enter a description for the profile:");
                    Console.Write("\t\tProfile Description> ");
                    var description = Console.ReadLine() ?? string.Empty;
                    Commands.CreateProfile(name, description, true);
                    break;

                case "load":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name> ");
                    var profileName = Console.ReadLine() ?? string.Empty;
                    Commands.LoadProfile(profileName);
                    break;
                
                case "delete":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name> ");
                    var profileNameToDelete = Console.ReadLine() ?? string.Empty;
                    Commands.DeleteProfile(profileNameToDelete);
                    break;
                
                case "edit":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name> ");
                    var profileNameToEdit = Console.ReadLine() ?? string.Empty;
                    Commands.EditProfile(profileNameToEdit);
                    break;

                case "dump":
                    Commands.CreateTempDump();
                    break;

                case "restore":
                    Commands.RestoreTempDump();
                    break;

                case "exit":
                    Console.WriteLine("\t\tThanks for using DeskSwap! Bye!");
                    Thread.Sleep(1000);
                    Environment.Exit(0);
                    break;

                case "clear":
                    Utilities.WriteHeader();
                    break;

                case "path":
                    Process.Start("explorer.exe", Paths.ProfilesPath);
                    break;

                case "about":
                    Console.WriteLine("\t\tThis is a simple tool to help you manage your desktop files.");
                    Console.WriteLine("\t\tYou can create profiles and swap between them\n");
                    Console.WriteLine("\t\tYou can also create a temp profile for a quick swap!\n");
                    Console.WriteLine("\t\tThis is a work in progress and I will be adding more features soon.");
                    Console.WriteLine("\t\tIf you have any suggestions or bugs, please let me know on Github !\n\n");
                    Console.WriteLine("\t\tMade with love in Saudi Arabia <3");
                    break;

                default:
                    Console.WriteLine("\t\tHmmm. This doesn't seem like a valid command.");
                    Console.WriteLine("\t\tYou can use `help` to see all the commands.\n");
                    break;
            }
        }

    }

}
    