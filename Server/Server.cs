using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace RemotePCControl
{
    public class Server
    {
        private TcpListener listener;
        private List<ConnectedClient> clients = new List<ConnectedClient>();
        private Dictionary<string, ClientSession> sessions = new Dictionary<string, ClientSession>();
        private readonly object lockObj = new object();
        private bool isRunning = false;
        private Dictionary<string, StreamStats> streamStats = new Dictionary<string, StreamStats>();

        public void Start(int port = 8888)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                isRunning = true;

                Console.WriteLine($"[SERVER] Started on port {port}");
                Console.WriteLine($"[SERVER] Waiting for connections...");

                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.IsBackground = true;
                acceptThread.Start();

                Thread statsThread = new Thread(DisplayStats);
                statsThread.IsBackground = true;
                statsThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to start server: {ex.Message}");
            }
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    if (isRunning)
                        Console.WriteLine($"[ERROR] Accept client failed: {ex.Message}");
                }
            }
        }

        private void HandleClient(TcpClient tcpClient)
        {
            ConnectedClient client = new ConnectedClient(tcpClient);
            lock (lockObj) { clients.Add(client); }
            Console.WriteLine($"[SERVER] New client connected from {client.RemoteEndPoint}");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                byte[] lengthBuffer = new byte[4];

                while (tcpClient.Connected)
                {
                    int bytesRead = 0;
                    while (bytesRead < 4)
                    {
                        int read = stream.Read(lengthBuffer, bytesRead, 4 - bytesRead);
                        if (read == 0) throw new Exception("Client disconnected");
                        bytesRead += read;
                    }

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    byte[] messageBuffer = new byte[messageLength];
                    bytesRead = 0;
                    while (bytesRead < messageLength)
                    {
                        int read = stream.Read(messageBuffer, bytesRead, messageLength - bytesRead);
                        if (read == 0) throw new Exception("Client disconnected");
                        bytesRead += read;
                    }

                    string message = Encoding.UTF8.GetString(messageBuffer, 0, messageLength);

                    if (message.StartsWith("RESPONSE") && message.Contains("WEBCAM_FRAME"))
                    {
                        ProcessMessage(client, message, stream, true);
                    }
                    else
                    {
                        Console.WriteLine($"[RECEIVED] {message.Substring(0, Math.Min(message.Length, 100))}");
                        ProcessMessage(client, message, stream, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Client handler: {ex.Message}");
            }
            finally
            {
                lock (lockObj)
                {
                    clients.Remove(client);
                    var sessionToRemove = sessions.FirstOrDefault(s => s.Value.ControlledClient == client);
                    if (!sessionToRemove.Equals(default(KeyValuePair<string, ClientSession>)))
                    {
                        string ip = sessionToRemove.Key;
                        sessions.Remove(ip);
                        streamStats.Remove(ip);
                        Console.WriteLine($"[SESSION] Removed session for {ip}");
                    }
                }
                tcpClient.Close();
                Console.WriteLine($"[SERVER] Client disconnected");
            }
        }

        private void ProcessMessage(ConnectedClient client, string message, NetworkStream stream, bool isWebcamFrame)
        {
            try
            {
                string[] parts = message.Split('|');
                string command = parts[0];

                switch (command)
                {
                    case "REGISTER_CONTROLLED":
                        if (parts.Length >= 3)
                        {
                            string ip = parts[1];
                            string password = parts[2];

                            lock (lockObj)
                            {
                                sessions[ip] = new ClientSession
                                {
                                    Password = password,
                                    ControlledClient = client
                                };

                                streamStats[ip] = new StreamStats
                                {
                                    IP = ip,
                                    IsStreaming = false
                                };
                            }

                            SendResponse(stream, "REGISTERED|SUCCESS");
                            Console.WriteLine($"[SESSION] Registered controlled PC: {ip} with password: {password}");
                        }
                        break;

                    case "LOGIN":
                        if (parts.Length >= 3)
                        {
                            string ip = parts[1];
                            string password = parts[2];

                            lock (lockObj)
                            {
                                if (sessions.ContainsKey(ip) && sessions[ip].Password == password)
                                {
                                    sessions[ip].ControllerClient = client;
                                    SendResponse(stream, "LOGIN|SUCCESS");
                                    Console.WriteLine($"[SESSION] Controller logged in to {ip}");
                                }
                                else
                                {
                                    SendResponse(stream, "LOGIN|FAILED");
                                    Console.WriteLine($"[SESSION] Login failed for {ip}");
                                }
                            }
                        }
                        break;

                    case "COMMAND":
                        if (parts.Length >= 3)
                        {
                            string targetIp = parts[1];
                            string actualCommand = parts[2];
                            string parameters = parts.Length > 3 ? string.Join("|", parts.Skip(3)) : "";

                            lock (lockObj)
                            {
                                if (sessions.ContainsKey(targetIp) && sessions[targetIp].ControlledClient != null)
                                {
                                    var targetClient = sessions[targetIp].ControlledClient;
                                    ForwardCommand(targetClient, actualCommand, parameters);

                                    if (actualCommand == "WEBCAM_STREAM_START" && streamStats.ContainsKey(targetIp))
                                    {
                                        streamStats[targetIp].IsStreaming = true;
                                        streamStats[targetIp].StartTime = DateTime.Now;
                                        streamStats[targetIp].FramesForwarded = 0;
                                        Console.WriteLine($"[WEBCAM] Stream started for {targetIp}");
                                    }
                                    else if (actualCommand == "WEBCAM_STREAM_STOP" && streamStats.ContainsKey(targetIp))
                                    {
                                        streamStats[targetIp].IsStreaming = false;
                                        Console.WriteLine($"[WEBCAM] Stream stopped for {targetIp}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"[FORWARD] Command {actualCommand} forwarded to {targetIp}");
                                    }
                                }
                            }
                        }
                        break;

                    case "RESPONSE":
                        if (parts.Length >= 3)
                        {
                            string sourceIp = parts[1];
                            string responseType = parts[2];

                            lock (lockObj)
                            {
                                if (sessions.ContainsKey(sourceIp) && sessions[sourceIp].ControllerClient != null)
                                {
                                    var controllerClient = sessions[sourceIp].ControllerClient;
                                    string data = string.Join("|", parts.Skip(2));
                                    SendResponse(controllerClient.TcpClient.GetStream(), $"RESPONSE|{data}");

                                    if (responseType == "WEBCAM_FRAME" && streamStats.ContainsKey(sourceIp))
                                    {
                                        streamStats[sourceIp].FramesForwarded++;
                                        streamStats[sourceIp].LastFrameTime = DateTime.Now;
                                    }
                                    else if (!isWebcamFrame)
                                    {
                                        Console.WriteLine($"[FORWARD] Response {responseType} forwarded from {sourceIp}");
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Process message: {ex.Message}");
            }
        }

        private void ForwardCommand(ConnectedClient targetClient, string command, string parameters)
        {
            try
            {
                NetworkStream stream = targetClient.TcpClient.GetStream();
                string message = $"EXECUTE|{command}|{parameters}";

                byte[] data = Encoding.UTF8.GetBytes(message);
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Forward command: {ex.Message}");
            }
        }

        private void SendResponse(NetworkStream stream, string response)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(response);
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Send response: {ex.Message}");
            }
        }

        private void DisplayStats()
        {
            while (isRunning)
            {
                Thread.Sleep(5000);

                lock (lockObj)
                {
                    var activeStreams = streamStats.Where(s => s.Value.IsStreaming).ToList();

                    if (activeStreams.Any())
                    {
                        Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                        Console.WriteLine("║                    WEBCAM STREAM STATS                      ║");
                        Console.WriteLine("╠════════════════════════════════════════════════════════════╣");

                        foreach (var kvp in activeStreams)
                        {
                            var stats = kvp.Value;
                            var elapsed = (DateTime.Now - stats.StartTime).TotalSeconds;
                            var fps = elapsed > 0 ? stats.FramesForwarded / elapsed : 0;

                            Console.WriteLine($"║ IP: {stats.IP,-15} | Frames: {stats.FramesForwarded,6} | FPS: {fps,4:F1}   ║");
                        }

                        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
                    }
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();

            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    client.TcpClient.Close();
                }
                clients.Clear();
                sessions.Clear();
                streamStats.Clear();
            }
        }
    }

    public class ConnectedClient
    {
        public TcpClient TcpClient { get; set; }
        public string RemoteEndPoint { get; set; }

        public ConnectedClient(TcpClient client)
        {
            TcpClient = client;
            RemoteEndPoint = client.Client.RemoteEndPoint.ToString();
        }
    }

    public class ClientSession
    {
        public string Password { get; set; }
        public ConnectedClient ControlledClient { get; set; }
        public ConnectedClient ControllerClient { get; set; }
    }

    public class StreamStats
    {
        public string IP { get; set; }
        public bool IsStreaming { get; set; }
        public int FramesForwarded { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastFrameTime { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         REMOTE PC CONTROL SERVER v2.0                    ║");
            Console.WriteLine("║         HCMUS - Socket Programming Project               ║");
            Console.WriteLine("║         ✓ Webcam Streaming Support                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            Server server = new Server();
            server.Start(8888);

            Console.WriteLine("\n[INFO] Server is running. Features:");
            Console.WriteLine("  • Applications & Processes Control");
            Console.WriteLine("  • Screenshot Capture");
            Console.WriteLine("  • Keylogger");
            Console.WriteLine("  • Webcam Real-time Streaming (15 FPS)");
            Console.WriteLine("  • System Shutdown/Restart");
            Console.WriteLine("\nPress any key to stop the server...\n");

            Console.ReadKey();

            Console.WriteLine("\n[SERVER] Shutting down...");
            server.Stop();
            Console.WriteLine("[SERVER] Server stopped successfully.");
        }
    }
}