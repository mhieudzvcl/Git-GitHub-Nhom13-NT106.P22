using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibraSoundServer
{
    internal class ServerBroadcase
    {
        public string type { get; set; }
        public string hostname { get; set; }
        public string avthostname { get; set; }
        public List<Member> member { get; set; }

        public class Member
        {
            public string name { get; set; }
            public string avatar { get; set; }
            public int slot { get; set; }
        }
    }
}
