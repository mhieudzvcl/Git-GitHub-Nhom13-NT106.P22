using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Guna.UI2.WinForms;


namespace Setting_UI
{
    public partial class Profile: Form
    {
        private Image MaskImageToCircle(Image srcImage, int diameter)
        {
            Bitmap bmp = new Bitmap(diameter, diameter);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, diameter, diameter);
                g.SetClip(path);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Scale ảnh cho vừa với nút tròn
                g.DrawImage(srcImage, new Rectangle(0, 0, diameter, diameter));
            }
            return bmp;
        }
        

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void mainButton_Click_1(object sender, EventArgs e)
        {
            buttonPanel.Visible = !buttonPanel.Visible;
        }
        public Profile()
        {
            InitializeComponent();
            buttonPanel.Visible = false;
            // Thêm các nút phụ vào panel
            buttonPanel.Controls.Add(subButton1);
            buttonPanel.Controls.Add(subButton2);
            // Thêm các control vào flowpanel
            flowPanel.Controls.Add(mainButton);   
            flowPanel.Controls.Add(buttonPanel);  
            flowPanel.Controls.Add(panelDarkmode);
            
            // Xử lý avt
            guna2CircleButton1.FillColor = Color.Transparent;
            guna2CircleButton1.BackColor = Color.Transparent;
            guna2CircleButton1.UseTransparentBackground = true;
            Image original = Image.FromFile(@"C:\Users\adm\Pictures\Saved Pictures\avt.jpg");
            int size = guna2CircleButton1.Width; 
            Image circularImage = MaskImageToCircle(original, size);
            guna2CircleButton1.Image = circularImage;
            guna2CircleButton1.ImageSize = new Size(size, size);
            guna2CircleButton1.ImageAlign = HorizontalAlignment.Center;
            guna2CircleButton1.Text = "";
            guna2CircleButton1.TextAlign = HorizontalAlignment.Center;
            guna2CircleButton1.ImageOffset = new Point(0, 0);

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Setting_Click(object sender, EventArgs e)
        {

        }
        private Changename cn = null;
        private Changepassword cp = null;

        private void subButton1_Click_1(object sender, EventArgs e)
        {
            if (cn == null || cn.IsDisposed)
            {
                cn = new Changename();
                cn.Show();
            }
            else if (!cn.Visible)
            {
                cn.Show();
            }
            else
            {
                cn.BringToFront();
                cn.Focus();
            }
        }
        private void subButton2_Click_1(object sender, EventArgs e)
        {
            if (cp == null || cp.IsDisposed)
            {
                cp = new Changepassword();
                cp.Show();
            }
            else if (!cp.Visible)
            {
                cp.Show();
            }
            else
            {
                cp.BringToFront();
                cp.Focus();
            }
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
