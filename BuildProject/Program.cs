using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuildProject;

internal record ShortcutDefinition(string Name, string ProjectFolder, string ExecutableName);

internal static class Program
{
    private static readonly ShortcutDefinition[] Targets =
    [
        new ShortcutDefinition("RemotePCControl - Server", "Server", "Server.exe"),
        new ShortcutDefinition("RemotePCControl - Client", "ClientControlled", "ClientControlled.exe"),
        new ShortcutDefinition("RemotePCControl - WebInterface", "WebInterface", "WebInterface.exe"),
    ];

    private static void Main()
    {
        try
        {
            string solutionRoot = FindSolutionRoot();
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string repoShortcutFolder = Path.Combine(solutionRoot, "Shortcuts");

            Directory.CreateDirectory(repoShortcutFolder);

            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║        RemotePCControl Shortcut Builder              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.WriteLine($"Solution root : {solutionRoot}");
            Console.WriteLine($"Desktop       : {desktopPath}");
            Console.WriteLine();

            foreach (var target in Targets)
            {
                string exePath = LocateExecutable(solutionRoot, target);
                Console.WriteLine($"[FOUND] {target.Name} => {exePath}");

                CreateShortcut(repoShortcutFolder, target.Name, exePath);
                CreateShortcut(desktopPath, target.Name, exePath);
            }

            Console.WriteLine();
            Console.WriteLine("[DONE] Shortcuts created both in:");
            Console.WriteLine($"       • {repoShortcutFolder}");
            Console.WriteLine($"       • {desktopPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static string FindSolutionRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "RemotePCControl.sln")))
            {
                return current.FullName;
            }
            current = current.Parent;
        }

        throw new InvalidOperationException("Không tìm thấy file RemotePCControl.sln khi dò lên trên.");
    }

    private static string LocateExecutable(string solutionRoot, ShortcutDefinition definition)
    {
        string binFolder = Path.Combine(solutionRoot, definition.ProjectFolder, "bin");
        if (!Directory.Exists(binFolder))
        {
            throw new FileNotFoundException($"Chưa build project {definition.ProjectFolder}. Không thấy thư mục bin.");
        }

        string? exePath = Directory.EnumerateFiles(binFolder, definition.ExecutableName, SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (exePath == null)
        {
            throw new FileNotFoundException($"Không tìm thấy {definition.ExecutableName} trong {binFolder}. Hãy build project tương ứng trước.");
        }

        return exePath;
    }

    private static void CreateShortcut(string folder, string shortcutName, string targetPath)
    {
        string shortcutPath = Path.Combine(folder, $"{shortcutName}.lnk");

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Không thể khởi tạo WScript.Shell (COM). Đảm bảo chạy trên Windows.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Không thể tạo instance WScript.Shell.");

        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
        shortcut.IconLocation = targetPath;
        shortcut.WindowStyle = 1;
        shortcut.Save();

        Console.WriteLine($"[CREATE] {shortcutPath}");
    }
}


