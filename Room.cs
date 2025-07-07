using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using LibVLCSharp.Shared;

namespace Login_or_Signup
{
    public partial class Room : Form
    {
        // private string NameInApp;
        private Timer seekTimer;
        public Room()
        {
            InitializeComponent();
        }

        private void SeekTimer_Tick(object sender, EventArgs e)
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying && _mediaPlayer.Length > 0)
            {
                long currentSeconds = _mediaPlayer.Time / 1000;
                long totalSeconds = _mediaPlayer.Length / 1000;

                if (!trackBarSeek.Capture) // Người dùng không kéo thì mới cập nhật
                {
                    if (trackBarSeek.Maximum != (int)totalSeconds)
                        trackBarSeek.Maximum = (int)totalSeconds;

                    trackBarSeek.Value = Math.Min((int)currentSeconds, trackBarSeek.Maximum);
                }

                lblTime.Text = $"{FormatTime(currentSeconds)} / {FormatTime(totalSeconds)}";
            }
        }

        private string FormatTime(long seconds)
        {
            long minutes = seconds / 60;
            long remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }
        public Room(string serverIP)
        {
            InitializeComponent();
            ServerIP = serverIP;

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mediaPlayer;

            seekTimer = new Timer();
            seekTimer.Interval = 1000;
            seekTimer.Tick += SeekTimer_Tick;
            seekTimer.Start();
        }

        private string ServerIP;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private List<DeezerTrack> searchResults = new List<DeezerTrack>();

        private async Task SearchDeezer(string keyword)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"https://api.deezer.com/search?q={Uri.EscapeDataString(keyword)}";
                string response = await client.GetStringAsync(url);

                var result = JsonSerializer.Deserialize<DeezerResponse>(response);
                searchResults = result.data;

                listBoxResults.Items.Clear();
                foreach (var track in searchResults)
                {
                    listBoxResults.Items.Add($"{track.title} - {track.artist.name}");
                }
            }
        }

        private void PlayMedia(string url)
        {
            try
            {
                _mediaPlayer?.Stop();
                _mediaPlayer?.Media?.Dispose();

                Media media;

                if (File.Exists(url)) // Local file
                    media = new Media(_libVLC, new Uri(url));
                else // Streaming
                    media = new Media(_libVLC, url, FromType.FromLocation);

                _mediaPlayer.Media = media;
                _mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi phát media: " + ex.Message);
            }
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
                btn.Font = new Font("Segoe UI Emoji", 18);
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
            openFileDialog.Filter = "Media Files|*.mp4;*.mp3;*.avi;*.wmv";
            openFileDialog.Title = "Select a media file";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                PlayMedia(filePath);
            }
            SongName.Text = "";
            SongDescription.Text = "";
            PicSong.Image = null;

            PanelCoverControls.Visible = true;
            btnPlayPause.Visible = true;
            btnForward.Visible = true;
            btnRewind.Visible = true;
            trackBarSeek.Visible = true;
            lblTime.Visible = true;

            PicSong.Visible = false;
            SongDescription.Visible = false;
            SongName.Visible = false;
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (keyword.Length >= 3)
            {
                listBoxResults.Visible = true;

                // Gắn listBox nằm dưới txtSearch
                listBoxResults.Top = txtSearch.Bottom + 5;
                listBoxResults.Left = txtSearch.Left;
                listBoxResults.Width = txtSearch.Width;

                await SearchDeezer(keyword);
            }
            else
            {
                listBoxResults.Visible = false;
            }
        }

        private void listBoxResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            PanelCoverControls.Visible = true;
            btnPlayPause.Visible = true;
            btnForward.Visible = true;
            btnRewind.Visible = true;
            trackBarSeek.Visible = true;
            lblTime.Visible = true;

            SongDescription.Visible = true;
            SongName.Visible = true;
            PicSong.Visible = true;
            SongName.Text = "";
            SongDescription.Text = "";
            PicSong.Image = null;
            int index = listBoxResults.SelectedIndex;
            if (index >= 0 && index < searchResults.Count)
            {
                var selectedTrack = searchResults[index];

                // Phát nhạc
                PlayMedia(selectedTrack.preview);

                // Gán ảnh bài hát
                if (!string.IsNullOrEmpty(selectedTrack.album?.cover_medium))
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var imgStream = client.GetStreamAsync(selectedTrack.album.cover_medium).Result;
                            PicSong.Image = Image.FromStream(imgStream);
                        }
                    }
                    catch
                    {
                        PicSong.Image = null;
                    }
                }

                // Gán tên bài hát và mô tả
                SongName.Text = "Currently play: " + selectedTrack.title;
                SongDescription.Text = $"{selectedTrack.artist?.name} - {selectedTrack.album?.title}";

                txtSearch.Text = "";
                listBoxResults.Visible = false;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Play();
                btnPlayPause.Text = "⏸";
            }
            else
            {
                _mediaPlayer.Pause();
                btnPlayPause.Text = "▶️";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            PanelCoverControls.Visible = false;
            btnPlayPause.Visible = false;
            btnForward.Visible = false;
            btnRewind.Visible = false;
            trackBarSeek.Visible = false;
            lblTime.Visible = false;

            SongName.Text = "";
            SongDescription.Text = "";
            PicSong.Image = null;

            SongName.Visible = false;
            SongDescription.Visible = false;
            PicSong.Visible = false;
            PicSong.Image = null;
            _mediaPlayer.Stop();
            lblTime.Text = "";
        }

        private void trackBarSeek_Scroll(object sender, ScrollEventArgs e)
        {
            _mediaPlayer.Time = trackBarSeek.Value * 1000;
        }

        private void trackBarVolume_Scroll(object sender, ScrollEventArgs e)
        {
            _mediaPlayer.Volume = trackBarVolume.Value;
        }

        public class DeezerResponse
        {
            public List<DeezerTrack> data { get; set; }
        }

        public class DeezerTrack
        {
            public string title { get; set; }
            public string preview { get; set; }
            public DeezerArtist artist { get; set; }
            public string link { get; set; } // link đến trang bài hát (nếu cần)

            public DeezerAlbum album { get; set; } // chứa ảnh bìa
        }

        public class DeezerAlbum
        {
            public string title { get; set; }
            public string cover_medium { get; set; } // ảnh vừa (320x320)
        }

        public class DeezerArtist
        {
            public string name { get; set; }
        }

        private void btnRewind_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                long newTime = _mediaPlayer.Time - 1000; // tua lùi 5 giây
                _mediaPlayer.Time = Math.Max(newTime, 0);
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                long newTime = _mediaPlayer.Time + 1000; // tua tới 5 giây
                if (newTime < _mediaPlayer.Length)
                    _mediaPlayer.Time = newTime;
            }
        }

        private void trackBarSeek_ValueChanged(object sender, EventArgs e)
        {

        }

        private void RoomMainPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2ControlBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
