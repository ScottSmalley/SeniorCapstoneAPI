/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Gets all the sent messages for a specified person.
    /// </summary>
    class CommandGetSentMessage : ActionCommand
    {
        public CommandGetSentMessage(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                List<Message> result = new List<Message>();
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Get all the sent messages for a specified person.
                    string sql = "SELECT msg_id, receiver_id, date_sent, msg_text, sender_read, receiver_read FROM flexpooldb.message " +
                        "WHERE sender_id = " + requestBody["sender_id"][0] + ";";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                Message newMsg = new Message()
                                {
                                    msg_id = (int)rdr[0],
                                    receiver_id = (int)rdr[1],
                                    date_sent = (DateTime)rdr[2],
                                    msg_text = (string)rdr[3],
                                    sender_read = Int32.Parse(rdr[4].ToString()),
                                    receiver_read = Int32.Parse(rdr[5].ToString())
                                };
                                responseData.Add(newMsg.msg_id.ToString(), new string[] { JsonConvert.SerializeObject(newMsg) });
                            }
                            return responseData;
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing item in dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "unspecified problem." });
                return responseData;
            }
        }
    }
}
