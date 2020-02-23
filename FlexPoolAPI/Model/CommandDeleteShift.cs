/*
 * Deletes a Shift record in the
 * database. 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandDeleteShift : ActionCommand
    {
        public CommandDeleteShift(Action newAction) : base(newAction) { }
        
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    if (inDebugMode)
                    {
                        //Delete the shift
                        string sql = "DELETE FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        Console.WriteLine(sql);
                        //Make a Command Object to then execute.
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                            responseData.Add("response", new string[] { "success" });
                            return responseData;
                        }
                    }
                    else
                    {
                        // Store emp_ids that we need to update their weekly_hours
                        //since we're deleting this shift (if they're signed up for it).
                        List<int> emp_ids = new List<int>();

                        //Select the emp_ids for the specified shift.
                        string sql = "SELECT emp_id FROM flexpooldb.assigned_shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        Console.WriteLine(sql);
                        using (MySqlCommand cmdAssignedShiftEmps = new MySqlCommand(sql, conn))
                        {
                            using (MySqlDataReader rdr = cmdAssignedShiftEmps.ExecuteReader())
                            {
                                while (rdr.Read())
                                {
                                    emp_ids.Add((int)rdr[0]);
                                }
                            }
                        }
                        if (emp_ids.Count > 0)
                        {
                            int shiftDuration = 0;
                            //Select the duration of the shift so we can remove hours from each employee affected.
                            sql = "SELECT duration FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                            using (MySqlCommand cmdDuration = new MySqlCommand(sql, conn))
                            {
                                object resultDuration = cmdDuration.ExecuteScalar();
                                shiftDuration = Convert.ToInt32(resultDuration);
                            }

                            //Iterate through and remove weekly_hours since this shift is being deleted.
                            foreach (int emp_id in emp_ids)
                            {
                                sql = "UPDATE flexpooldb.person " +
                                    "SET weekly_hours = weekly_hours - ((" + shiftDuration + ")/60) " +
                                    $"WHERE emp_id = {emp_id};";
                                using (MySqlCommand cmdUpdateHours = new MySqlCommand(sql, conn))
                                {
                                    cmdUpdateHours.ExecuteNonQuery();
                                }
                            }

                            //Migrate the assigned_shift records over to the storage table.
                            sql = "INSERT INTO flexpooldb.assigned_shift_storage (shift_id, emp_id, was_completed) " +
                                  "SELECT shift_id, emp_id, 0 FROM flexpooldb.assigned_shift " +
                                  "WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                            using (MySqlCommand cmdMigrateShifts = new MySqlCommand(sql, conn))
                            {
                                cmdMigrateShifts.ExecuteNonQuery();
                            }

                            //Delete the assigned shifts
                            sql = "DELETE FROM flexpooldb.assigned_shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                            Console.WriteLine(sql);
                            //Make a Command Object to then execute.
                            using (MySqlCommand cmdDeleteAssignedShifts = new MySqlCommand(sql, conn))
                            {
                                cmdDeleteAssignedShifts.ExecuteNonQuery();
                            }
                        }

                        //Migrate the shift record over to the storage table.
                        sql = "INSERT INTO flexpooldb.shift_storage (shift_id, date, duration, num_worker, max_worker, mgr_id, dept_id, skill_id, was_completed) " +
                                "SELECT shift_id, date, duration, num_worker, max_worker, mgr_id, dept_id, skill_id, 0 " +
                                "FROM flexpooldb.shift " +
                                "WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        using (MySqlCommand cmdMigrateShift = new MySqlCommand(sql, conn))
                        {
                            cmdMigrateShift.ExecuteNonQuery();
                        }

                        //Delete the shift
                        sql = "DELETE FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        Console.WriteLine(sql);
                        //Make a Command Object to then execute.
                        using (MySqlCommand cmdDeleteShift = new MySqlCommand(sql, conn))
                        {
                            cmdDeleteShift.ExecuteNonQuery();
                            responseData.Add("response", new string[] { "success" });
                            return responseData;
                        }
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
            catch (InvalidCastException)
            {
                Console.WriteLine("ERROR: class cast exception from pulling from the db");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "internal database error." });
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
