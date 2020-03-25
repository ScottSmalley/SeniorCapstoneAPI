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
    /// Returns the amount of hours accumulated for a specific person.
    /// </summary>
    class CommandGetWeeklyHour : ActionCommand
    {
        public CommandGetWeeklyHour(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                //List<string> result = new List<string>();
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    DateTime dateToCheck = DateTime.Parse(requestBody["date"][0]);
                    //Get the hours a person has worked for a specified work week.
                    string sql = "SELECT hours_worked FROM flexpooldb.work_history " +
                                 "WHERE emp_id = " + requestBody["emp_id"][0] + " AND " +
                                 "week_id = (SELECT week_id FROM flexpooldb.calendar_year " +
                                    "WHERE start_date <= \"" + dateToCheck.ToString("yyyy-MM-dd HH:mm:ss") + 
                                    "\" AND end_date >= \"" + dateToCheck.ToString("yyyy-MM-dd HH:mm:ss") + "\");";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmdWorkHours = new MySqlCommand(sql, conn))
                    {
                        object result = cmdWorkHours.ExecuteScalar();
                        int hoursAccumulated = Convert.ToInt32(result);
                        responseData.Add($"{requestBody["emp_id"][0]}", new string[] { $"{ hoursAccumulated }" });
                        return responseData;
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
