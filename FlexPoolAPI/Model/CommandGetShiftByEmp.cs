/*
 * Returns all the existing shifts 
 * that a specific employee has signed
 * up for.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetShiftByEmp : ActionCommand
    {
        public CommandGetShiftByEmp(Action newAction) : base(newAction) { }
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
                    string sql = "SELECT shifts.shift_id, shifts.date, shifts.duration, shifts.num_worker, shifts.max_worker, shifts.mgr_id, shifts.dept_name, shifts.skill_name " +
                                 "FROM(SELECT flexpooldb.assigned_shift.emp_id, flexpooldb.shift.shift_id, date, duration, num_worker, max_worker, mgr_id, flexpooldb.dept_type.dept_name, flexpooldb.skill.skill_name " +
                                 "FROM flexpooldb.shift " +
                                 "INNER JOIN flexpooldb.assigned_shift " +
                                 "ON flexpooldb.assigned_shift.shift_id = flexpooldb.shift.shift_id " +
                                 "INNER JOIN flexpooldb.skill " +
                                 "ON flexpooldb.shift.skill_id = flexpooldb.skill.skill_id " +
                                 "INNER JOIN flexpooldb.dept_type " +
                                 "ON flexpooldb.shift.dept_id = flexpooldb.dept_type.dept_id) AS shifts " +
                                 "WHERE shifts.emp_id = " + requestBody["emp_id"][0] + ";";

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
                                    dept_name = (string)rdr[6],
                                    skill_name = (string)rdr[7]
                                };
                                responseData.Add($"assigned shift_id {newShift.shift_id}", new string[] { JsonConvert.SerializeObject(newShift) });
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
