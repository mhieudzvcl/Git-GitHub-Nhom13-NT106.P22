//login
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace Login_or_Signup
{
    public partial class Login_SignUp : Form
    {
        private WelcomeScreen welcomeScreen;
        public Login_SignUp(WelcomeScreen welcome)
        {
            InitializeComponent();
            this.welcomeScreen = welcome;
        }

        private string SomeMethod()
        {
            string ip = welcomeScreen.ServerIP;
            // sử dụng IP ở đây
            return ip;
        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            this.Close();
            WelcomeScreen welcomeScreen = new WelcomeScreen();
            welcomeScreen.Visible = true;

        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            this.Close();
            WelcomeScreen welcomeScreen = new WelcomeScreen();
            welcomeScreen.Show();
        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {
            if (txtPassword.PasswordChar == '\0')
            {
                // Đặt thành mật khẩu dạng ẩn
                txtPassword.PasswordChar = '*';
                pictureboxPassword.Image = Image.FromFile("C:\\LTMCB-Project\\icons8-blind-32.png");
            }
            else
            {
                // Hiển thị mật khẩu
                txtPassword.PasswordChar = '\0';
                pictureboxPassword.Image = Image.FromFile("C:\\LTMCB-Project\\icons8-invisible-50.png");
            }
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Close();
            SignUp signup = new SignUp(welcomeScreen);
            signup.Show();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            var msg = new RequestMessage
            {
                Type = "login",
                Username = txtUsername.Text,
                Password = txtPassword.Text
            };

            string response = SendRequest(msg);
            if (response == "login_success")
            {
                MessageBox.Show("Đăng nhập thành công!");
                Lobby lobby = new Lobby(txtUsername.Text, welcomeScreen.ServerIP); // truyền username và IP
                lobby.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Đăng nhập thất bại! Sai username hoặc password.");
            }
        }

        //cái này dùng để dùng IP của form WelcomeScreen
        private string SendRequest(object requestObj)
        {
            try
            {
                string serverIP = SomeMethod();
                int port = 9000;

                using (TcpClient client = new TcpClient(serverIP, port))
                {
                    NetworkStream stream = client.GetStream();

                    string json = JsonSerializer.Serialize(requestObj);
                    byte[] dataToSend = Encoding.UTF8.GetBytes(json);
                    stream.Write(dataToSend, 0, dataToSend.Length);

                    // Đọc response
                    byte[] buffer = new byte[4096];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public class RequestMessage
        {
            public string Type { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
        }
    }
}
