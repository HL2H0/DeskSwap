namespace DeskSwap;

public class Program
{
    public static void Main(string[] args)
    {
        string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string DumpPath = Path.Combine(DesktopPath, "DeskSwapDump");

        string[] files = Directory.GetFiles(DesktopPath, "*.*", SearchOption.TopDirectoryOnly);
        string[] folders = Directory.GetDirectories(DesktopPath, "*", SearchOption.TopDirectoryOnly);
        string[] shortcuts = Directory.GetFiles(DesktopPath, "*.lnk", SearchOption.TopDirectoryOnly);
        string[] all = files.Concat(folders).Concat(shortcuts).ToArray();
        
        if (!Directory.Exists(DumpPath))
        {
            Directory.CreateDirectory(DumpPath);
        }
        foreach (string item in all)
        {
            var path = Path.Combine(DumpPath, Path.GetFileName(item));
            if (Path.GetFileName(item) == "DeskSwapDump")
            {
                Console.WriteLine($"Skipping {item} as it is the dump folder itself.");
                continue;
            }
            if (File.Exists(path) || Directory.Exists(path))
            {
                Console.WriteLine($"Skipping {item} as it already exists in the dump folder.");
                continue;
            }
            if (File.Exists(item))
            {
                File.Move(item, path);
            }
            else if (Directory.Exists(item))
            {
                Directory.Move(item, path);
            }
            Console.WriteLine(item);
        }
        Console.ReadKey();
    }
}