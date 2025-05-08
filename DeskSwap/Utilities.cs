using System.Diagnostics;
using System.Security.Principal;

namespace DeskSwap;

public static class Utilities
{
    public static void GetDesktopItems(out string[] folders, out string[] files)
    {
        folders = Directory.GetDirectories(Paths.DesktopPath).Concat(Directory.GetDirectories(Paths.PublicDesktopPath)).ToArray();
        files = Directory.GetFiles(Paths.DesktopPath).Concat(Directory.GetFiles(Paths.PublicDesktopPath)).ToArray();
    }
    
    public static void RestartAsAdmin()
    {
        var exeName = Process.GetCurrentProcess().MainModule.FileName;
        
        var startInfo = new ProcessStartInfo(exeName)
        {
            UseShellExecute = true,
            Verb = "runas" // This triggers the UAC prompt
        };
    
        try
        {
            Process.Start(startInfo);
            Process.GetCurrentProcess().Kill(); // Close the current process
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User refused the UAC prompt
            Console.WriteLine("\t\t [ERROR] Must be run as admin to work properly.");
            Console.WriteLine("\t\t Please restart the program as admin.");
            Console.WriteLine("\t\t [ERROR] Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }

    public static bool IsAdministrator()
    {
        // Check if the current user is an administrator
        
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
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