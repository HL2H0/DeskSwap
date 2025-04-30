using System.Text.Json;

namespace DeskSwap;

public static class Program
{
    // System paths
    public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public static readonly string PublicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
    public static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    
    //DeskSwap paths
    public static string TempDumpPath = Path.Combine(Path.GetTempPath(), "DeskSwap");

    public static string ProfilesPath = Path.Combine(DocumentsPath, "DeskSwap", "Profiles");

    public static void Main(string[] args)
    {
        WriteHeader();
        Console.WriteLine("\t\tWelcome to DeskSwap.");
        Console.WriteLine("\t\tif you are not sure how to start, use `help` (:\n");

        //Main loop
        while (true)
        {
            Console.Write("\t\t> ");

            string input = Console.ReadLine() ?? string.Empty;
            Console.Write("\n");
            WriteHeader();
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
                    Console.WriteLine("\t\t- about    | About DeskSwap.");
                    Console.WriteLine("\t\t- list     | List all profiles.");
                    Console.WriteLine("\t\t- load     | Load a profile.");
                    Console.WriteLine("\t\t- delete   | Delete a profile.");
                    Console.WriteLine("\t\t- clear    | Clear the console.");
                    Console.WriteLine("\t\t- exit     | Exit the program.");
                    Console.Write("\n");
                    break;

                case "list":
                    ListProfiles();
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
                    WriteHeader(); 
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
                    Console.WriteLine("\t\tHmmmm. This doesn't seem like a valid command\n\t\tYou can always use `help` for help(obviously lol)");
                    break;
            }
        }

    }

    // Commands

    public static void ListProfiles()
    {
        Console.WriteLine("\t\tAvailable profiles:");
        if (!Directory.Exists(ProfilesPath))
        {
            Console.WriteLine("\t\tNo profiles found.");
            return;
        }
        string[] profiles = Directory.GetDirectories(ProfilesPath);
        if (profiles.Length == 0)
        {
            Console.WriteLine("\t\tNo profiles found.");
            return;
        }   
        Console.WriteLine("\t\t Profile       | Description");
        Console.WriteLine("\t\t ------------- + ---------------------------------------------");
        foreach (string profile in profiles)
        {
            string profileName = Path.GetFileName(profile);
            string profileJsonPath = Path.Combine(profile, $"_{profileName}.json");
            if (!File.Exists(profileJsonPath))
                continue;
            var profileData = JsonSerializer.Deserialize<DeskSwapProfile>(File.ReadAllText(profileJsonPath));
            if (profileData == null)
                continue;
            Console.WriteLine($"\t\t- {profileName} | {profileData.Description}");
        }
        Console.WriteLine("\n");
    }

    public static void CreateProfile(string name, string description = "None")
    {
        var profilePath = Path.Combine(ProfilesPath, name, $"{name}.json");
        if (File.Exists(profilePath))
        {
            Console.WriteLine("\t\tProfile already exists. Please check the name and try again.");
            return;
        }
        
        string[] files = Directory.GetFiles(DesktopPath);
        string[] publicFiles = Directory.GetFiles(PublicDesktopPath);
        string[] folders = Directory.GetDirectories(DesktopPath);
        string[] publicFolders = Directory.GetDirectories(PublicDesktopPath);
        
        string[] allFiles = files.Concat(publicFiles).ToArray();
        Dictionary<string, bool> allFilesDict = new Dictionary<string, bool>();
        foreach (string file in allFiles)
        {
            var fileInfo = new FileInfo(file);
            string filePath = fileInfo.FullName;
            allFilesDict.Add(filePath, true);
        }
        string[] allFolders = folders.Concat(publicFolders).ToArray();
        Dictionary<string, bool> allFoldersDict = new Dictionary<string, bool>();
        foreach (string folder in allFolders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderPath = folderInfo.FullName;
            allFoldersDict.Add(folderPath, true);
        }
        
        var profile = new DeskSwapProfile(name, description, allFoldersDict ,allFilesDict, new Dictionary<string, string>());
        string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(profilePath, json);
        Console.WriteLine($"\t\tProfile {name} created successfully!");
        Console.WriteLine($"\t\tTip: You can now use `edit` to customize this profile\n");
        


        // Directory.CreateDirectory(profilePath);
        // string[] files = Directory.GetFiles(DesktopPath);
        // string[] publicFiles = Directory.GetFiles(PublicDesktopPath);
        // string[] folders = Directory.GetDirectories(DesktopPath);
        // string[] publicFolders = Directory.GetDirectories(PublicDesktopPath);
        //
        // string [] allFiles = files.Concat(publicFiles).ToArray();
        // string[] allFolders = folders.Concat(publicFolders).ToArray();
        //
        // Dictionary<string, string> originalPaths = new Dictionary<string, string>();
        //
        // if(allFiles.Contains($"_{name}.json"))
        // {
        //     allFiles = allFiles.Where(x => x != $"_{name}.json").ToArray();
        // }
        // foreach (string file in allFiles)
        // {
        //     var fileInfo = new FileInfo(file);
        //     string fileName = fileInfo.Name;
        //     string filePath = fileInfo.FullName;
        //     string tempPath = Path.Combine(profilePath, fileName);
        //     File.Move(filePath, tempPath);
        //     originalPaths.Add(fileName, filePath);
        // }
        // foreach (string folder in allFolders)
        // {
        //     var folderInfo = new DirectoryInfo(folder);
        //     string folderName = folderInfo.Name;
        //     string folderPath = folderInfo.FullName;
        //     string tempPath = Path.Combine(profilePath, folderName);
        //     Directory.Move(folderPath, tempPath);
        // }
        // var profile = new DeskSwapProfile(name, description, allFolders, allFiles, originalPaths);
        // string profileJsonPath = Path.Combine(profilePath, $"_{name}.json");
        // string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        // File.WriteAllText(profileJsonPath, json);
        // Console.WriteLine($"\t\tProfile {name} created successfully!");
        // Console.WriteLine($"\t\tTip:You can now use `load {name}` to load this profile.\n");
    }

    public static void EditProfile(string name)
    {
        var profilePath = Path.Combine(ProfilesPath, name);
        if (!Directory.Exists(profilePath))
        {
            Console.WriteLine("\t\tProfile not found. Please check the name and try again.");
            return;
        }
        string profileJsonPath = Path.Combine(profilePath, $"{name}.json");
        if (!File.Exists(profileJsonPath))
        {
            Console.WriteLine("\t\tProfile not found. Please check the name and try again.");
            return;
        }
        Console.WriteLine("Quick Note: This will open the profile in your default text editor.");
        Console.WriteLine("You'll see the item's name and if it used or not.");
        Console.WriteLine("You can edit the file and save it to apply the changes.\n");
        Console.WriteLine("So, are you ready to edit, Click any key to continue...");
        Console.ReadKey();
        Console.WriteLine("\t\tEditing profile...");
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = profileJsonPath,
            UseShellExecute = true
        });
        
        // var profilePath = Path.Combine(ProfilesPath, name, $"{name}.json"); 
        // if (!File.Exists(profilePath))
        //     return;
        // string profileJsonPath = Path.Combine(profilePath, $"{name}.json");
        // var profile = JsonSerializer.Deserialize<DeskSwapProfile>(File.ReadAllText(profileJsonPath));
        // if(profile == null)
        //     return;
        // string[] allItems = profile.Folders.Concat(profile.Files).ToArray();
        // Console.WriteLine($"\t\tEditing profile {name}...");
        // Console.WriteLine("\t\tID  | Item");
        // for (var i = 0; i < allItems.Length; i++)
        // {
        //     Console.WriteLine($"{i} | {allItems[i]}");
        // }
        //
        // while (true)
        // {
        //     Console.WriteLine("\t\t Now, enter the ID of the item you want to remove, or `done` to finish editing:");
        //     Console.Write("\t\t> ");
        //     string input = Console.ReadLine() ?? string.Empty;
        //     if (input == "done")
        //         break;
        //     if (int.TryParse(input, out int id))
        //     {
        //         if (id < 0 || id >= allItems.Length)
        //         {
        //             Console.WriteLine("\t\tInvalid ID. Please try again.");
        //             continue;
        //         }
        //         string item = allItems[id];
        //         if (profile.Files.Contains(item))
        //         {
        //             profile.Files = profile.Files.Where(x => x != item).ToArray();
        //         }
        //         else if (profile.Folders.Contains(item))
        //         {
        //             profile.Folders = profile.Folders.Where(x => x != item).ToArray();
        //         }
        //         else
        //         {
        //             Console.WriteLine("\t\tItem not found in profile.");
        //             continue;
        //         }
        //         Console.WriteLine($"\t\tItem {item} removed from profile {name}.");
        //     }
        //     else
        //     {
        //         Console.WriteLine("\t\tInvalid input. Please enter a valid ID or `done`.");
        //     }
        // }
        // Console.WriteLine("\t\tSaving profile...");
        // string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        // File.WriteAllText(profileJsonPath, json);
        // WriteHeader();
        // Console.WriteLine($"\t\tProfile {name} edited successfully!");
        // Console.WriteLine("\t\tTip: You can always `load default` to restore your desktop!\n");
    }

    public static void LoadProfile(string name)
    {
        var profilePath = Path.Combine(ProfilesPath, name);
        if (!Directory.Exists(profilePath))
        {
            Console.WriteLine("\t\tProfile not found. Please check the name and try again.");
            return;
        }
        string profileJsonPath = Path.Combine(profilePath, $"{name}.json");
        var profile = JsonSerializer.Deserialize<DeskSwapProfile>(File.ReadAllText(profileJsonPath));
        if(profile == null)
        {
            Console.WriteLine("\t\tSeems like something is wrong.\nCouldn't read dump JSON file");
            return;
        }
        Console.WriteLine("\t\tLoading profile...");
        string[] files = profile.Files;
        string[] folders = profile.Folders;
        Dictionary<string, string> originalPaths = profile.OriginalPaths;

        foreach (string file in files)
        {
            var fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            string originalPath = originalPaths[fileName];
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(profilePath, fileName);
            File.Move(tempPath, filePath);
        }
        foreach (string folder in folders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string originalPath = originalPaths[folderName];
            string folderPath = folderInfo.FullName;
            string tempPath = Path.Combine(profilePath, folderName);
            Directory.Move(tempPath, folderPath);
        }
        
        File.WriteAllText(profileJsonPath, JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"\t\tProfile {name} loaded successfully!");
        Console.WriteLine("\t\tTip: You can always `load default` to restore your desktop!\n");
    }

    public static void CreateTempDump()
    {
        if (!Directory.Exists(TempDumpPath))
            Directory.CreateDirectory(TempDumpPath);

        if(Directory.GetFiles(TempDumpPath).Length > 0)
        {
            Console.WriteLine("\t\tYou already have a temp dump. Do you want to overwrite it? (y/n)");
            string input = Console.ReadLine();
            if (input != "y")
                return;
        }

        Console.WriteLine("\t\tCreating temp dump...");
        string[] files = Directory.GetFiles(DesktopPath);
        string[] publicFiles = Directory.GetFiles(PublicDesktopPath);
        string[] folders = Directory.GetDirectories(DesktopPath);
        string[] publicFolders = Directory.GetDirectories(PublicDesktopPath);

        Dictionary<string, string> originalPaths = new Dictionary<string, string>();

        string[] allFiles = files.Concat(publicFiles).ToArray();
        string[] allFolders = folders.Concat(publicFolders).ToArray();

        if(allFiles.Contains("_tempdump.json"))
        {
            allFiles = allFiles.Where(x => x != "_tempdump.json").ToArray();
        }

        foreach (string file in allFiles)
        {
            var fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(TempDumpPath, fileName);
            File.Move(filePath, tempPath);
            originalPaths.Add(fileName, filePath);
        }
        foreach (string folder in allFolders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string folderPath = folderInfo.FullName;
            string tempPath = Path.Combine(TempDumpPath, folderName);
            Directory.Move(folderPath, tempPath);
            originalPaths.Add(folderName, folderPath);
        }
        
        var tempDump = new DeskSwapProfile("TempDump", "Temp dump auto created by DeskSwap", allFolders, allFiles, originalPaths);
        string tempDumpPath = Path.Combine(TempDumpPath, "_tempdump.json");
        string json = JsonSerializer.Serialize(tempDump, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(tempDumpPath, json);

        Console.WriteLine("\t\tTemp dump created successfully!");
        Console.WriteLine("\t\t Tip: You can use `restore` to restore your desktop state from the last temp dump.\n");
    }

    public static void RestoreTempDump()
    {
        if (!Directory.Exists(TempDumpPath) || (Directory.GetFiles(TempDumpPath).Length < 0))
        {
            Console.WriteLine("\t\tNo temp dump found. Please create a temp dump first.");
            return;
        }
        string tempDumpJsonPath = Path.Combine(TempDumpPath, "_tempdump.json");
        var profile = JsonSerializer.Deserialize<DeskSwapProfile>(File.ReadAllText(tempDumpJsonPath));
        if(profile == null)
        {
            Console.WriteLine("\t\tSeems like something is wrong.\nCouldn't read dump JSON file");
            return;
        }

        Console.WriteLine("\t\tRestoring temp dump...");
        string[] files = profile.Files.Keys.ToArray();
        string[] folders = profile.Folders.Keys.ToArray();
        Dictionary<string, string> originalPaths = profile.OriginalPaths;

        if(files.Contains("_tempdump.json"))
        {
            files = files.Where(x => x != "_tempdump.json").ToArray();
        }

        foreach (string file in files)
        {
            var fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            string originalPath = originalPaths[fileName];
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(TempDumpPath, fileName);
            File.Move(tempPath, filePath);
        }
        foreach (string folder in folders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string originalPath = originalPaths[folderName];
            string folderPath = folderInfo.FullName;
            string tempPath = Path.Combine(TempDumpPath, folderName);
            Directory.Move(tempPath, folderPath);
        }

        Directory.Delete(TempDumpPath, true);
        Console.WriteLine("\t\tTemp dump restored successfully!");
        Console.WriteLine("\t\t Tip: You can use `dump` to create a new temp dump.\n");
    }

    public static void WriteHeader()
    {
        Console.Clear();
        
        Console.WriteLine("\n");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("██████╗ ███████╗███████╗██╗  ██╗  ███████╗██╗    ██╗ █████╗ ██████╗ ");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("██╔══██╗██╔════╝██╔════╝██║ ██╔╝  ██╔════╝██║    ██║██╔══██╗██╔══██╗");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("██║  ██║█████╗  ███████╗█████╔╝   ███████╗██║ █╗ ██║███████║██████╔╝");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("██║  ██║██╔══╝  ╚════██║██╔═██╗   ╚════██║██║███╗██║██╔══██║██╔═══╝ ");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("██████╔╝███████╗███████║██║  ██╗  ███████║╚███╔███╔╝██║  ██║██║     ");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("╚═════╝ ╚══════╝╚══════╝╚═╝  ╚═╝  ╚══════╝ ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝     ");
        Console.SetCursorPosition((Console.WindowWidth - 65) / 2, Console.CursorTop);
        Console.WriteLine("Version 1.0.0 ==================================== DeskSwap by HL2H0\n\n\n");
        
    }
}