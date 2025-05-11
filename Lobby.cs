using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using Guna.UI2.WinForms;
using static System.Net.WebRequestMethods;
using System.Web.UI.WebControls;
//using Setting_UI;


namespace Login_or_Signup
{
    public partial class Lobby : Form
    {
        bool sidebarExpand;
        Timer hoverTimer = new Timer();
        private int originalPlaylistPanelWidth;
        private int originalGuna2GradientPanel2Width;
        private int originalguna2GradientPanel4Width;
        private int originalpanelCoverAlbumsWidth;
        public Lobby()
        {
            InitializeComponent();
        }

        private async void Lobby_Load(object sender, EventArgs e)
        {
            //  AttachMouseEvents(guna2GradientPanel2);
            originalPlaylistPanelWidth = playlistPanel1.Width;
            originalGuna2GradientPanel2Width = guna2GradientPanel2.Width;
            originalguna2GradientPanel4Width = guna2GradientPanel4.Width;
            originalpanelCoverAlbumsWidth = panelCoverAlbums.Width;
            await LoadDeezerPlaylistsAsync();
            await LoadDeezerTopArtistsAsync();
            await LoadDeezerTopAlbumsAsync();
        }

        public async Task LoadDeezerPlaylistsAsync()
        {
            string apiUrl = "https://api.deezer.com/chart/0/playlists";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl);
                    JObject json = JObject.Parse(response);

                    var playlists = json["data"];
                    int maxPanels = 12;
                    int totalPlaylists = playlists.Count();
                    int count = Math.Min(totalPlaylists, maxPanels);

                    // Hiển thị các playlist có dữ liệu
                    for (int i = 0; i < count; i++)
                    {
                        string playlistId = playlists[i]["id"].ToString();
                        string detailUrl = $"https://api.deezer.com/playlist/{playlistId}";

                        var detailResponse = await client.GetStringAsync(detailUrl);
                        JObject detailJson = JObject.Parse(detailResponse);

                        string title = detailJson["title"]?.ToString();
                        string imageUrl = detailJson["picture_medium"]?.ToString();
                        string description = detailJson["description"]?.ToString() ?? "No description";

                        SetPlaylistToUI(i + 1, title, imageUrl, description, true);
                    }

                    // Ẩn các panel còn lại (nếu có)
                    for (int i = count; i < maxPanels; i++)
                    {
                        SetPlaylistToUI(i + 1, "", "", "", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API Error: " + ex.Message);
            }
        }


        private async void SetPlaylistToUI(int index, string title, string imageUrl, string description, bool visible)
        {
            Guna2GradientPanel playlistPanel = FindControlRecursive(this, $"playlistPanel{index}") as Guna2GradientPanel;

            if (playlistPanel != null)
            {
                // Ẩn panel nếu không có dữ liệu
                if (!visible)
                {
                    playlistPanel.Visible = false;

                    // Nếu panel nằm trong FlowLayoutPanel hoặc container nào khác, loại bỏ luôn
                    var parent = playlistPanel.Parent;
                    if (parent is FlowLayoutPanel || parent is TableLayoutPanel)
                    {
                        parent.Controls.Remove(playlistPanel);
                    }
                    return;
                }

                playlistPanel.Visible = true;

                Guna2PictureBox pictureBox = FindControlRecursive(playlistPanel, $"picturebox{index}") as Guna2PictureBox;
                System.Windows.Forms.Label label = FindControlRecursive(playlistPanel, $"label{index}") as System.Windows.Forms.Label;
                System.Windows.Forms.Label labelDesc = FindControlRecursive(playlistPanel, $"labelDesc{index}") as System.Windows.Forms.Label;

                if (pictureBox != null && label != null && labelDesc != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            pictureBox.Image = System.Drawing.Image.FromStream(ms);
                            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }

                    label.Text = title;
                    labelDesc.Text = description;
                }
            }
        }

        private Control FindControlRecursive(Control root, string name)
        {
            foreach (Control control in root.Controls)
            {
                if (control.Name == name)
                    return control;

                Control found = FindControlRecursive(control, name);
                if (found != null)
                    return found;
            }
            return null;
        }





        private void guna2GradientPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel2_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void Tittle_Click(object sender, EventArgs e)
        {

        }

