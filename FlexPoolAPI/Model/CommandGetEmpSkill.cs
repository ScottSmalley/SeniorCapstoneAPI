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
    /// Returns all the skills assigned to a specific person.
    /// </summary>
    class CommandGetEmpSkill : ActionCommand
    {
        public CommandGetEmpSkill(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                List<string> result = new List<string>();
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Get all the skills assigned with a specific employee.
                    string sql = "SELECT skill_name FROM flexpooldb.skill AS skill " +
                        "INNER JOIN (SELECT skill_id FROM flexpooldb.assigned_skill WHERE emp_id = " + requestBody["emp_id"][0] + ") AS assigned_skill " +
                        "ON skill.skill_id = assigned_skill.skill_id;";

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
                                result.Add((string)rdr[0]);
                            }
                            responseData.Add("skill", result.ToArray());
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
