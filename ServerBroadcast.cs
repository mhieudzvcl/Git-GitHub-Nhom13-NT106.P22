using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login_or_Signup
{
    public class ServerBroadcast
    {
        //public string type { get; set; }
        //public string hostname { get; set; }
        //public string avthostname { get; set; }

        //public string roomName { get; set; }
        //public List<Member> member { get; set; }
        //public int membercount { get; set; }
        //public class Member
        //{
        //    public string name { get; set; }
        //    public string avatar { get; set; }

        //    public int slot { get; set; }
        //}
            public string type { get; set; }
            public string hostname { get; set; }
            public string avthostname { get; set; }
            public string roomName { get; set; }
            public string roomID { get; set; }
            public string videoPath { get; set; }
            public string startTime { get; set; }
            public int membercount { get; set; }
            public List<Member> member { get; set; }
            public long seek { get; set;}
            public string action { get; set; } // "play", "pause", "stop", "seek"

            public string NameInApp { get; set; } // Tên người dùng trong ứng dụng

            public string currentRoomID { get; set; } // ID của phòng hiện tại

            public string title { get; set; }         // Thêm
            public string description { get; set; }   // Thêm
            public string coverImageUrl { get; set; } // Thêm

            public string sender { get; set; }
            public string content { get; set; }
            public string timestamp { get; set; }


        public class Member
            {
                public string name { get; set; }
                public string avatar { get; set; }
                public int slot { get; set; }
            }
    }
    public class ChatMessage
    {
        public string type { get; set; }
        public string sender { get; set; }
        public string message { get; set; }
        public string timestamp { get; set; }
    }

}
