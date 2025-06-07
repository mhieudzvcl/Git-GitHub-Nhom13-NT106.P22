using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login_or_Signup
{
    public partial class ChangePassword : Form
    {
        private WelcomeScreen welcomeScreen;
        private string verificationCode;
        private string userEmail;
        private Timer countdownTimer;
        private DateTime codeExpirationTime;


        public ChangePassword(WelcomeScreen screen)
        {
            InitializeComponent();
            this.welcomeScreen = screen;
        }

        public ChangePassword()
        {
            InitializeComponent();
        }

        public ChangePassword(string email)
        {
            InitializeComponent();
            userEmail = email;
        }   

        private void ChangePassword_Load(object sender, EventArgs e)
        {
           
        }

        private void llblSendCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            llblSendCode.Text = "Resend";
            // 1. Tạo mã xác nhận
            Random rnd = new Random();
            verificationCode = rnd.Next(100000, 999999).ToString();

            // 2. Lấy email người dùng
            userEmail = Lobby.UserEmail;

            try
            {
                // 3. Gửi email
                MailMessage mail = new MailMessage("vibrasoundapp@gmail.com", userEmail);
                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("vibrasoundapp@gmail.com", "mpgtrheirmqzahua"),
                    EnableSsl = true,
                };
                mail.Subject = "Your Verification Code";
                mail.Body = "Your verification code is: " + verificationCode;
                client.Send(mail);

                MessageBox.Show("Verification code sent to your email.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //countdown
                codeExpirationTime = DateTime.Now.AddMinutes(5);

                if (countdownTimer != null)
                {
                    countdownTimer.Stop();
                }
                else
                {
                    countdownTimer = new Timer();
                    countdownTimer.Interval = 1000; // 1 giây
                    countdownTimer.Tick += CountdownTimer_Tick;
                }

                countdownTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send email: " + ex.Message);
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan remaining = codeExpirationTime - DateTime.Now;
            if (remaining <= TimeSpan.Zero)
            {
                countdownTimer.Stop();
                verificationCode = null;
                MessageBox.Show("The verification code has expired. Please request a new code.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // Bạn có thể hiển thị thời gian còn lại nếu muốn, ví dụ cập nhật label:
                // lblTimer.Text = $"Mã hết hạn sau {remaining.Minutes:D2}:{remaining.Seconds:D2}";
                lblTimer.Text = $"Remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }

        private void btnVerifyCode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(verificationCode))
            {
                MessageBox.Show("The verification code has expired. Please request a new code.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtVerifyCode.Text.Trim() == verificationCode)
            {
                countdownTimer?.Stop(); // Dừng timer
                VerifyCodePanel.Visible = false;
                ChangePasswordPanel.Visible = true;
                MessageBox.Show("Code confirmation successful. Please change your password.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Invalid code. Please try again.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
