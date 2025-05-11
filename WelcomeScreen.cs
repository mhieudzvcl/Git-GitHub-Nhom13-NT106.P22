using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Login_or_Signup
{
    public partial class WelcomeScreen : Form
    {
        private Timer timer;
        private float angle = 0;
        private Image originalImage;
        public WelcomeScreen()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (this.Visible)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void paneldragcontrol_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2ShadowPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //originalImage = pictureboxdisk.Image;

            //if (originalImage != null)
            //{
            //    // Cắt ảnh thành hình tròn trước khi xoay
            //    originalImage = CropToCircle(originalImage);

            //    // Đặt PictureBox thành hình tròn
            //    SetCircularPictureBox(pictureboxdisk);

            //    // Khởi tạo Timer
            //    timer = new Timer();
            //    timer.Interval = 20; // Xoay mỗi 20ms (50 FPS)
            //    timer.Tick += timer1_Tick;
            //    timer.Start(); // Bắt đầu xoay khi form load
            //}
            if (originalImage != null)
            {
                originalImage.Dispose(); // Giải phóng bộ nhớ cũ
            }

            originalImage = pictureboxdisk.Image;

            if (originalImage != null)
            {
                originalImage = CropToCircle(originalImage);
                SetCircularPictureBox(pictureboxdisk);

                // Đảm bảo timer được tạo mới
                if (timer == null)
                {
                    timer = new Timer();
                    timer.Interval = 20;
                    timer.Tick += timer1_Tick;
                }
                timer.Start(); // Bắt đầu lại timer nếu bị dừng
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            angle += 20;
            if (angle >= 360)
                angle = 0;

            if (originalImage != null)
            {
                Image oldImage = pictureboxdisk.Image;
                pictureboxdisk.Image = RotateImage(originalImage, angle);
                if (oldImage != null && oldImage != originalImage)
                {
                    oldImage.Dispose(); // Giải phóng bộ nhớ của ảnh cũ
                }
            }
        }

        private Image RotateImage(Image image, float angle)
        {
            int size = Math.Min(image.Width, image.Height);
            Bitmap rotatedImage = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.Clear(Color.Transparent);

                // Đặt chế độ khử răng cưa cho ảnh mượt hơn
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Xoay từ tâm ảnh
                g.TranslateTransform(size / 2, size / 2);
                g.RotateTransform(angle);
                g.TranslateTransform(-size / 2, -size / 2);

                g.DrawImage(image, new Point(0, 0));
            }
            return rotatedImage;
        }

        private void SetCircularPictureBox(PictureBox pictureBox)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, pictureBox.Width, pictureBox.Height);
            pictureBox.Region = new Region(path);
        }

        private Image CropToCircle(Image srcImage)
        {
            int size = Math.Min(srcImage.Width, srcImage.Height);
            Bitmap croppedImage = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.Clear(Color.Transparent);
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddEllipse(0, 0, size, size);
                    g.SetClip(path);
                    g.DrawImage(srcImage, 0, 0, size, size);
                }
            }

            return croppedImage;
        }

        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            Login_SignUp login = new Login_SignUp();
            login.Show();
            this.Hide();
        }

      

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void WelcomeScreen_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (originalImage != null)
            {
                originalImage.Dispose();
                originalImage = null;
            }

            Application.Exit();
        }

        private void guna2HtmlLabel3_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            //test xong nho xoa
            Lobby lobby = new Lobby();
            lobby.Show();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientHtmlLabel3_Click(object sender, EventArgs e)
        {
            SignUp signUp = new SignUp();
            signUp.Show();
            this.Hide();
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {

        }
    }
}
