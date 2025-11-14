using Microsoft.AspNetCore.SignalR;
using RemotePCControl.WebInterface.Services; // Thêm namespace Service
using System.Threading.Tasks;

namespace RemotePCControl.WebInterface.Hubs
{
    public class ControlHub : Hub
    {
        private readonly ConnectionService _connectionService;

        // Inject (tiêm) Service vào Hub
        public ControlHub(ConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        // Hàm này được JavaScript gọi
        public async Task SendCommandToServer(string commandType, string targetIp, string parameters)
        {
            // Lấy ID của client JS đang gọi
            string connectionId = Context.ConnectionId;

            // Xây dựng lại tin nhắn chuẩn (protocol) mà Server Console của bạn hiểu

            string message = "";
            if (commandType == "LOGIN")
            {
                // JS gửi: "LOGIN", "192...", "12345"
                // Server Console mong muốn: "LOGIN|192...|12345"
                message = $"LOGIN|{targetIp}|{parameters}";
            }
            else
            {
                // JS gửi: "SCREENSHOT", "192...", ""
                // Server Console mong muốn: "COMMAND|192...|SCREENSHOT|"
                message = $"COMMAND|{targetIp}|{commandType}|{parameters}";
            }

            // Gửi lệnh này cho Service xử lý
            await _connectionService.ProcessCommand(connectionId, message);
        }

        // Tự động dọn dẹp khi client JS (tab trình duyệt) bị đóng
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionService.CloseConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}