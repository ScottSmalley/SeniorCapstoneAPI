﻿/*
 * Returns all the existing shifts 
 * that a specific department.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetShiftByDept : ActionCommand
    {
        public CommandGetShiftByDept(Action newAction) : base(newAction) { }
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
                    string sql = "SELECT shift_id, date, duration, num_worker, max_worker, mgr_id, dept_id, skill_name " +
                                 "FROM flexpooldb.shift " +
                                 "INNER JOIN flexpooldb.skill " +
                                 "ON flexpooldb.shift.skill_id = flexpooldb.skill.skill_id AND " +
                                 "flexpooldb.shift.dept_id = (SELECT dept_id FROM flexpooldb.dept_type WHERE dept_name = \"" + requestBody["dept_name"][0] + "\");";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        //Executes the command, and returns the result as an array.
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                Shift newShift = new Shift()
                                {
                                    shift_id = (int)rdr[0],
                                    date = (DateTime)rdr[1],
                                    duration = (int)rdr[2],
                                    num_worker = (int)rdr[3],
                                    max_worker = (int)rdr[4],
                                    mgr_id = (int)rdr[5],
                                    dept_id = (int)rdr[6],
                                    skill_name = (string)rdr[7]
                                };
                                responseData.Add($"shift_id {newShift.shift_id}", new string[] { JsonConvert.SerializeObject(newShift) });
                            }
                            return responseData;
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: get_emp_skills didn't receive a valid emp_id field.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
        }
    }
}
