namespace DeskSwap;

public class Paths
{
    // System paths
    public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    public static readonly string PublicDesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
    public static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    
    //DeskSwap paths
    public static string TempDumpPath = Path.Combine(Path.GetTempPath(), "DeskSwap");
    
    public static string DumpPath = Path.Combine(DocumentsPath, "DeskSwap", "Dump");

    public static string ProfilesPath = Path.Combine(DocumentsPath, "DeskSwap", "Profiles");

}