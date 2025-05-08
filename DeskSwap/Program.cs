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
            Console.WriteLine("\t\t-Make sure you have all of your files, folders and shortcuts on your desktop.");
            Console.WriteLine("\t\tOnce that's done, press any key...\n\n\t\t");
            Console.ReadKey();
            CreateProfile("Default", "Default profile created by DeskSwap", false);
            Utilities.WriteHeader();
            Console.WriteLine("\t\tDone! Your default profile has been created.");
            Console.WriteLine("\t\tYou can use `load` to load this profile later.");
            Console.WriteLine("\t\tClick any key to continue...\n\t\t");
            Console.ReadKey();
            config["firstRun"] = "false";
            File.WriteAllText("config.json",
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
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

                case "list":
                    ListProfiles();
                    break;

                case "profile":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name > ");
                    var name = Console.ReadLine() ?? string.Empty;
                    Console.WriteLine("\t\tPlease enter a description for the profile:");
                    Console.Write("\t\tProfile Description> ");
                    var description = Console.ReadLine() ?? string.Empty;
                    CreateProfile(name, description, true);
                    break;

                case "load":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name> ");
                    var profileName = Console.ReadLine() ?? string.Empty;
                    LoadProfile(profileName);
                    break;

                case "edit":
                    Console.WriteLine("\t\tPlease enter the name of the profile:");
                    Console.Write("\t\tProfile Name> ");
                    var profileNameToEdit = Console.ReadLine() ?? string.Empty;
                    EditProfile(profileNameToEdit);
                    break;

                case "dump":
                    CreateTempDump();
                    break;

                case "restore":
                    RestoreTempDump();
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
                    Console.WriteLine(
                        "\t\tHmmm. This doesn't seem like a valid command\n\t\tYou can always use `help` for help(obviously lol)");
                    break;
            }
        }

    }

    // Commands

    private static void CreateProfile(string name, string description, bool writeTips)
    {
        if (name == "default")
        {
            Console.WriteLine("\t\tSorry, but you can't create a profile with the name 'default'.");
            Console.WriteLine("\t\tIf you want to update the default profile, you can use `setup`.\n");
            return;
        }

        Utilities.GetDesktopItems(out var folders, out var files);
        var profile = new DeskSwapProfile(name, description, folders, files, new Dictionary<string, string>());
        foreach (var folder in folders)
        {
            profile.OriginalPaths.Add(folder, Path.GetDirectoryName(folder) ?? string.Empty);
        }

        foreach (var file in files)
        {
            profile.OriginalPaths.Add(file, Path.GetDirectoryName(file) ?? string.Empty);
        }

        var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
        File.WriteAllText(profilePath, json);
        if (!writeTips) return;
        Console.WriteLine($"\t\tProfile {name} created successfully!");
        Console.WriteLine("\t\tYou can use `load` to load this profile later.");
        Console.WriteLine("\t\tYou can also use `edit` to edit this profile\n");
        Console.WriteLine($"Tip: The profile is saved in {Paths.DocumentsPath} as {name}.json\n\n");
    }

    private static void EditProfile(string name)
    {
        if (name == "default")
        {
            Console.WriteLine("\t\tSorry, but you can't edit the default profile.");
            Console.WriteLine("\t\tIf you want to update the default profile, you can use `setup`.\n");
            return;
        }

        var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
        if (!File.Exists(profilePath))
        {
            Console.WriteLine($"\t\tAh sorry, but the profile {name} does not exist.");
            Console.WriteLine("\t\tMaybe you have misspelled the name or it was deleted?");
            Console.WriteLine("\t\tYou can use `list` to see all the profiles you have.\n\n");
            return;
        }

        var profileData = File.ReadAllText(profilePath);
        var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);
        string[] folders = profileObj?.Folders ?? [];
        string[] files = profileObj?.Files ?? [];
        string[] allItems = folders.Concat(files).ToArray();
        Console.WriteLine($"\t\t===Items in {name}===");
        for (int i = 0; i < allItems.Length; i++)
        {
            Console.WriteLine($"\t\t{i}- {allItems[i]}");
        }

        Console.WriteLine("\t\t===End of items===\n");
        Console.WriteLine("\t\tYou can use the following commands:");
        Console.WriteLine("\t\t- add <path> : Add a new item to the profile");
        Console.WriteLine("\t\t- remove <index> : Remove an item from the profile");
        Console.WriteLine("\t\t- quit : Quit the editor\n");
        while (true)
        {
            Console.Write("\t\t> ");
            var input = Console.ReadLine() ?? string.Empty;
            if (input == "quit")
            {
                break;
            }

            if (input.StartsWith("remove"))
            {
                var index = input.Split(" ")[1];
                if (int.TryParse(index, out int indexInt))
                {
                    if (indexInt < 0 || indexInt >= allItems.Length)
                    {
                        Console.WriteLine("\t\tInvalid index.");
                        continue;
                    }

                    var item = allItems[indexInt];
                    if (folders.Contains(item))
                    {
                        folders = folders.Where(x => x != item).ToArray();
                    }
                    else if (files.Contains(item))
                    {
                        files = files.Where(x => x != item).ToArray();
                    }
                    else
                    {
                        Console.WriteLine("\t\tInvalid item.");
                        continue;
                    }
                }
            }

            if (input.StartsWith("add"))
            {
                var path = input.Split(" ")[1];
                if (!path.StartsWith(Paths.DesktopPath))
                {
                    Console.WriteLine("\t\tInvalid path, it should be on the desktop.");
                    continue;
                }

                if (File.Exists(path))
                {
                    files = files.Append(path).ToArray();
                }
                else if (Directory.Exists(path))
                {
                    folders = folders.Append(path).ToArray();
                }
                else
                {
                    Console.WriteLine("\t\tInvalid path, it should be a file or a folder.");
                }
            }
        }

        var newProfile = new DeskSwapProfile(name, profileObj?.Description ?? "", folders, files,
            profileObj?.OriginalPaths ?? new Dictionary<string, string>());
        var newJson = JsonSerializer.Serialize(newProfile, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(profilePath, newJson);
        Console.WriteLine("\t\tDone! Your profile has been updated.");
        Utilities.WriteHeader();
    }

    private static void ListProfiles()
    {
        var profiles = Directory.GetFiles(Paths.ProfilesPath, "*.json");
        if (profiles.Length == 0)
        {
            Console.WriteLine("\t\tYou don't have any profiles yet.");
            Console.WriteLine("\t\tYou can create a new profile using `profile` command.\n");
            return;
        }

        Console.WriteLine("\t\tHere are your profiles:");
        foreach (var profile in profiles)
        {
            var profileData = File.ReadAllText(profile);
            var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);
            if (profileObj == null)
            {
                Console.WriteLine(
                    $"\t\t- {Path.GetFileNameWithoutExtension(profile)} | [ERROR] Could not load profile.");
                continue;
            }

            Console.WriteLine($"\t\t- {profileObj.Name} : {profileObj.Description}");

        }

        Console.WriteLine("---===============================---");
        Console.WriteLine("\n\t\tTip : You can use `load` to load a profile.\n");
    }

    private static void CreateTempDump()
    {
        Utilities.GetDesktopItems(out var folders, out var files);
        if (!Directory.Exists(Paths.TempDumpPath))
        {
            Directory.CreateDirectory(Paths.TempDumpPath);
        }

        foreach (var folder in folders)
        {
            var folderName = Path.GetFileName(folder);
            var newFolderPath = Path.Combine(Paths.TempDumpPath, folderName);
            Directory.Move(folder, newFolderPath);
        }

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var newFilePath = Path.Combine(Paths.TempDumpPath, fileName);
            File.Move(file, newFilePath);
        }

        Console.WriteLine("\t\tDone! Your desktop is now empty.");
        Console.WriteLine("\t\tYou can use `restore` to restore your desktop state.\n");
    }

    private static void RestoreTempDump()
    {
        if (!Directory.Exists(Paths.TempDumpPath))
        {
            Console.WriteLine("\t\tYou don't have any temp dump yet.");
            Console.WriteLine("\t\tYou can create a new temp dump using `dump` command.\n");
            return;
        }

        var folders = Directory.GetDirectories(Paths.TempDumpPath);
        var files = Directory.GetFiles(Paths.TempDumpPath);
        foreach (var folder in folders)
        {
            var folderName = Path.GetFileName(folder);
            var newFolderPath = Path.Combine(Paths.DesktopPath, folderName);
            Directory.Move(folder, newFolderPath);

        }

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var newFilePath = Path.Combine(Paths.DesktopPath, fileName);
            File.Move(file, newFilePath);
        }

        Directory.Delete(Paths.TempDumpPath, true);
        Console.WriteLine("\t\tDone! Your desktop is now restored.");
    }

    private static void LoadProfile(string name)
    {
        var profilePath = Path.Combine(Paths.ProfilesPath, $"{name}.json");
        var defaultProfilePath = Path.Combine(Paths.ProfilesPath, "Default.json");

        if (!File.Exists(profilePath))
        {
            Console.WriteLine($"\t\tAh sorry, but the profile {name} does not exist.");
            Console.WriteLine("\t\tMaybe you have misspelled the name or it was deleted?");
            Console.WriteLine("\t\tYou can use `list` to see all the profiles you have.\n\n");
            return;
        }

        var profileData = File.ReadAllText(profilePath);
        var profileObj = JsonSerializer.Deserialize<DeskSwapProfile>(profileData);

        var defaultProfileData = File.ReadAllText(defaultProfilePath);
        var defaultProfileObj = JsonSerializer.Deserialize<DeskSwapProfile>(defaultProfileData);

        if (profileObj == null)
        {
            Console.WriteLine("\t\t[ERROR] Profile is found but could not be loaded.");
            Console.WriteLine("\t\tMaybe the file is corrupted or not a valid profile.");
            return;
        }

        // Get current desktop items
        Utilities.GetDesktopItems(out var currentFolders, out var currentFiles);

        var foldersToRemove = currentFolders.Where(f => !profileObj.Folders.Contains(f)).ToArray();
        var filesToRemove = currentFiles.Where(f => !profileObj.Files.Contains(f)).ToArray();

        var foldersToAdd = profileObj.Folders.Where(f => !currentFolders.Contains(f)).ToArray();
        var filesToAdd = profileObj.Files.Where(f => !currentFiles.Contains(f)).ToArray();

        if (!Directory.Exists(Paths.DumpPath))
        {
            Directory.CreateDirectory(Paths.DumpPath);
        }

        foreach (var folder in foldersToAdd)
        {
            var folderName = Path.GetFileName(folder);
            var folderPath = Path.Combine(Paths.DumpPath, folderName);
            var newFolderPath = defaultProfileObj.OriginalPaths[folder];
            try
            {
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.Move(folderPath, newFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t[ERROR] Could not move folder {folderName}: {ex.Message}");
            }
        }

        foreach (var file in filesToAdd)
        {
            var fileName = Path.GetFileName(file);
            Console.WriteLine("[DEBUG] File name: " + fileName);
            var filePath = Path.Combine(Paths.DumpPath, fileName);
            Console.WriteLine("[DEBUG] File path: " + filePath);
            var newFilePath = Path.Combine(defaultProfileObj.OriginalPaths[file], fileName);
            Console.WriteLine("[DEBUG] New file path: " + newFilePath);
            try
            {
                if (!File.Exists(newFilePath))
                {
                    File.Move(filePath, newFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t[ERROR] Could not move file {fileName}: {ex.Message}");
            }
        }


        foreach (var folder in foldersToRemove)
        {
            var folderName = Path.GetFileName(folder);
            var newFolderPath = Path.Combine(Paths.DumpPath, folderName);
            try
            {
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.Move(folder, newFolderPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t[ERROR] Could not move folder {folderName}: {ex.Message}");
            }
        }

        foreach (var file in filesToRemove)
        {
            var fileName = Path.GetFileName(file);
            var newFilePath = Path.Combine(Paths.DumpPath, fileName);
            try
            {
                if (!File.Exists(newFilePath))
                {
                    File.Move(file, newFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\t\t[ERROR] Could not move file {fileName}: {ex.Message}");
            }
        }

        // // Move profile items to desktop
        // foreach (var folder in profileObj.Folders)
        // {
        //     var folderName = Path.GetFileName(folder);
        //     var newFolderPath = Path.Combine(Paths.DesktopPath, folderName);
        //     if (Directory.Exists(newFolderPath))
        //     {
        //         Console.WriteLine($"\t\t[ERROR] Folder {folderName} already exists on the desktop.");
        //         continue;
        //     }
        //     try
        //     {
        //         if (Directory.Exists(folder))
        //         {
        //             Directory.Move(folder, newFolderPath);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"\t\t[ERROR] Could not move folder {folderName}: {ex.Message}");
        //     }
        // }
        //
        // foreach (var file in profileObj.Files)
        // {
        //     var fileName = Path.GetFileName(file);
        //     var newFilePath = Path.Combine(Paths.DesktopPath, fileName);
        //     if (File.Exists(newFilePath))
        //     {
        //         Console.WriteLine($"\t\t[ERROR] File {fileName} already exists on the desktop.");
        //         continue;
        //     }
        //     try
        //     {
        //         if (File.Exists(file))
        //         {
        //             File.Move(file, newFilePath);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"\t\t[ERROR] Could not move file {fileName}: {ex.Message}");
        //     }
    }
}
    