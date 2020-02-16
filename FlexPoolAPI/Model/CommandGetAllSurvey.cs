/*
 * Returns all the existing surveys 
 * in the database.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
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
                    string sql = "SELECT * FROM flexpooldb.shift_survey;";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        //Executes the command, and returns the result as an array.
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
                Console.WriteLine("ERROR: get_all_shift missing item in dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
        }
    }
}
