using RemotePCControl;
using System;
using System.Drawing; // Added for custom coloring
using System.Windows.Forms;

namespace ClientControlled
{
    public partial class Form1 : Form
    {
        // Field to keep reference to ClientControlled object
        private ClientService? client;

        public Form1()
        {
            InitializeComponent();
        }

        // This code runs right after the white form shows up
        private void Form1_Load(object sender, EventArgs e)
        {
            var settings = ClientSettings.Load();

            client = new ClientService(settings.ServerIp, settings.ServerPort);

            bool connected = client.Connect();

            if (connected)
            {
                string ip = client.LocalIp;
                string pass = client.Password;

                lblIp.Text = $"IP Address: {ip}";
                lblPassword.Text = $"Password: {pass}";

                this.Text = "ĐÃ KẾT NỐI - Sẵn sàng nhận lệnh";
                lblIp.Font = new Font(lblIp.Font.FontFamily, 14, FontStyle.Bold);
                lblPassword.Font = new Font(lblPassword.Font.FontFamily, 14, FontStyle.Bold);
            }
            else
            {
                lblIp.Text = $"Không thể kết nối đến Server ({settings.ServerIp}:{settings.ServerPort})";
                lblIp.ForeColor = Color.Red;
                lblPassword.Text = "Password: N/A";
            }
        }
    }
}