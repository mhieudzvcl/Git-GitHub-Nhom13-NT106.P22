using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    }
}