        private async void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {
            string keyword = guna2TextBox1.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                listBox1.Visible = false;
                return;
            }

            string searchUrl = $"https://api.deezer.com/search/track?q={Uri.EscapeDataString(keyword)}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(searchUrl);
                    JObject json = JObject.Parse(response);

                    var data = json["data"];
                    if (data != null && data.Any())
                    {
                        listBox1.Items.Clear();
                        foreach (var item in data.Take(10))
                        {
                            string title = item["title"].ToString();
                            string artist = item["artist"]["name"].ToString();
                            listBox1.Items.Add($"{title} - {artist}");
                        }

                        // Lấy vị trí của guna2TextBox1 trên form
                        Point textBoxLocationOnScreen = guna2TextBox1.PointToScreen(Point.Empty);
                        Point listBoxLocation = this.PointToClient(new Point(textBoxLocationOnScreen.X, textBoxLocationOnScreen.Y + guna2TextBox1.Height));

                        // Đặt vị trí và kích thước của ListBox
                        listBox1.Location = listBoxLocation;
                        listBox1.Width = guna2TextBox1.Width;
                        listBox1.Height = listBox1.ItemHeight * Math.Min(10, listBox1.Items.Count) + 5;

                        listBox1.BringToFront();
                        listBox1.Visible = true;
                    }
                    else
                    {
                        listBox1.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lấy kết quả tìm kiếm: " + ex.Message);
                listBox1.Visible = false;
            }
        }

        private void guna2TextBox1_Leave(object sender, EventArgs e)
        {
            listBox1.Visible = false;
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                guna2TextBox1.Text = listBox1.SelectedItem.ToString();
                listBox1.Visible = false;
                guna2TextBox1.Focus();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {

        }
        private void btnMenu_Click(object sender, EventArgs e)
        {
            timersidebar.Start();

        }
        private void timersidebar_Tick(object sender, EventArgs e)
        {
            int delta = 10;

            if (sidebarExpand)
            {
                sidebarPanel.Width -= delta;

                playlistPanel1.Width += delta;
                guna2GradientPanel2.Width += delta;
                guna2GradientPanel4.Width += delta;
                panelCoverAlbums.Width += delta;

                if (sidebarPanel.Width <= sidebarPanel.MinimumSize.Width)
                {
                    sidebarPanel.Width = sidebarPanel.MinimumSize.Width;
                    sidebarExpand = false;
                    timersidebar.Stop();
                }
            }
            else
            {
                sidebarPanel.Width += delta;

                playlistPanel1.Width -= delta;
                guna2GradientPanel2.Width -= delta;
                guna2GradientPanel4.Width -= delta;
                panelCoverAlbums.Width -= delta;

                if (sidebarPanel.Width >= sidebarPanel.MaximumSize.Width)
                {
                    sidebarPanel.Width = sidebarPanel.MaximumSize.Width;

                    // Trả về kích thước ban đầu
                    playlistPanel1.Width = originalPlaylistPanelWidth;
                    guna2GradientPanel2.Width = originalGuna2GradientPanel2Width;
                    guna2GradientPanel4.Width = originalguna2GradientPanel4Width;
                    panelCoverAlbums.Width = originalpanelCoverAlbumsWidth;
                    sidebarExpand = true;
                    timersidebar.Stop();
                }
            }
        }

        private void guna2GradientPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        //private void AttachMouseEvents(Control parent)
        //{
        //    parent.MouseHover += guna2GradientPanel2_MouseHover;
        //    parent.MouseLeave += guna2GradientPanel2_MouseLeave;

        //    foreach (Control child in parent.Controls)
        //    {
        //        AttachMouseEvents(child); // Đệ quy để gán cho mọi control con
        //    }
        //}
        private int scrollStep = 850; // Số pixel cuộn mỗi lần

        private void btnScrollLeft_Click(object sender, EventArgs e)
        {
            int newScroll = guna2GradientPanel2.HorizontalScroll.Value - scrollStep;
            if (newScroll < 0) newScroll = 0;
            guna2GradientPanel2.AutoScrollPosition = new Point(newScroll, 0);
        }

        private void btnScrollRight_Click(object sender, EventArgs e)
        {
            int maxScroll = guna2GradientPanel2.HorizontalScroll.Maximum;
            int newScroll = guna2GradientPanel2.HorizontalScroll.Value + scrollStep;
            if (newScroll > maxScroll) newScroll = maxScroll;
            guna2GradientPanel2.AutoScrollPosition = new Point(newScroll, 0);
        }

        public async Task LoadDeezerTopArtistsAsync()
        {
            string apiUrl = "https://api.deezer.com/chart/0/artists?limit=30";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(apiUrl);
                    JObject json = JObject.Parse(response);

                    var artists = json["data"];
                    int maxArtists = 25;
                    int totalArtists = artists.Count();
                    int count = Math.Min(totalArtists, maxArtists);

                    for (int i = 0; i < count; i++)
                    {
                        string artistId = artists[i]["id"]?.ToString();
                        string artistName = artists[i]["name"]?.ToString() ?? "Unknown";
                        string imageUrl = artists[i]["picture_medium"]?.ToString() ?? "";

                        // Gọi thêm API chi tiết của nghệ sĩ
                        string detailUrl = $"https://api.deezer.com/artist/{artistId}";
                        string detailResponse = await client.GetStringAsync(detailUrl);
                        JObject detailJson = JObject.Parse(detailResponse);

                        string fanCount = detailJson["nb_fan"]?.ToString() ?? "0";

                        SetArtistToUI(i + 1, artistName, imageUrl, fanCount, true);
                    }

                    // Ẩn các panel còn lại (nếu ít hơn 10)
                    for (int i = count; i < maxArtists; i++)
                    {
                        SetArtistToUI(i + 1, "", "", "", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API Error: " + ex.Message);
            }
        }


        private async void SetArtistToUI(int index, string name, string imageUrl, string fanCount, bool visible)
        {
            Guna2GradientPanel artistPanel = FindControlRecursive(this, $"panelArtist{index}") as Guna2GradientPanel;

            if (artistPanel != null)
            {
                if (!visible)
                {
                    artistPanel.Visible = false;
                    var parent = artistPanel.Parent;
                    if (parent is FlowLayoutPanel || parent is TableLayoutPanel)
                    {
                        parent.Controls.Remove(artistPanel);
                    }
                    return;
                }

                artistPanel.Visible = true;

                Guna2CirclePictureBox pictureBox = FindControlRecursive(artistPanel, $"picArtist{index}") as Guna2CirclePictureBox;
                System.Windows.Forms.Label nameLabel = FindControlRecursive(artistPanel, $"lblArtist{index}") as System.Windows.Forms.Label;
                System.Windows.Forms.Label fanLabel = FindControlRecursive(artistPanel, $"lblFan{index}") as System.Windows.Forms.Label;

                if (pictureBox != null && nameLabel != null && fanLabel != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            pictureBox.Image = System.Drawing.Image.FromStream(ms);
                            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }

                    nameLabel.Text = name;
                    int fans = 0;
                    int.TryParse(fanCount, out fans);
                    fanLabel.Text = $"{fans:N0} fans";
                }
            }
        }
    
        private void btnScrollLeftArtist_Click(object sender, EventArgs e)
        {
            int newScroll = guna2GradientPanel4.HorizontalScroll.Value - 400;
            if (newScroll < 0) newScroll = 0;
            guna2GradientPanel4.AutoScrollPosition = new Point(newScroll, 0);
        }

        private void btnScrollRightArtist_Click(object sender, EventArgs e)
        {
            int maxScroll = guna2GradientPanel4.HorizontalScroll.Maximum;
            int newScroll = guna2GradientPanel4.HorizontalScroll.Value + 400;
            if (newScroll > maxScroll) newScroll = maxScroll;
            guna2GradientPanel4.AutoScrollPosition = new Point(newScroll, 0);
        }

        public async Task LoadDeezerTopAlbumsAsync()
        {
            string initialAlbumId = "302127"; // Album ID khởi đầu (ví dụ: Daft Punk)

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Bước 1: Lấy artistId từ album
                    string albumUrl = $"https://api.deezer.com/album/{initialAlbumId}";
                    var albumResponse = await client.GetStringAsync(albumUrl);
                    JObject albumJson = JObject.Parse(albumResponse);

                    string artistId = albumJson["artist"]?["id"]?.ToString();
                    if (string.IsNullOrEmpty(artistId))
                    {
                        MessageBox.Show("Không tìm thấy artistId từ album.");
                        return;
                    }

                    // Bước 2: Lấy danh sách album của artist
                    string artistAlbumsUrl = $"https://api.deezer.com/artist/{artistId}/albums";
                    var artistAlbumsResponse = await client.GetStringAsync(artistAlbumsUrl);
                    JObject artistAlbumsJson = JObject.Parse(artistAlbumsResponse);
                    var albums = artistAlbumsJson["data"];

                    int maxAlbums = 11;
                    int count = Math.Min(albums.Count(), maxAlbums);

                    // Bước 3: Load từng album chi tiết và cập nhật UI
                    for (int i = 0; i < count; i++)
                    {
                        string albumId = albums[i]["id"]?.ToString();
                        if (string.IsNullOrEmpty(albumId)) continue;

                        string detailUrl = $"https://api.deezer.com/album/{albumId}";
                        var detailResponse = await client.GetStringAsync(detailUrl);
                        JObject detailJson = JObject.Parse(detailResponse);

                        string title = detailJson["title"]?.ToString() ?? "No Title";
                        string imageUrl = detailJson["cover_medium"]?.ToString() ?? "";
                        string fanCount = detailJson["fans"]?.ToString() ?? "0";

                        SetAlbumToUI(i + 1, title, imageUrl, fanCount, true);
                    }

                    // Ẩn các panel còn lại nếu không đủ album
                    for (int i = count; i < maxAlbums; i++)
                    {
                        SetAlbumToUI(i + 1, "", "", "", false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi load album: " + ex.Message);
            }
        }


        private async void SetAlbumToUI(int index, string title, string imageUrl, string fanCount, bool visible)
        {
            Guna2GradientPanel panel = FindControlRecursive(this, $"panelAlbum{index}") as Guna2GradientPanel;

            if (panel != null)
            {
                if (!visible)
                {
                    panel.Visible = false;

                    var parent = panel.Parent;
                    if (parent is FlowLayoutPanel || parent is TableLayoutPanel)
                    {
                        parent.Controls.Remove(panel);
                    }
                    return;
                }

                panel.Visible = true;

                Guna2PictureBox pictureBox = FindControlRecursive(panel, $"picAlbum{index}") as Guna2PictureBox;
                System.Windows.Forms.Label lblTitle = FindControlRecursive(panel, $"lblAlbumName{index}") as System.Windows.Forms.Label;
                System.Windows.Forms.Label lblFans = FindControlRecursive(panel, $"lblAlbumFan{index}") as System.Windows.Forms.Label;

                if (pictureBox != null && lblTitle != null && lblFans != null)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            pictureBox.Image = System.Drawing.Image.FromStream(ms);
                            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        }
                    }

                    lblTitle.Text = title;
                    lblFans.Text = $"{fanCount} fans";
                }
            }
        }

        private void btnScrollLeftAlbum_Click(object sender, EventArgs e)
        {
            int newScroll = panelCoverAlbums.HorizontalScroll.Value - 400;
            if (newScroll < 0) newScroll = 0;
            panelCoverAlbums.AutoScrollPosition = new Point(newScroll, 0);
        }

        private void btnScrollRightAlbum_Click(object sender, EventArgs e)
        {
            int maxScroll = panelCoverAlbums.HorizontalScroll.Maximum;
            int newScroll = panelCoverAlbums.HorizontalScroll.Value + 400;
            if (newScroll > maxScroll) newScroll = maxScroll;
            panelCoverAlbums.AutoScrollPosition = new Point(newScroll, 0);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = false;           
            //panelSetting.Visible = true;
            //panelSetting.Dock = DockStyle.Fill;
            guna2Transition1.ShowSync(panelSetting);
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = true;
            panelSetting.Visible = false;
            guna2Transition1.HideSync(panelSetting);
            guna2Transition1.ShowSync(mainButton);
        }      

        private void mainButton_Click(object sender, EventArgs e)
        {
            changeInfo.Visible = !changeInfo.Visible;
        }
    }
}

