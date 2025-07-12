//server
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Text.Json;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json.Serialization;
namespace VibraSoundServer
{
    public partial class Server : Form
    {
        static TcpListener listener;
        static string connectionString = "Server=localhost;Database=VibraSound;Integrated Security=True";
        public static Server instance;
        static Dictionary<string, List<TcpClient>> roomClients = new Dictionary<string, List<TcpClient>>();

        public Server()
        {
            InitializeComponent();
            instance = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Server.instance.Log("Server is running on port 9000...");

            // Tạo thread mới để lắng nghe client, không block UI thread
            Thread listenerThread = new Thread(() =>
            {
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Thread t = new Thread(HandleClient);
                    t.Start(client);
                }
            });

            listenerThread.IsBackground = true; 
            listenerThread.Start();
        }
        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Server.instance.Log("Received JSON: " + request); //
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var message = JsonSerializer.Deserialize<RequestMessage>(request, options);
                    if (message?.Type == "video_state")
                    {
                        HandleVideoState(request, message.RoomID, client); // dùng rawJson = request
                        continue; // không phản hồi gì thêm
                    }
                    else if (message?.Type == "play_media")
                    {
                        HandlePlayMedia(request, message.RoomID, client);
                        continue;
                    }
                    else if (message?.Type == "room_chat_message")
                    {
                        BroadcastChatMessage(request, message.RoomID, client); // 👈 dùng raw JSON
                        continue;
                    }


