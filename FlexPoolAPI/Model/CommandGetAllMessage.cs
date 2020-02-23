/*
 * Gets all the messages that are sent by a specific user.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetAllMessage : ActionCommand
    {
        public CommandGetAllMessage(Action newAction) : base(newAction) { }
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
                    //Select all the messages that I sent, and include the Receiver's name in the results.
                    string sql = "SELECT msg_id, receiver_id, (SELECT name FROM flexpooldb.person WHERE emp_id = receiver_id), date_sent, msg_text " +
                                 "FROM flexpooldb.message " +
                                 "WHERE sender_id = " + requestBody["emp_id"][0] + ";";
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
                                    receiver_id = (int)rdr[1],
                                    receiver_name = (string)rdr[2],
                                    date_sent = (DateTime)rdr[3],
                                    msg_text = (string)rdr[4]
                                };
                                responseData.Add($"Sent Message {newMsg.msg_id}", new string[] { JsonConvert.SerializeObject(newMsg) });
                            }
                            //return responseData;
                        }
                    }
                    //Select all the messages I received, and include the Sender's name in the results.
                    sql = "SELECT msg_id, sender_id, (SELECT name FROM flexpooldb.person WHERE emp_id = sender_id), date_sent, msg_text " +
                          "FROM flexpooldb.message " +
                          "WHERE receiver_id = " + requestBody["emp_id"][0] + ";";
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
                                    sender_name = (string)rdr[2],
                                    date_sent = (DateTime)rdr[3],
                                    msg_text = (string)rdr[4]
                                };
                                responseData.Add($"Received Message {newMsg.msg_id}", new string[] { JsonConvert.SerializeObject(newMsg) });
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
