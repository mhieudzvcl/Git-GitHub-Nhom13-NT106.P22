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
    public partial class ChangePassword : Form
    {
        private WelcomeScreen welcomeScreen;

        public ChangePassword(WelcomeScreen screen)
        {
            InitializeComponent();
            this.welcomeScreen = screen;
        }

        private void ChangePassword_Load(object sender, EventArgs e)
        {
            
        }
    }
}
