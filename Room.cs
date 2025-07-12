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
using System.Net.Sockets;
using Login_or_Signup;
using System.Threading;
using System.Text.Json.Nodes; // thêm ở đầu file
using Guna.UI2.WinForms;

namespace Login_or_Signup
{
    public partial class Room : Form
    {
        // private string NameInApp;
        private System.Windows.Forms.Timer seekTimer;
        private TcpClient persistentClient;
        private NetworkStream persistentStream;
        private string myNameInApp;
        private Image hostAvatarImage;
        private string currentRoomID;
        private string lastTitle = "";
        private string lastDescription = "";
        private string lastCoverImageUrl = "";



        //      private string hostNameInApp;
        TcpClient tcpClient;
        NetworkStream roomStream;
        Thread listenThread;

        private Dictionary<string, Color> userColorMap = new Dictionary<string, Color>();
        private Color[] predefinedColors = new Color[]
        {
        Color.LightBlue,
        Color.LightGreen,
        Color.LightPink,
        Color.LightYellow,
        Color.LightCoral,
        Color.LightSalmon,
        Color.LightCyan,
        Color.Khaki,
        Color.Thistle,
        Color.Plum
        };
        private int colorIndex = 0;

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
        private void HandleServerMessage(string json)
        {
            try
            {
                var data = JsonSerializer.Deserialize<ServerBroadcast>(json);

                if (data.type == "room_member_list")
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ClearAllClientSlots(); // clear tất cả slot từ 2 -> 5
                        lblNameRoom.Text = data.roomName;
                        foreach (var mem in data.member)
                        {
                            AddMemberToUISlot(mem.name, mem.avatar, mem.slot); // dùng chung cho host và client
                        }
                    });
                }
                else if (data.type == "play_media")
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        string videoPath = data.videoPath;

                        // 👉 Chuyển file:// URI về local path nếu cần
                        if (videoPath.StartsWith("file:///"))
                        {
                            videoPath = new Uri(videoPath).LocalPath;
                        }

                        long seekPosition = data.seek;
                        string action = data.action;

                        if (!File.Exists(videoPath) && !videoPath.StartsWith("http"))
                        {
                            MessageBox.Show("Client chưa có file video này: " + videoPath);
                            return;
                        }

                        // Nếu chưa có media hoặc media khác thì load lại
                        if (_mediaPlayer.Media == null || _mediaPlayer.Media.Mrl != new Uri(videoPath).AbsoluteUri)
                        {
                            _mediaPlayer.Stop();
                            var media = new Media(_libVLC, new Uri(videoPath));
                            _mediaPlayer.Media = media;
                            _mediaPlayer.Play();
                        }

                        switch (action)
                        {
                            case "play":
                                _mediaPlayer.Play();
                                btnPlayPause.Text = "⏸";
                                break;
                            case "pause":
                                _mediaPlayer.Pause();
                                btnPlayPause.Text = "▶️";
                                break;
                            case "seek":
                            case "forward":
                            case "rewind":
                                _mediaPlayer.Time = seekPosition;
                                break;
                            case "stop":
                                _mediaPlayer.Stop();
                                btnPlayPause.Text = "▶️";

                                PanelCoverControls.Visible = false;
                                btnPlayPause.Visible = false;
                                btnForward.Visible = false;
                                btnRewind.Visible = false;
                                trackBarSeek.Visible = false;
                                lblTime.Visible = false;
                                lblTime.Text = "";

                                // Ẩn thông tin bài hát
                                PicSong.Visible = false;
                                SongName.Visible = false;
                                SongDescription.Visible = false;

                                // Reset nội dung
                                SongName.Text = "";
                                SongDescription.Text = "";
                                PicSong.Image = null;

                                // Reset biến nhớ
                                lastTitle = "";
                                lastDescription = "";
                                lastCoverImageUrl = "";

                                return;

                        }

