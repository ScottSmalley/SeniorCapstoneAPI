/*
 * Deletes a survey from the database.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandDeleteSurvey : ActionCommand
    {
        public CommandDeleteSurvey(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    
                    //Create a storage record for the survey.
                    string sql = "INSERT INTO flexpooldb.shift_survey_storage (emp_id, shift_id, mgr_id, rating, text) " +
                                 "SELECT emp_id, shift_id, mgr_id, rating, text FROM flexpooldb.shift_survey " +
                                 "WHERE shift_id = " + requestBody["shift_id"][0] + " AND emp_id = " + requestBody["emp_id"][0] + ";";
                    Console.WriteLine(sql);
                    using (MySqlCommand cmdMigrateShift = new MySqlCommand(sql, conn))
                    {
                        cmdMigrateShift.ExecuteNonQuery();
                    }

                    //Delete survey record from shift_survey.
                    sql = "DELETE FROM flexpooldb.shift_survey " +
                          "WHERE shift_id = " + requestBody["shift_id"][0] + " AND emp_id = " + requestBody["emp_id"][0] + ";";
                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmdDeleteSurvey = new MySqlCommand(sql, conn))
                    {
                        cmdDeleteSurvey.ExecuteNonQuery();
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
