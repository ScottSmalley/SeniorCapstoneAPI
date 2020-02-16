/*
 * Deletes an assignment record in the
 * database for a specific employee and
 * a shift they signed up for previously.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
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
                    
                    //Delete the record assigning the shift to the employee.
                    string sql = "DELETE FROM flexpooldb.assigned_shift " +
                                 "WHERE shift_id = " + requestBody["shift_id"][0] + " " +
                                 "AND emp_id = " + requestBody["emp_id"][0] + ";";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        
                        //Check the database to make sure the deletion was successful.
                        sql = "SELECT COUNT(*) FROM flexpooldb.assigned_shift " +
                            "WHERE shift_id = " + requestBody["shift_id"][0] + " AND emp_id = " + requestBody["emp_id"][0] + ";";

                        Console.WriteLine(sql);
                        //Make a Command Object to then execute.
                        using (MySqlCommand cmdCount = new MySqlCommand(sql, conn))
                        {
                            object result = cmdCount.ExecuteScalar();
                            int wasDeleted = Convert.ToInt32(result);
                            //If the record still exists, send a failure back.
                            if (wasDeleted > 0)
                            {
                                Console.WriteLine("ERROR: internal server error");
                                responseData.Add("response", new string[] { "failure" });
                                responseData.Add("reason", new string[] { "internal database error." });
                                return responseData;
                            }
                            //If the record is gone, decrement the number of employees assigned to the shift.
                            sql = "UPDATE flexpooldb.shift " +
                                  "SET num_worker = num_worker - 1 " +
                                  "WHERE shift_id = " + requestBody["shift_id"][0] + ";";

                            Console.WriteLine(sql);
                            //Make a Command Object to then execute.
                            using (MySqlCommand cmdIncrement = new MySqlCommand(sql, conn))
                            {
                                cmdIncrement.ExecuteNonQuery();
                                responseData.Add("response", new string[] { "success" });
                                return responseData;
                            }
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
            catch (Exception)
            {
                Console.WriteLine("ERROR: there was a problem executing the action.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "unspecified problem assigning skill." });
                return responseData;
            }
        }
    }
}
