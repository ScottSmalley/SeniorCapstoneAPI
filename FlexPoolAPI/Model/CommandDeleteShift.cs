﻿/*
 * Deletes a Shift record in the
 * database. 
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandDeleteShift : ActionCommand
    {
        public CommandDeleteShift(Action newAction) : base(newAction) { }
        
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Will revisit later to check other tables to clean them of data of this shift. 
                    string sql = "DELETE FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";

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
                Console.WriteLine("Couldn't find key in dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary" });
                return responseData;
            }
        }
    }
}