/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Creates a survey record in the database.
    /// </summary>
    class CommandCreateSurvey : ActionCommand
    {
        public CommandCreateSurvey(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Inserts a shift id, employee id, manager id, a 1-5 rating, and a comment box into the record.
                    string sql = "INSERT INTO flexpooldb.shift_survey  " +
                                  "VALUES(" + requestBody["shift_id"][0] + ", " + requestBody["emp_id"][0] + ", " + 
                                  requestBody["mgr_id"][0] + ", " + requestBody["rating"][0] + ", \"" + requestBody["text"][0] + "\");";
                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
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
