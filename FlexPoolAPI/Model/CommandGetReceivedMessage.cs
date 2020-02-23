/*
 * Returns all the messages that are received by 
 * a specific user.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetReceivedMessage : ActionCommand
    {
        public CommandGetReceivedMessage(Action newAction) : base(newAction) { }
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
                    string sql = "SELECT msg_id, sender_id, date_sent, msg_text FROM flexpooldb.message " +
                        "WHERE receiver_id = " + requestBody["receiver_id"][0] + ";";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        //Executes the command, and returns the result as an array.
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                Message newMsg = new Message()
                                {
                                    msg_id = (int)rdr[0],
                                    sender_id = (int)rdr[1],
                                    date_sent = (DateTime)rdr[2],
                                    msg_text = (string)rdr[3]
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
                Console.WriteLine("ERROR: get_sent_message missing item in dictionary.");
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
