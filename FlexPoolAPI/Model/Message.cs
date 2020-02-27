/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Represents a message between two people.
    /// </summary>
    class Message
    {
        public int msg_id { get; set; }
        public int sender_id { get; set; }
        public string sender_name { get; set; }
        public int receiver_id { get; set; }
        public string receiver_name { get; set; }
        public DateTime date_sent { get; set; }
        public string msg_text { get; set; }
    }
}
