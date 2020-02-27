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
    /// Gets all the completed surveys from the database.
    /// </summary>
    class CommandGetAllSurvey : ActionCommand
    {
        public CommandGetAllSurvey(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Get all the surveys from the database.
                    string sql = "SELECT * FROM flexpooldb.shift_survey;";

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
                                Survey newShift = new Survey()
                                {
                                    shift_id = (int)rdr[0],
                                    emp_id = (int)rdr[1],
                                    mgr_id = (int)rdr[2],
                                    rating = (int)rdr[3],
                                    text = (string)rdr[4]
                                };
                                responseData.Add($"shift_id {newShift.shift_id} emp_id {newShift.emp_id}", new string[] { JsonConvert.SerializeObject(newShift) });
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
