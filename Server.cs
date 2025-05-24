using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace VibraSoundServer
{
    public partial class Server : Form
    {
        static TcpListener listener;
        static string connectionString = "Server=localhost;Database=VibraSound;Integrated Security=True";
        public static Server instance;
        public Server()
        {
            InitializeComponent();
            instance = this;
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Server.instance.Log("Server is running on port 9000...");

            // Tạo thread mới để lắng nghe client, không block UI thread
            Thread listenerThread = new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread t = new Thread(HandleClient);
                    t.Start(client);
                }
            });

            listenerThread.IsBackground = true; // để thread tự kết thúc khi app đóng
            listenerThread.Start();
        }

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            Server.instance.Log("Client connected");

            byte[] buffer = new byte[4096];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Server.instance.Log("Received: " + request);

            var message = JsonSerializer.Deserialize<RequestMessage>(request);
            string response = HandleRequest(message);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);

            Server.instance.Log("Sent: " + response);
            client.Close();
        }

        static string HandleRequest(RequestMessage msg)
        {
            switch (msg.Type)
            {
                case "signup":
                    return Signup(msg.Username, msg.Password, msg.Email);
                case "login":
                    return Login(msg.Username, msg.Password);
                default:
                    return "Invalid";
            }
        }

        static string Signup(string username, string password, string email)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO [User] (Username, Password, Email) VALUES (@u, @p, @e)", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@e", email);

                try
                {
                    cmd.ExecuteNonQuery();
                    Server.instance.Log($"Signup success: {username}");
                    return "signup_success";
                }
                catch
                {
                    Server.instance.Log($"Signup failed: {username}");
                    return "signup_failed";
                }
            }
        }

        static string Login(string username, string password)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE Username=@u AND Password=@p", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    Server.instance.Log($"Login success: {username}");
                    return "login_success";
                }
                else
                {
                    Server.instance.Log($"Login failed: {username}");
                    return "login_failed";
                }
            }
        }

        public class RequestMessage
        {
            public string Type { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }

        //flag
        public void Log(string message)
        {
            if (rtxtInfo.InvokeRequired)
            {
                rtxtInfo.Invoke(new Action(() => {
                    rtxtInfo.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                }));
            }
            else
            {
                rtxtInfo.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            }
        }
    }
}

