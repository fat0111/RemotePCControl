using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace BuildProjectClient;

internal record ShortcutDefinition(string Name, string ProjectFolder, string ExecutableName);
internal record ServerInfo(string Ip, int Port, DateTime GeneratedAt);

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

            ServerInfo serverInfo = LoadServerInfo(solutionRoot);
            SaveServerInfoCopies(solutionRoot, serverInfo);

            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║    RemotePCControl Client Build & Shortcut Builder   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.WriteLine($"Solution root : {solutionRoot}");
            Console.WriteLine($"Desktop       : {desktopPath}");
            Console.WriteLine($"Server IP     : {serverInfo.Ip}");
            Console.WriteLine($"Server Port   : {serverInfo.Port}");
            Console.WriteLine();

            foreach (var target in Targets)
            {
                Console.WriteLine($"[PROCESSING] {target.Name}...");

                string publishPath = BuildAndPublishProject(solutionRoot, target);
                string exePath = Path.Combine(publishPath, target.ExecutableName);

                if (!File.Exists(exePath))
                {
                    throw new FileNotFoundException($"Không tìm thấy {target.ExecutableName} sau khi publish tại {publishPath}");
                }

                Console.WriteLine($"[FOUND] {exePath}");

                CreateShortcut(repoShortcutFolder, target.Name, exePath);
                CreateShortcut(desktopPath, target.Name, exePath);

                if (target.ProjectFolder.Equals("ClientControlled", StringComparison.OrdinalIgnoreCase))
                {
                    ApplyClientSettings(publishPath, serverInfo);
                    CreateClientPackage(solutionRoot, publishPath);
                }

                Console.WriteLine();
            }

            Console.WriteLine("[DONE] All projects have been built, published and shortcuts created!");
            Console.WriteLine($"       • Shortcuts at: {repoShortcutFolder}");
            Console.WriteLine($"       • Shortcuts at: {desktopPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    private static ServerInfo LoadServerInfo(string solutionRoot)
    {
        string infoPath = Path.Combine(solutionRoot, "server-info.json");
        if (File.Exists(infoPath))
        {
            try
            {
                string json = File.ReadAllText(infoPath);
                var info = JsonSerializer.Deserialize<ServerInfo>(json);
                if (info != null && !string.IsNullOrWhiteSpace(info.Ip))
                {
                    return info;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Cannot parse server-info.json: {ex.Message}");
            }
        }

        string fallbackIp = GetLocalIpAddress();
        Console.WriteLine($"[WARN] server-info.json missing. Using local IP {fallbackIp}:8888");
        return new ServerInfo(fallbackIp, 8888, DateTime.Now);
    }

    private static void SaveServerInfoCopies(string solutionRoot, ServerInfo info)
    {
        string rootFile = Path.Combine(solutionRoot, "server-info.json");
        string webFile = Path.Combine(solutionRoot, "WebInterface", "wwwroot", "server-info.json");
        string json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(rootFile, json);
        Directory.CreateDirectory(Path.GetDirectoryName(webFile)!);
        File.WriteAllText(webFile, json);
        Console.WriteLine($"[INFO] Server info copied to {rootFile} and web root");
    }

    private static void ApplyClientSettings(string publishPath, ServerInfo info)
    {
        try
        {
            string settingsPath = Path.Combine(publishPath, "clientsettings.json");
            var settings = new
            {
                ServerIp = info.Ip,
                ServerPort = info.Port
            };
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
            Console.WriteLine($"  ✓ clientsettings.json updated with server {info.Ip}:{info.Port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Failed to update clientsettings.json: {ex.Message}");
        }
    }

    private static void CreateClientPackage(string solutionRoot, string publishPath)
    {
        try
        {
            string downloadsFolder = Path.Combine(solutionRoot, "WebInterface", "wwwroot", "downloads");
            Directory.CreateDirectory(downloadsFolder);

            string zipPath = Path.Combine(downloadsFolder, "client-controlled.zip");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(publishPath, zipPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            Console.WriteLine($"  ✓ Client package generated at {zipPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Failed to create client package zip: {ex.Message}");
        }
    }

    private static string GetLocalIpAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211 &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
            {
                continue;
            }

            var props = ni.GetIPProperties();
            if (props.GatewayAddresses.Count == 0)
                continue;

            foreach (var addr in props.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(addr.Address))
                {
                    return addr.Address.ToString();
                }
            }
        }

        return "";
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

        throw new InvalidOperationException("Cannot find RemotePCControl.sln file when searching up.");
    }

    private static string BuildAndPublishProject(string solutionRoot, ShortcutDefinition definition)
    {
        string projectPath = Path.Combine(solutionRoot, definition.ProjectFolder);
        string publishPath = Path.Combine(projectPath, "bin", "publish");

        Console.WriteLine($"  → Building project: {definition.ProjectFolder}");

        string? csprojFile = Directory.GetFiles(projectPath, "*.csproj").FirstOrDefault();
        if (csprojFile == null)
        {
            throw new FileNotFoundException($"Cannot find .csproj file in {projectPath}");
        }

        if (Directory.Exists(publishPath))
        {
            Directory.Delete(publishPath, true);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish \"{csprojFile}\" -c Release -o \"{publishPath}\" --self-contained false",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Cannot start dotnet process");
        }

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            Console.WriteLine(output);
            Console.WriteLine(error);
            throw new InvalidOperationException($"Build/Publish failed for {definition.ProjectFolder} (Exit code: {process.ExitCode})");
        }

        Console.WriteLine($"  ✓ Published to: {publishPath}");
        return publishPath;
    }

    private static void CreateShortcut(string folder, string shortcutName, string targetPath)
    {
        string shortcutPath = Path.Combine(folder, $"{shortcutName}.lnk");

        var shellType = Type.GetTypeFromProgID("WScript.Shell")
            ?? throw new InvalidOperationException("Cannot create WScript.Shell (COM). Ensure running on Windows.");

        dynamic shell = Activator.CreateInstance(shellType)
            ?? throw new InvalidOperationException("Cannot create WScript.Shell instance.");

        dynamic shortcut = shell.CreateShortcut(shortcutPath);
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
        shortcut.IconLocation = targetPath;
        shortcut.WindowStyle = 1;
        shortcut.Save();

        Console.WriteLine($"  [SHORTCUT] {shortcutPath}");
    }
}

