/*
 * Creates a record in the DB that
 * an employee signed up for a shift.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandAssignShift : ActionCommand
    {
        public CommandAssignShift(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Check to see if there's room on this shift.
                    string sql = "SELECT (max_worker - num_worker) " +
                                 "FROM flexpooldb.shift " +
                                 "WHERE shift_id = " + requestBody["shift_id"][0] + ";";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int shiftSpotsRemaining = Convert.ToInt32(result);
                        
                        //If there's 1 or more open positions
                        if (shiftSpotsRemaining > 0)
                        {
                            //Increment the counter
                            sql = "UPDATE flexpooldb.shift " +
                                 "SET num_worker = num_worker + 1 " +
                                 "WHERE shift_id = " + requestBody["shift_id"][0] + ";";

                            Console.WriteLine(sql);
                            //Make a Command Object to then execute.
                            using (MySqlCommand cmdInsert = new MySqlCommand(sql, conn))
                            {
                                cmdInsert.ExecuteNonQuery();
                            }

                            //Insert a record into assignment table to complete the command.
                            sql = "INSERT INTO flexpooldb.assigned_shift " +
                                 "VALUES(" + requestBody["shift_id"][0] + ", " + requestBody["emp_id"][0] + ");";

                            Console.WriteLine(sql);
                            //Make a Command Object to then execute.
                            using (MySqlCommand cmdInsert = new MySqlCommand(sql, conn))
                            {
                                cmdInsert.ExecuteNonQuery();
                                responseData.Add("response", new string[] { "success" });
                                return responseData;
                            }
                            
                        }
                        //If the number of positions available was 0 or less, 
                        //return a failure message to let them know this shift is full.
                        else
                        {
                            Console.WriteLine("FAILED: the shift is full.");
                            responseData.Add("response", new string[] { "failure" });
                            responseData.Add("reason", new string[] { "full shift" });
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
