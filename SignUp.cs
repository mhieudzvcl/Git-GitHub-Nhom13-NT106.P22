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


namespace Login_or_Signup
{
    public partial class SignUp : Form
    {
        public SignUp()
        {
            InitializeComponent();
        }
        string connectionString = "Data Source=localhost;Initial Catalog=VibraSound;Integrated Security=True;";

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            Login_SignUp lg_signup = new Login_SignUp();
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

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (username == "" || password == "" || email == "")
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // ✅ Kiểm tra Username đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM Account WHERE Username = @Username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@Username", username);

                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                        return;
                    }

                    // Nếu chưa tồn tại thì INSERT
                    string insertQuery = "INSERT INTO Account (Username, Password, Gmail) VALUES (@Username, @Password, @Gmail)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@Username", username);
                    insertCmd.Parameters.AddWithValue("@Password", password); // Nên mã hoá mật khẩu sau này
                    insertCmd.Parameters.AddWithValue("@Gmail", email);

                    int result = insertCmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Đăng ký thành công!");
                    }
                    else
                    {
                        MessageBox.Show("Đăng ký thất bại.");
                    }
                    Login_SignUp login = new Login_SignUp();
                    this.Hide();
                    login.Show();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Lỗi SQL: " + ex.Message);
                }
            }
        }

    }
}
