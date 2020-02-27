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
    /// Deletes a skill record from the database.
    /// </summary>
    class CommandDeleteSkill : ActionCommand
    {
        public CommandDeleteSkill(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Will revisit later to check other tables to clean them of data of this skill. 
                    string sql = "DELETE FROM flexpooldb.skill WHERE skill_name = \"" + requestBody["skill"][0] + "\";";

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
                Console.WriteLine("missing item in the dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary" });
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
