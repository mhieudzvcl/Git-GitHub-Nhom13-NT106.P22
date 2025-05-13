using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login_or_Signup
{
    public partial class Login_SignUp : Form
    {
        public Login_SignUp()
        {
            InitializeComponent();
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
            SignUp signup = new SignUp();
            signup.Show();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (username == "" || password == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!");
                return;
            }

            using (SqlConnection conn = new SqlConnection("Data Source=localhost;Initial Catalog=VibraSound;Integrated Security=True;"))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM Account WHERE Username = @Username AND Password = @Password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password); 

                    int result = (int)cmd.ExecuteScalar();

                    if (result > 0)
                    {
                        MessageBox.Show("Đăng nhập thành công!");
                        // Chuyển sang form chính sau khi đăng nhập, ví dụ:
                        Lobby lobby = new Lobby();
                        lobby.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Lỗi SQL: " + ex.Message);
                }
            }
        }
    }
}
