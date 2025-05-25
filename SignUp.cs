using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Login_or_Signup
{
    public partial class SignUp : Form
    {
        private WelcomeScreen welcomeScreen;
        public SignUp(WelcomeScreen welcome)
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

        string connectionString = "Data Source=localhost;Initial Catalog=VibraSound;Integrated Security=True;";

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            Login_SignUp lg_signup = new Login_SignUp(welcomeScreen);
            lg_signup.Visible = true;
            this.Close();
        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void pictureboxPassword_Click(object sender, EventArgs e)
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

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Biểu thức regex kiểm tra email cơ bản
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }
        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            // Kiểm tra định dạng email
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Email không hợp lệ! Vui lòng nhập đúng định dạng.");
                return;
            }

            // Gửi request kiểm tra email tồn tại
            var checkEmailRequest = new
            {
                Type = "check_email_exists",
                Email = txtEmail.Text
            };

            string emailCheckResponse = SendRequest(checkEmailRequest);

            if (emailCheckResponse == "email_exists")
            {
                MessageBox.Show("Email đã tồn tại! Vui lòng chọn email khác.");
                return;
            }

            // Tiếp tục đăng ký nếu email chưa tồn tại
            var request = new
            {
                Type = "signup",
                Username = txtUsername.Text,
                Password = txtPassword.Text,
                Email = txtEmail.Text
            };

            string response = SendRequest(request);
            if (response == "signup_success")
            {
                MessageBox.Show("Signup thành công!");
                txtUsername.Clear();
                txtPassword.Clear();
                txtEmail.Clear();
            }
            else
                MessageBox.Show("Signup thất bại hoặc user đã tồn tại! Kiểm tra lại các thông tin vừa nhập");
        }

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
