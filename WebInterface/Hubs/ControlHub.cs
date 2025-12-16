using Microsoft.AspNetCore.SignalR;
using RemotePCControl.WebInterface.Services; // Import ConnectionService namespace
using System.Threading.Tasks;

namespace RemotePCControl.WebInterface.Hubs
{
    public class ControlHub : Hub
    {
        private readonly ConnectionService _connectionService;

        // Inject ConnectionService into Hub
        public ControlHub(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        // This method is invoked from JavaScript client
        public async Task SendCommandToServer(string commandType, string targetIp, string parameters)
        {
            // Get SignalR connection ID of the caller
            string connectionId = Context.ConnectionId;

            // Build message in the protocol format that the TCP Server understands

            string message = "";
            if (commandType == "LOGIN")
            {
                // JS sends: "LOGIN", "192...", "12345"
                // TCP Server expects: "LOGIN|192...|12345"
                message = $"LOGIN|{targetIp}|{parameters}";
            }
            else
            {
                // JS sends: "SCREENSHOT", "192...", ""
                // TCP Server expects: "COMMAND|192...|SCREENSHOT|"
                message = $"COMMAND|{targetIp}|{commandType}|{parameters}";
            }

            // Forward this message to ConnectionService for processing
            await _connectionService.ProcessCommand(connectionId, message);
        }

        public async Task RequestSessions()
        {
            string connectionId = Context.ConnectionId;
            await _connectionService.ProcessCommand(connectionId, "LIST_SESSIONS");
        }

        // Auto cleanup when browser tab disconnects
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionService.CloseConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}