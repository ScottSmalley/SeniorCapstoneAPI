﻿/*
 * Creates a message and inserts it into the database.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandSendMessage : ActionCommand
    {
        public CommandSendMessage(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    string sql = "INSERT INTO flexpooldb.message (sender_id, receiver_id, date_sent, msg_text)" +
                                " VALUES(" + requestBody["sender_id"][0] + ", " + requestBody["receiver_id"][0] + 
                                ", \"" + newAction.GetDateTimeUTC() + "\", \"" + requestBody["msg_text"][0] + "\");"; 

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        responseData.Add("response", new string[] { "success" });
                        return responseData;
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing key in the dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });

                return responseData;
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: there was a problem executing the action.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "unspecified problem generating message." });
                return responseData;
            }
        }
    }
}