                        // ✅ Hiển thị thông tin bài hát nếu có
                        PanelCoverControls.Visible = true;
                        btnPlayPause.Visible = true;
                        btnForward.Visible = true;
                        btnRewind.Visible = true;
                        trackBarSeek.Visible = true;
                        lblTime.Visible = true;

                        PicSong.Visible = true;
                        SongName.Visible = true;
                        SongDescription.Visible = true;

                        // Nếu có thông tin mới từ server thì cập nhật vào biến nhớ
                        if (!string.IsNullOrEmpty(data.title))
                            lastTitle = data.title;
                        if (!string.IsNullOrEmpty(data.description))
                            lastDescription = data.description;
                        if (!string.IsNullOrEmpty(data.coverImageUrl))
                            lastCoverImageUrl = data.coverImageUrl;

                        // Hiển thị lại từ biến nhớ
                        SongName.Text = !string.IsNullOrEmpty(lastTitle) ? "Currently play: " + lastTitle : "";
                        SongDescription.Text = lastDescription;

                        // Hiển thị ảnh từ link
                        if (!string.IsNullOrEmpty(lastCoverImageUrl))
                        {
                            try
                            {
                                using (HttpClient httpClient = new HttpClient())
                                {
                                    var imageStream = httpClient.GetStreamAsync(lastCoverImageUrl).Result;
                                    PicSong.Image = Image.FromStream(imageStream);
                                }
                            }
                            catch
                            {
                                PicSong.Image = null;
                            }
                        }
                        else
                        {
                            PicSong.Image = null;
                        }
                    });
                }
                else if (data.type == "room_chat_message")
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        string sender = data.sender; // luôn là tên thật do server broadcast lại
                        string content = data.content;
                        string timestamp = data.timestamp;

                        AddChatMessageToPanel(sender, content, timestamp);
                    });
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xử lý dữ liệu từ server: " + ex.Message);
            }
        }

        private int lastMessageBottom = 56; // Vị trí Y bắt đầu cho tin nhắn đầu tiên

        private void AddChatMessageToPanel(string sender, string content, string timestamp)
        {
            // Mỗi người dùng 1 màu nếu bạn dùng Dictionary - bạn có thể tích hợp lại ở đây
            bool isOwnMessage = sender == myNameInApp;
            string displaySenderLine = $"{timestamp} \n {sender}:";

            var msgContainer = new Guna.UI2.WinForms.Guna2GradientPanel
            {
                AutoSize = true,
                BorderRadius = 11,
                Padding = new Padding(8),
                Margin = new Padding(8, 0, 8, 10),
                MaximumSize = new Size(PanelChatRoom.Width - 40, 0),
                FillColor = isOwnMessage ? Color.LightBlue : Color.Beige,
                FillColor2 = isOwnMessage ? Color.LightCyan : Color.DimGray,
                Location = new Point(1, lastMessageBottom)
            };

            var senderLabel = new Label
            {
                Text = displaySenderLine,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.Black
            };

            var contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 11),
                MaximumSize = new Size(msgContainer.MaximumSize.Width - 16, 0),
                AutoSize = true,
                ForeColor = Color.Black
            };

            msgContainer.Controls.Add(senderLabel);
            msgContainer.Controls.Add(contentLabel);
            contentLabel.Location = new Point(0, senderLabel.Bottom + 5);

            PanelChatRoom.Controls.Add(msgContainer);

            // Cập nhật tọa độ Y cho tin nhắn tiếp theo
            lastMessageBottom = msgContainer.Bottom + 10;

            // Cuộn xuống cuối nếu nội dung vượt khỏi panel
            PanelChatRoom.ScrollControlIntoView(msgContainer);
        }


        private void SendToServer(string json)
        {
            if (persistentStream != null && persistentStream.CanWrite)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                persistentStream.Write(bytes, 0, bytes.Length);
            }
        }


        private void btnUpload_Click(object sender, EventArgs e)
        {
            PicSong.Visible = false;
            SongDescription.Visible = false;
            SongName.Visible = false;
            btnPlayPause.Text = "⏸";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp4;*.mp3;*.avi;*.wmv";
            openFileDialog.Title = "Select a media file";

            string filePath = null; // ✅ Đặt ở đây

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                PlayMedia(filePath);
            }

            if (!string.IsNullOrEmpty(filePath)) // Chỉ gửi nếu có file
            {
                var msg = new
                {
                    type = "play_media",
                    roomID = txtIDRoom.Text.Trim(),
                    videoPath = filePath,
                    seek = 0,
                    action = "play"
                };
                string json = JsonSerializer.Serialize(msg);
                SendToServer(json); // ✅ Giải quyết lỗi thứ 2 bên dưới
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

        private void SendPlaybackAction(string action)
        {
            if (persistentStream == null) return;
            long seekTime = _mediaPlayer.Time; // thời gian hiện tại (ms)
            // Chuyển file:// URI về local path
            string videoPath = _mediaPlayer?.Media?.Mrl;
            if (videoPath != null && videoPath.StartsWith("file:///"))
            {
                videoPath = new Uri(videoPath).LocalPath;
            }

            var msg = new
            {
                type = "play_media",
                roomID = txtIDRoom.Text.Trim(),
                videoPath = videoPath,
                //   seek = _mediaPlayer?.Time ?? 0,
                seek = seekTime,
                action = action,
                title = "",             // gửi rỗng
                description = "",       // gửi rỗng
                coverImageUrl = ""
            };
            string json = JsonSerializer.Serialize(msg);
            SendToServer(json);
        }
        private void ClearAllClientSlots()
        {
            for (int i = 2; i <= 5; i++) // clear tất cả slot từ 1 đến 5
            {
                PictureBox avatarPic = this.Controls.Find("avtUser" + i, true).FirstOrDefault() as PictureBox;
                Label nameLbl = this.Controls.Find("lblNameUser" + i, true).FirstOrDefault() as Label;

                if (avatarPic != null)
                    avatarPic.Image = Properties.Resources.anhdaidienmacdinh2;

                if (nameLbl != null)
                    nameLbl.Text = $"User {i}";
            }

            usedSlots.Clear();
            addedMembers.Clear();
            memberSlotMap.Clear();
        }
        private string FormatTime(long seconds)
        {
            long minutes = seconds / 60;
            long remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }

        private Dictionary<string, int> memberSlotMap = new Dictionary<string, int>();
        private int maxSlotUsed = 1; // Bắt đầu từ 1, vì host là slot 1

        private HashSet<int> usedSlots = new HashSet<int>();
        private HashSet<string> addedMembers = new HashSet<string>();

        public void AddMemberToUISlot(string name, string avatarPath, int slot)
        {
            if (addedMembers.Contains(name)) return;

            PictureBox avatarPic = this.Controls.Find("avtUser" + slot, true).FirstOrDefault() as PictureBox;
            Label nameLbl = this.Controls.Find("lblNameUser" + slot, true).FirstOrDefault() as Label;

            if (avatarPic != null && nameLbl != null)
            {
                nameLbl.Text = name;

                if (!string.IsNullOrEmpty(avatarPath) && File.Exists(avatarPath))
                    avatarPic.Image = Image.FromFile(avatarPath);
                else
                    avatarPic.Image = Properties.Resources.anhdaidienmacdinh2;

                addedMembers.Add(name);
                usedSlots.Add(slot);
                memberSlotMap[name] = slot;
            }
        }
        public void AddHostToSlot1(string name, string avatarPath)
        {
            PictureBox avatarPic = this.Controls.Find("avtUser1", true).FirstOrDefault() as PictureBox;
            Label nameLbl = this.Controls.Find("lblUserName1", true).FirstOrDefault() as Label;

            if (avatarPic != null && nameLbl != null)
            {
                nameLbl.Text = name;
                avatarPic.Image = hostAvatarImage ?? Properties.Resources.anhdaidienmacdinh2;

                usedSlots.Add(1);
                addedMembers.Add(name);
                memberSlotMap[name] = 1;
            }
            maxSlotUsed = 1;
        }
        public Room(string serverIP, string roomID, string roomName, string hostName, Image hostAvt, TcpClient client)
        {
            InitializeComponent();

            this.ServerIP = serverIP;
            this.persistentClient = client;
            this.persistentStream = client.GetStream();
            this.myNameInApp = hostName;
            this.tcpClient = client;
            this.hostAvatarImage = hostAvt;
            
            lblNameRoom.Text = roomName;
            txtIDRoom.Text = "ID: " + roomID;
            this.currentRoomID = roomID;

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mediaPlayer;

            seekTimer = new System.Windows.Forms.Timer();
            seekTimer.Interval = 1000;
            seekTimer.Tick += SeekTimer_Tick;
            seekTimer.Start();


            StartListening(); 
        }
        private string CurUserName = "";
        public Room(string serverIP, string roomID, string roomName, string hostName, Image hostAvt, TcpClient client, string currentUserName)
        {
            InitializeComponent();

            this.ServerIP = serverIP;
            this.persistentClient = client;
            this.persistentStream = client.GetStream();
            this.myNameInApp = currentUserName;
            this.tcpClient = client;
            this.hostAvatarImage = hostAvt;

            lblNameRoom.Text = roomName;
            txtIDRoom.Text = roomID;
            this.currentRoomID = roomID;
            CurUserName = currentUserName;


            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mediaPlayer;

            seekTimer = new System.Windows.Forms.Timer();
            seekTimer.Interval = 1000;
            seekTimer.Tick += SeekTimer_Tick;
            seekTimer.Start();

            // ✅ So sánh current user với host thực sự
            if (currentUserName != hostName)
            {
                btnUpload.Visible = false;
                btnStop.Visible = false;
                txtSearch.Visible = false;  
            }

            StartListening();
        }
        public Room(string serverIP, string roomID, string roomName, TcpClient client)
        {
            InitializeComponent();

            this.ServerIP = serverIP;
            this.persistentClient = client;
            this.persistentStream = client.GetStream();
            this.tcpClient = client;

            lblNameRoom.Text = roomName;
            txtIDRoom.Text = roomID;
            this.currentRoomID = roomID;

            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mediaPlayer;

            seekTimer = new System.Windows.Forms.Timer();
            seekTimer.Interval = 1000;
            seekTimer.Tick += SeekTimer_Tick;
            seekTimer.Start();

            StartListening(); // bắt đầu lắng nghe từ server
        }
        private string SaveTempAvatarImage(Image image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            string tempPath = Path.Combine(Path.GetTempPath(), $"avatar_{Guid.NewGuid()}.png");

            // Fix lỗi GDI+ bằng cách tạo bản sao
            using (Bitmap bmp = new Bitmap(image.Width, image.Height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
                bmp.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }
        public static Image Base64ToImage(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            using (var ms = new MemoryStream(bytes))
            {
                return Image.FromStream(ms); // nhưng không dispose ms quá sớm
            }
        }

        private string ServerIP;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private List<DeezerTrack> searchResults = new List<DeezerTrack>();

        public void StartListening()
        {
            Task.Run(() =>
            {
                byte[] buffer = new byte[4096];
                while (true)
                {
                    try
                    {
                        int bytesRead = persistentStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        HandleServerMessage(json);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi đọc dữ liệu từ server: " + ex.Message);
                        break;
                    }
                }
            });
        }
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
                    txtChat.AppendText(emoji + " ");
                    txtChat.Focus(); // Đặt con trỏ vào ô nhập văn bản
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
                   this.Close(); // Đóng form Room hiện tại
            }    
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
            int index = listBoxResults.SelectedIndex;
            if (index >= 0 && index < searchResults.Count)
            {
                var selectedTrack = searchResults[index];

                // Giao diện hiển thị controls
                PanelCoverControls.Visible = true;
                btnPlayPause.Visible = true;
                btnForward.Visible = true;
                btnRewind.Visible = true;
                trackBarSeek.Visible = true;
                lblTime.Visible = true;

                PicSong.Visible = true;
                SongDescription.Visible = true;
                SongName.Visible = true;

                // Hiển thị tên bài hát
                SongName.Text = "Currently play: " + selectedTrack.title;

                // Hiển thị mô tả bài hát
                SongDescription.Text = $"{selectedTrack.artist?.name} - {selectedTrack.album?.title}";

                // Hiển thị ảnh bìa
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
                else
                {
                    PicSong.Image = null;
                }

                // Phát nhạc preview (cục bộ host)
                PlayMedia(selectedTrack.preview);

                // Gửi đến các client khác
                var msg = new
                {
                    type = "play_media",
                    roomID = txtIDRoom.Text.Trim(),
                    videoPath = selectedTrack.preview,
                    seek = 0,
                    action = "play",
                    title = selectedTrack.title,
                    description = $"{selectedTrack.artist?.name} - {selectedTrack.album?.title}",
                    coverImageUrl = selectedTrack.album?.cover_medium
                };
                string json = JsonSerializer.Serialize(msg);
                SendToServer(json);

                // Xóa tìm kiếm
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
                SendPlaybackAction("play");
            }
            else
            {
                _mediaPlayer.Pause();
                btnPlayPause.Text = "▶️";
                SendPlaybackAction("pause");
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            // Ẩn toàn bộ controls và thông tin
            PanelCoverControls.Visible = false;
            btnPlayPause.Visible = false;
            btnForward.Visible = false;
            btnRewind.Visible = false;
            trackBarSeek.Visible = false;
            lblTime.Visible = false;
            lblTime.Text = "";

            SongName.Visible = false;
            SongDescription.Visible = false;
            PicSong.Visible = false;

            SongName.Text = "";
            SongDescription.Text = "";
            PicSong.Image = null;

            _mediaPlayer.Stop();

            // Gửi hành động stop đến tất cả client
            SendPlaybackAction("stop");
        }

        private void trackBarSeek_Scroll(object sender, ScrollEventArgs e)
        {
            //_mediaPlayer.Time = trackBarSeek.Value * 1000;
            //SendPlaybackAction("seek");
            _mediaPlayer.Time = trackBarSeek.Value * 1000;
            SendPlaybackAction("seek");
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
                long newTime = _mediaPlayer.Time - 2000;
                _mediaPlayer.Time = Math.Max(newTime, 0);

                SendPlaybackAction("rewind");
            }
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                long newTime = _mediaPlayer.Time + 2000;
                if (newTime < _mediaPlayer.Length)
                    _mediaPlayer.Time = newTime;

                SendPlaybackAction("forward");
            }
        }

        private void SendVideoState(string action, double position)
        {
            var request = new
            {
                type = "video_state",
                roomID = currentRoomID,
                action = action,
                position = position
            };

            string json = JsonSerializer.Serialize(request);
            SendToServer(json); // Hàm này gửi JSON qua TCP stream
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
            Application.Exit(); // Đóng toàn bộ ứng dụng
        }
        private void RoomPanelBottom_Paint(object sender, PaintEventArgs e)
        {

        }
        private void SongName_Click(object sender, EventArgs e)
        {

        }

        private void RoomtxtChat_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtChat.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                var msg = new
                {
                    type = "room_chat_message",
                    roomID = currentRoomID,
                    sender = myNameInApp,
                    content = message,
                    timestamp = DateTime.Now.ToString("HH:mm:ss")
                };

                string json = JsonSerializer.Serialize(msg);
                SendToServer(json);

                txtChat.Clear();
            }
        }
    }
}
