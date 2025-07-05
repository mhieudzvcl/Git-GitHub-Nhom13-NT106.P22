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
    public partial class Room : Form
    {
        public Room()
        {
            InitializeComponent();
        }

        private void Room_Load(object sender, EventArgs e)
        {
            string[] emojiList = new string[]
   {
        "😀", "😁", "😂", "🤣", "😃", "😄", "😅", "😆", "😉", "😊", "😋", "😎", "😍", "😘", "🥰", "😗", "😙", "😚",
        "🙂", "🤗", "🤩", "🤔", "🤨", "😐", "😑", "😶", "🙄", "😏", "😣", "😥", "😮", "🤐", "😯", "😪", "😫", "🥱"
       // Có thể thêm nhiều emoji nữa
   };

            // Tạo nút emoji và thêm vào panel
            foreach (string emoji in emojiList)
            {
                Button btn = new Button();
                btn.Text = emoji;
                btn.Font = new Font("Segoe UI Emoji", 16);
                btn.Width = 40;
                btn.Height = 40;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Margin = new Padding(7);

                btn.Click += (s, ev) =>
                {
                    RoomtxtChat.AppendText(emoji + " ");
                    RoomtxtChat.Focus(); // Đặt con trỏ vào ô nhập văn bản
                    RoomIconList.Visible = false; // Ẩn danh sách biểu tượng cảm xúc sau khi chọn
                };
                    

                RoomIconList.Controls.Add(btn);
            }
        }

        private void guna2CircleButton3_Click(object sender, EventArgs e)
        {
            RoomIconList.Visible = true;
        }

        private void RoomtxtChat_MouseEnter(object sender, EventArgs e)
        {
            RoomIconList.Visible = false; // Ẩn danh sách biểu tượng cảm xúc khi chuột di chuyển vào ô nhập văn bản
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to leave the music room?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                   this.Close(); // Đóng form Room
                   Lobby lobbyForm = new Lobby();
                   lobbyForm.Show(); // Hiển thị form Lobby
            }    
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP4 files (*.mp4)|*.mp4";
            openFileDialog.Title = "Select a video file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                RoomWMP.URL = filePath;
                RoomWMP.Ctlcontrols.play();
            }
        }
    }
}
