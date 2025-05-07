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
    public partial class SignUp : Form
    {
        public SignUp()
        {
            InitializeComponent();
        }

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
    }
}
