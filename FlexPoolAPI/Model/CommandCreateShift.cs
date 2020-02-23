/*
 * Generates a Shift record
 * in the database.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandCreateShift : ActionCommand
    {
        public CommandCreateShift(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    for(int dateIdx = 0; dateIdx < requestBody["date"].Length; dateIdx++)
                    {
                        DateTime requestedDate = DateTime.Parse(requestBody["date"][dateIdx]);
                        string sql = "INSERT INTO flexpooldb.shift (date, duration, max_worker, mgr_id, dept_id, skill_id) " +
                                      "VALUES(\"" + requestedDate.ToString("yyyy-MM-dd HH:mm:ss") + "\", " + requestBody["duration"][0] + ", " + requestBody["max_worker"][0] + ", " + requestBody["mgr_id"][0] + ", " +
                                      "(SELECT dept_id FROM flexpooldb.dept_type WHERE dept_name = \"" + requestBody["dept_name"][0] + "\"), " +
                                      "(SELECT skill_id FROM flexpooldb.skill WHERE skill_name = \"" + requestBody["skill_name"][0] + "\")); ";
                        Console.WriteLine(sql);
                        //Make a Command Object to then execute.
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    responseData.Add("response", new string[] { "success" });
                    return responseData;
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
