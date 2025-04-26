using System.Text.Json;

namespace DeskSwap;

public class Program
{
    // System paths
    public static string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public static string PublicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
    public static string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    public static string TempDumpPath = Path.Combine(Path.GetTempPath(), "DeskSwap");

    public static string ProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeskSwap", "Profiles");

    public static void Main(string[] args)
    {
        WriteHeader();
        Console.WriteLine("\t\tWelcome to DeskSwap.");
        Console.WriteLine("\t\tif you are not sure how to start, use `help` (:\n");

        //Main loop
        while (true)
        {
            Console.Write("\t\t> ");

            string input = Console.ReadLine();
            Console.Write("\n");
            WriteHeader();
            switch (input)
            {
                case "help":
                    Console.WriteLine("\t\t Commands  | Description");
                    Console.WriteLine("\t\t --------- + ----------------------------------------------");
                    Console.WriteLine("\t\t1. dump    | Create temp dump of your current desktop state.");
                    Console.WriteLine("\t\t2. restore | Restore your desktop state from the last temp dump.");
                    Console.WriteLine("\t\t3. profile | Create a new profile based on the current desktop");
                    Console.WriteLine("\t\t4. help    | Show this help message.");
                    Console.WriteLine("\t\t5. clear   | Clear the console.");
                    Console.WriteLine("\t\t6. about   | About DeskSwap.");
                    Console.WriteLine("\t\t7. list    | List all profiles.");
                    Console.WriteLine("\t\t8. load    | Load a profile.");
                    Console.WriteLine("\t\t9. delete  | Delete a profile.");
                    Console.WriteLine("\t\t10.exit    | Exit the program.");
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
        Console.WriteLine("\t\t Profile    | Description");
        Console.WriteLine("\t\t ---------- + ---------------------------------------------");
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
        var profilePath = Path.Combine(ProfilesPath, name);
        if (Directory.Exists(profilePath))
        {
            Console.WriteLine("\t\tProfile already exists. Please choose a different name.");
            return;
        }
        Directory.CreateDirectory(profilePath);
        string[] files = Directory.GetFiles(DesktopPath);
        string[] publicFiles = Directory.GetFiles(PublicDesktopPath);
        string[] folders = Directory.GetDirectories(DesktopPath);
        string[] publicFolders = Directory.GetDirectories(PublicDesktopPath);

        string [] allFiles = files.Concat(publicFiles).ToArray();
        string[] allFolders = folders.Concat(publicFolders).ToArray();

        Dictionary<string, string> originalPaths = new Dictionary<string, string>();

        if(allFiles.Contains($"_{name}.json"))
        {
            allFiles = allFiles.Where(x => x != $"_{name}.json").ToArray();
        }
        foreach (string file in allFiles)
        {
            var fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(profilePath, fileName);
            File.Move(filePath, tempPath);
            originalPaths.Add(fileName, filePath);
        }
        foreach (string folder in allFolders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string folderPath = folderInfo.FullName;
            string tempPath = Path.Combine(profilePath, folderName);
            Directory.Move(folderPath, tempPath);
        }
        var profile = new DeskSwapProfile(name, description, allFolders, allFiles, originalPaths);
        string profileJsonPath = Path.Combine(profilePath, $"_{name}.json");
        string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(profileJsonPath, json);
        Console.WriteLine($"\t\tProfile {name} created successfully!");
        Console.WriteLine($"\t\tTip:You can now use `load {name}` to load this profile.\n");
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
            string orginalPath = originalPaths[fileName];
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(profilePath, fileName);
            File.Move(tempPath, filePath);
        }
        foreach (string folder in folders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string orginalPath = originalPaths[folderName];
            string folderPath = folderInfo.FullName;
            string tempPath = Path.Combine(profilePath, folderName);
            Directory.Move(tempPath, folderPath);
        }
        profile.OriginalPaths.Clear();
        profile.Files = Array.Empty<string>();
        profile.Folders = Array.Empty<string>();
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
        string[] files = profile.Files;
        string[] folders = profile.Folders;
        Dictionary<string, string> originalPaths = profile.OriginalPaths;

        if(files.Contains("_tempdump.json"))
        {
            files = files.Where(x => x != "_tempdump.json").ToArray();
        }

        foreach (string file in files)
        {
            var fileInfo = new FileInfo(file);
            string fileName = fileInfo.Name;
            string orginalPath = originalPaths[fileName];
            string filePath = fileInfo.FullName;
            string tempPath = Path.Combine(TempDumpPath, fileName);
            File.Move(tempPath, filePath);
        }
        foreach (string folder in folders)
        {
            var folderInfo = new DirectoryInfo(folder);
            string folderName = folderInfo.Name;
            string orginalPath = originalPaths[folderName];
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