                    Server.instance.Log("Parsed Type: " + message?.Type); // 👈 in ra Type
                    string response = HandleRequest(message, client);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }

                catch
                {
                    break;
                }

            }
            client.Close();
        }

        static void BroadcastChatMessage(string rawJson, string roomID, TcpClient sender)
        {
            if (!roomClients.ContainsKey(roomID)) return;

            foreach (var client in roomClients[roomID])
            {
                try
                {
                    var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(rawJson);
                    stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Server.instance.Log("Lỗi gửi chat: " + ex.Message);
                }
            }

            Server.instance.Log($"Broadcast chat trong phòng {roomID}: {rawJson}");
        }


        static void HandlePlayMedia(string rawJson, string roomID, TcpClient sender)
        {
            if (!roomClients.ContainsKey(roomID)) return;

            foreach (var client in roomClients[roomID])
            {
                if (client != sender) // không gửi lại cho người gửi
                {
                    try
                    {
                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(rawJson);
                        stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Server.instance.Log("Lỗi gửi play_media: " + ex.Message);
                    }
                }
            }

            Server.instance.Log("Broadcast play_media đến các client trong phòng " + roomID);
        }

        static void HandleVideoState(string rawJson, string roomID, TcpClient sender)
        {
            if (!roomClients.ContainsKey(roomID)) return;

            foreach (var client in roomClients[roomID])
            {
                if (client != sender)
                {
                    try
                    {
                        var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                        writer.WriteLine(rawJson); // 👈 Broadcast toàn bộ JSON nguyên gốc
                    }
                    catch (Exception ex)
                    {
                        Server.instance.Log("Lỗi broadcast video_state: " + ex.Message);
                    }
                }
            }

            Server.instance.Log("Broadcast video_state (play/pause/seek) đến các client trong phòng " + roomID);
        }
        static string HandleRequest(RequestMessage msg, TcpClient client = null)
        {
            switch (msg.Type)
            {
                case "signup":
                    return Signup(msg.Username, msg.Password, msg.Email);
                case "login":
                    return Login(msg.Username, msg.Password);
                case "check_email_exists":
                    return CheckEmailExists(msg.Email);
                case "get_user_info":
                    return GetUserInfo(msg.Username);
                case "update_avatar":
                    return UpdateAvatar(msg.Username, msg.Avatar);
                case "update_nameinapp":
                    return UpdateNameInApp(msg.Username, msg.NameInApp);
                case "changepassword":
                    return ChangePassword(msg.Email, msg.Password);
                case "create_room":
                    return CreateRoom(msg, client);
                case "join_room":
                    return JoinRoom(msg, client);
                default:
                    return JsonSerializer.Serialize(new { status = "invalid_request" });
            }
        }

        static string JoinRoom(RequestMessage msg, TcpClient client)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    Server.instance.Log("JoinRoom: Opened DB with RoomID = " + msg.RoomID + " - NameInApp = " + msg.NameInApp);

                    // Lấy thông tin phòng
                    var cmd = new SqlCommand("SELECT * FROM Room WHERE RoomID = @id", conn);
                    cmd.Parameters.AddWithValue("@id", msg.RoomID);
                    string roomName = null;
                    bool isPrivate = false;
                    string password = null;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Server.instance.Log("Room not found: " + msg.RoomID);
                            return JsonSerializer.Serialize(new { status = "room_not_found" });
                        }

                        roomName = reader["RoomName"].ToString();
                        isPrivate = (bool)reader["IsPrivate"];
                        password = reader["MatKhau"]?.ToString();
                    }

                    if (isPrivate)
                    {
                        if (string.IsNullOrEmpty(msg.RoomPassword) || msg.RoomPassword.Trim() != password?.Trim())
                        {
                            Server.instance.Log("Wrong password for room " + msg.RoomID);
                            return JsonSerializer.Serialize(new { status = "wrong_password" });
                        }
                    }

                    // Thêm user vào RoomMembers nếu chưa có
                    var checkCmd = new SqlCommand("SELECT COUNT(*) FROM RoomMembers WHERE RoomID = @id AND Username = @u", conn);
                    checkCmd.Parameters.AddWithValue("@id", msg.RoomID);
                    checkCmd.Parameters.AddWithValue("@u", msg.NameInApp);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        // Tìm slot IDTrongPhong còn trống trong phòng (từ 2 đến 5)
                        var usedSlotsCmd = new SqlCommand("SELECT IDTrongPhong FROM RoomMembers WHERE RoomID = @id", conn);
                        usedSlotsCmd.Parameters.AddWithValue("@id", msg.RoomID);
                        var usedSlots = new HashSet<int>();
                        using (var reader = usedSlotsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["IDTrongPhong"] != DBNull.Value)
                                    usedSlots.Add(Convert.ToInt32(reader["IDTrongPhong"]));
                            }
                        }

                        int newSlot = 2; // Host là slot 1
                        while (usedSlots.Contains(newSlot)) newSlot++;

                        var insert = new SqlCommand("INSERT INTO RoomMembers (RoomID, Username, IDTrongPhong) VALUES (@id, @u, @slot)", conn);
                        insert.Parameters.AddWithValue("@id", msg.RoomID);
                        insert.Parameters.AddWithValue("@u", msg.NameInApp);
                        insert.Parameters.AddWithValue("@slot", newSlot);
                        insert.ExecuteNonQuery();
                    }
                    else
                    {
                        Server.instance.Log($"User {msg.NameInApp} đã có trong RoomMembers.");
                    }

                    // Tăng số lượng người trong phòng (kể cả host)
                    var updateCmd = new SqlCommand("UPDATE Room SET MemberCount = MemberCount + 1 WHERE RoomID = @id", conn);
                    updateCmd.Parameters.AddWithValue("@id", msg.RoomID);
                    updateCmd.ExecuteNonQuery();

                    // Lấy lại MemberCount
                    int memberCount = 1;
                    var countCmd = new SqlCommand("SELECT MemberCount FROM Room WHERE RoomID = @id", conn);
                    countCmd.Parameters.AddWithValue("@id", msg.RoomID);
                    memberCount = (int)countCmd.ExecuteScalar();

                    // Gửi "join_success" để client biết đã join được
                    var stream = client.GetStream();
                    var successResponse = JsonSerializer.Serialize(new
                    {
                        status = "join_success",
                        NameInApp = msg.NameInApp
                    });
                    byte[] successBytes = Encoding.UTF8.GetBytes(successResponse);
                    stream.Write(successBytes, 0, successBytes.Length);
                    Server.instance.Log("Sent join_success to client");

                    // Lấy HostID và Avatar
                    string hostName = null;
                    string hostAvatar = null;
                    var hostCmd = new SqlCommand(@"
                    SELECT r.HostID, u.Avatar 
                    FROM Room r 
                    JOIN [User] u ON r.HostID = u.NameInApp
                    WHERE r.RoomID = @id", conn);
                    hostCmd.Parameters.AddWithValue("@id", msg.RoomID);

                    using (var reader = hostCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            hostName = reader["HostID"].ToString();
                            hostAvatar = reader["Avatar"]?.ToString();
                        }
                    }

                    // Lấy danh sách tất cả thành viên (bao gồm host)
                    var membersCmd = new SqlCommand(@"
                    SELECT u.NameInApp, u.Avatar, rm.IDTrongPhong 
                    FROM RoomMembers rm 
                    JOIN [User] u ON rm.Username = u.NameInApp 
                    WHERE rm.RoomID = @id", conn);

                    membersCmd.Parameters.AddWithValue("@id", msg.RoomID);
                    membersCmd.Parameters.AddWithValue("@host", hostName);

                    List<object> members = new List<object>();
                    using (var reader = membersCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            members.Add(new
                            {
                                name = reader["NameInApp"].ToString(),
                                avatar = reader["Avatar"]?.ToString(),
                                slot = Convert.ToInt32(reader["IDTrongPhong"]) // slot đã JOIN sẵn
                            });
                        }
                    }
                    // Bắt buộc thêm host vào danh sách (slot 1)
                    members.Add(new
                    {
                        name = hostName,
                        avatar = hostAvatar,
                        slot = 1
                    });
                    members = members.OrderBy(m => ((dynamic)m).slot).ToList();

                    if (!roomClients.ContainsKey(msg.RoomID))
                        roomClients[msg.RoomID] = new List<TcpClient>();

                    if (!roomClients[msg.RoomID].Contains(client))
                        roomClients[msg.RoomID].Add(client);

                    var memberList = JsonSerializer.Serialize(new
                    {
                        type = "room_member_list",
                        roomName = roomName,            
                        hostname = hostName,
                        avthostname = hostAvatar,
                        member = members,
                        membercount = memberCount,
                        NameInApp = msg.NameInApp
                    });
                    byte[] resBytes = Encoding.UTF8.GetBytes(memberList);
                    // Broadcast đến tất cả client trong phòng
                    if (roomClients.ContainsKey(msg.RoomID))
                    {
                        var broadcastClients = roomClients[msg.RoomID].Distinct().ToList();
                        foreach (var c in broadcastClients)
                        {
                            try
                            {
                                var s = c.GetStream();
                                s.Write(resBytes, 0, resBytes.Length);
                            }
                            catch (Exception ex)
                            {
                                Server.instance.Log("Gửi room_member_list bị lỗi: " + ex.Message);
                            }
                        }
                    }
                    Server.instance.Log("Broadcast room_member_list đến tất cả client trong phòng: " + memberList);

                    Server.instance.Log("JoinRoom Success and sent room_member_list: " + memberList);

                    return ""; // đã gửi trực tiếp
                }
            }
            catch (Exception ex)
            {
                Server.instance.Log("JoinRoom ERROR: " + ex.Message);
                return JsonSerializer.Serialize(new { status = "error", message = ex.Message });
            }
        }
        static string CreateRoom(RequestMessage msg, TcpClient client)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string roomID = new Random().Next(1000, 9999).ToString();
                bool isPrivate = !string.IsNullOrEmpty(msg.RoomPassword);

                // Tạo phòng
                var cmd = new SqlCommand("INSERT INTO Room (RoomID, RoomName, HostID, IsPrivate, MatKhau, MemberCount) VALUES (@id, @name, @host, @pri, @pw, 1)", conn);
                cmd.Parameters.AddWithValue("@id", roomID);
                cmd.Parameters.AddWithValue("@name", msg.RoomName);
                cmd.Parameters.AddWithValue("@host", msg.HostUsername);
                cmd.Parameters.AddWithValue("@pri", isPrivate ? 1 : 0);
                cmd.Parameters.AddWithValue("@pw", isPrivate ? msg.RoomPassword : (object)DBNull.Value);
                cmd.ExecuteNonQuery();

                // Thêm host vào RoomMembers với slot 1
                var insert = new SqlCommand("INSERT INTO RoomMembers (RoomID, Username, IDTrongPhong) VALUES (@rid, @u, 1)", conn);
                insert.Parameters.AddWithValue("@rid", roomID);
                insert.Parameters.AddWithValue("@u", msg.HostUsername);
                insert.ExecuteNonQuery();

                if (!roomClients.ContainsKey(roomID))
                    roomClients[roomID] = new List<TcpClient>();

                roomClients[roomID].Add(client);

                // Lấy thông tin host từ bảng User
                string hostName = null, hostAvatar = null;
                var userCmd = new SqlCommand("SELECT NameInApp, Avatar FROM [User] WHERE NameInApp = @n", conn);
                userCmd.Parameters.AddWithValue("@n", msg.HostUsername);

                using (var reader = userCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        hostName = reader["NameInApp"]?.ToString();
                        hostAvatar = reader["Avatar"]?.ToString();
                    }
                }

                // Sau khi lấy hostName và hostAvatar:
                var response = new
                {
                    status = "room_created",
                    roomID = roomID,
                    roomName = msg.RoomName,
                    hostname = hostName,
                    avthostname = hostAvatar,
                    membercount = 1,
                    member = new List<object>
                    {
                        new { name = hostName, avatar = hostAvatar, slot = 1 }
                    }
                };

                var stream = client.GetStream();
                byte[] responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                stream.Write(responseBytes, 0, responseBytes.Length);

                Server.instance.Log("Đã gửi room_created (kèm danh sách thành viên) cho host");

                return ""; // không trả gì nữa vì đã gửi phản hồi
            }
        }
        static string GetUserInfoForJoinRoom(string username)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT NameInApp, Avatar FROM [User] WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string nameInApp = reader["NameInApp"].ToString();
                        string avatar = reader["Avatar"] != DBNull.Value ? reader["Avatar"].ToString() : null;

                        return JsonSerializer.Serialize(new
                        {
                            status = "join_success",
                            nameInApp,
                            avatar
                        });
                    }
                    else
                    {
                        return JsonSerializer.Serialize(new { status = "user_not_found" });
                    }
                }
            }
        }
        static string ChangePassword(string email, string newPassword)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE [User] SET Password = @p WHERE Email = @e", conn);
                cmd.Parameters.AddWithValue("@p", newPassword);
                cmd.Parameters.AddWithValue("@e", email);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    Server.instance.Log($"Password changed for email: {email}");
                    return "change_success";
                }
                else
                {
                    Server.instance.Log($"Failed to change password for email: {email}");
                    return "change_failed";
                }
            }
        }
        static string UpdateNameInApp(string username, string newName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE [User] SET NameInApp = @n WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@n", newName);
                cmd.Parameters.AddWithValue("@u", username);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Server.instance.Log($"Updated NameInApp for {username} to {newName}");
                    return JsonSerializer.Serialize(new { status = "update_success" });
                }
                else
                {
                    Server.instance.Log($"Failed to update NameInApp for {username}");
                    return JsonSerializer.Serialize(new { status = "update_failed" });
                }
            }
        }
        static string UpdateAvatar(string username, string avatarPath)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE [User] SET Avatar = @a WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@a", avatarPath);
                cmd.Parameters.AddWithValue("@u", username);

                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    Server.instance.Log($"Avatar updated for {username}");
                    return JsonSerializer.Serialize(new { status = "update_success" });
                }
                else
                {
                    return JsonSerializer.Serialize(new { status = "update_failed" });
                }
            }
        }
        static string GetUserInfo(string username)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Email, Password, NameInApp, Username, Avatar FROM [User] WHERE Username = @u", conn);
                cmd.Parameters.AddWithValue("@u", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var email = reader.GetString(0);
                        var password = reader.GetString(1);
                        var nameInApp = reader.GetString(2);
                        
                        string avatar = reader.IsDBNull(4) ? null : reader.GetString(4);


                        Server.instance.Log($"Fetched info for {username}");

                        return JsonSerializer.Serialize(new
                        {
                            status = "success",
                            username = username,
                            email = email,
                            password = password,
                            nameInApp = nameInApp,
                            avatar = avatar
                        });
                    }
                    else
                    {
                        Server.instance.Log($"User not found: {username}");
                        return JsonSerializer.Serialize(new { status = "user_not_found" });
                    }
                }
            }
        }
        static string CheckEmailExists(string email)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE Email = @e", conn);
                cmd.Parameters.AddWithValue("@e", email);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    Server.instance.Log($"Email check: {email} exists");
                    return "email_exists";
                }
                else
                {
                    Server.instance.Log($"Email check: {email} available");
                    return "email_available";
                }
            }
        }
        static string Signup(string username, string password, string email)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string nameInApp = "user" + new Random().Next(100000, 1000000);

                var cmd = new SqlCommand("INSERT INTO [User] (Username, Password, Email, NameInApp) VALUES (@u, @p, @e, @n)", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@n", nameInApp);

                try
                {
                    cmd.ExecuteNonQuery();
                    Server.instance.Log($"Signup success: {username} with NameInApp: {nameInApp}");
                    return "signup_success";
                }
                catch
                {
                    Server.instance.Log($"Signup failed: {username}");
                    return "signup_failed";
                }
            }
        }
        static string Login(string username, string password)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE Username=@u AND Password=@p", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                {
                    Server.instance.Log($"Login success: {username}");
                    return "login_success";
                }
                else
                {
                    Server.instance.Log($"Login failed: {username}");
                    return "login_failed";
                }
            }
        }
        public class RequestMessage
        {
            [JsonPropertyName("Type")]
            public string Type { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }

            [JsonPropertyName("NameInApp")]
            public string NameInApp { get; set; }
            public string Avatar { get; set; }

            [JsonPropertyName("RoomID")]
            public string RoomID { get; set; }

            [JsonPropertyName("RoomPassword")]
            public string RoomPassword {  get; set; }
            public string RoomName { get; set; }
            public string HostUsername { get; set; }
            public string Action { get; set; }  // play, pause, seek, rewind, forward
            public double? Position { get; set; }  // current time position

            public string Sender { get; set; }
            public string Content { get; set; }
            public string Timestamp { get; set; }
        }
        public void Log(string message)
        {
            if (rtxtInfo.InvokeRequired)
            {
                rtxtInfo.Invoke(new Action(() => {
                    rtxtInfo.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
                }));
            }
            else
            {
                rtxtInfo.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            }
        }

        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(   );
        }
    }
}

