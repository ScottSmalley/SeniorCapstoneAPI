using System;
using System.Collections.Generic;
using System.Text;

namespace FlexPoolAPI.Model
{
    class Message
    {
        public int msg_id { get; set; }
        public int sender_id { get; set; }
        public int receiver_id { get; set; }
        public string date_sent { get; set; }
        public string msg_text { get; set; }
    }
}
