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
    /// Deletes a shift assignment record for a specified 
    /// person and shift, and migrates the record to 
    /// a storage table. Also reduce the number of
    /// signed up people from the shift.
    /// </summary>
    class CommandRemoveShift : ActionCommand
    {
        public CommandRemoveShift(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //If in debug mode, just delete the assignment, regardless of any conditions.
                    if (inDebugMode)
                    {
                        //Delete the record assigning the shift to the employee.
                        string sql = "DELETE FROM flexpooldb.assigned_shift " +
                                     "WHERE shift_id = " + requestBody["shift_id"][0] + " " +
                                     "AND emp_id = " + requestBody["emp_id"][0] + ";";

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
                    else
                    {
                        //Decrement the number of employees assigned to the shift.
                        string sql = "UPDATE flexpooldb.shift " +
                                     "SET num_worker = num_worker - 1 " +
                                     "WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        
                        if (inDevMode)
                        {
                            Console.WriteLine(sql);
                        }
                        using (MySqlCommand cmdIncrement = new MySqlCommand(sql, conn))
                        {
                            cmdIncrement.ExecuteNonQuery();
                        }

                        //Remove the hours associated with the shift assignment we're removing.
                        sql = "UPDATE flexpooldb.work_history " +
                              "SET hours_worked = hours_worked - " +
                              "((SELECT duration FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ") / 60) " +
                              "WHERE emp_id = " + requestBody["emp_id"][0] + " " +
                              "AND week_id = " +
                              "(SELECT week_id FROM flexpooldb.calendar_year " +
                                  "WHERE start_date <= " +
                                    "(SELECT date FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ") " +
                                  "AND end_date >= " +
                                    "(SELECT date FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + "));";
                        using (MySqlCommand cmdAdjustWorkHours = new MySqlCommand(sql, conn))
                        {
                            cmdAdjustWorkHours.ExecuteNonQuery();
                        }

                        //Migrate the assigned_shift records over to the storage table.
                        sql = "REPLACE INTO flexpooldb.assigned_shift_storage (shift_id, emp_id, was_completed) " +
                              "SELECT shift_id, emp_id, 0 FROM flexpooldb.assigned_shift " +
                              "WHERE shift_id = " + requestBody["shift_id"][0] + ";";
                        using (MySqlCommand cmdMigrateShifts = new MySqlCommand(sql, conn))
                        {
                            cmdMigrateShifts.ExecuteNonQuery();
                        }

                        //Delete the record assigning the shift to the employee.
                        sql = "DELETE FROM flexpooldb.assigned_shift " +
                              "WHERE shift_id = " + requestBody["shift_id"][0] + " " +
                              "AND emp_id = " + requestBody["emp_id"][0] + ";";
                        
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
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing key in the dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
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
