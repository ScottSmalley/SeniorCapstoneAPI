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
    /// Also deletes any assigned skill records 
    /// from the database too.
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

                    //Delete any assigned skill records from the database.
                    string sql = "DELETE FROM flexpooldb.assigned_skill " +
                        "WHERE skill_id = " +
                        "(SELECT skill_id FROM flexpooldb.skill WHERE skill_name = \"" + requestBody["skill"][0] + "\");";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmdDeleteAssignedSkill = new MySqlCommand(sql, conn))
                    {
                        cmdDeleteAssignedSkill.ExecuteNonQuery();
                    }

                    //Delete the skill from the skill database.
                    sql = "DELETE FROM flexpooldb.skill WHERE skill_name = \"" + requestBody["skill"][0] + "\";";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmdDeleteSkill = new MySqlCommand(sql, conn))
                    {
                        cmdDeleteSkill.ExecuteNonQuery();
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
