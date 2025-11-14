using RemotePCControl;
using System;
using System.Drawing; // Thêm dòng này để chỉnh màu
using System.Windows.Forms;

namespace ClientControlled
{
    public partial class Form1 : Form
    {
        // Tạo một biến để giữ object ClientControlled
        private ClientService? client;

        public Form1()
        {
            InitializeComponent();
        }

        // Code này sẽ chạy ngay khi Form màu trắng hiện lên
        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Khởi tạo client
            // "127.0.0.1" là IP của Server Console (cửa sổ đen)
            // Nếu Server Console chạy ở máy khác, hãy thay IP đó
            client = new ClientService("127.0.0.1", 8888);

            // 2. Thử kết nối
            bool connected = client.Connect();

            // 3. Hiển thị thông tin lên Label
            if (connected)
            {
                // Lấy IP và Mật khẩu TỪ client
                string ip = client.LocalIp;
                string pass = client.Password;

                // Gán vào 2 Label bạn vừa tạo (lblIp và lblPassword)
                lblIp.Text = $"IP Address: {ip}";
                lblPassword.Text = $"Password: {pass}";

                // Cập nhật giao diện
                this.Text = "ĐÃ KẾT NỐI - Sẵn sàng nhận lệnh";
                lblIp.Font = new Font(lblIp.Font.FontFamily, 14, FontStyle.Bold);
                lblPassword.Font = new Font(lblPassword.Font.FontFamily, 14, FontStyle.Bold);
            }
            else
            {
                // Nếu không kết nối được Server Console
                lblIp.Text = "Không thể kết nối đến Server (port 8888)";
                lblIp.ForeColor = Color.Red;
            }
        }
    }
}