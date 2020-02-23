/*
 * Creates a record in the DB that
 * an employee signed up for a shift.
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
                    if (inDebugMode)
                    {
                        //Insert a record into assignment table to complete the command.
                        string sql = "INSERT INTO flexpooldb.assigned_shift " +
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
                    else
                    {
                        //Check to see if this employee is frozen.
                        string sql = "SELECT is_frozen FROM flexpooldb.person " +
                                     "WHERE emp_id = " + requestBody["emp_id"][0] + ";";
                        using (MySqlCommand cmdFrozen = new MySqlCommand(sql, conn))
                        {
                            object resultFrozen = cmdFrozen.ExecuteScalar();
                            int empFrozen = Convert.ToInt32(resultFrozen);
                            //If they're a 1 or greater, means they're frozen and to return a failure message.
                            if (empFrozen > 0)
                            {
                                Console.WriteLine("FAILED: employee frozen");
                                responseData.Add("response", new string[] { "failure" });
                                responseData.Add("reason", new string[] { "frozen person" });
                                return responseData;
                            }
                        }
                        //Check to see if this employee has the room in their weekly_hours.
                        sql = "SELECT ((weekly_cap - weekly_hours) - (SELECT (duration / 60) FROM flexpooldb.shift " +
                                     "WHERE shift_id = " + requestBody["shift_id"][0] + ")) FROM flexpooldb.person " +
                                     "WHERE emp_id = " + requestBody["emp_id"][0] + ";";

                        Console.WriteLine(sql);
                        using (MySqlCommand cmdHours = new MySqlCommand(sql, conn))
                        {
                            object resultHours = cmdHours.ExecuteScalar();
                            int empWeeklyHoursLeft = Convert.ToInt32(resultHours);
                            if (empWeeklyHoursLeft >= 0)
                            {
                                //Check to see if this employee has the skill to sign up for this shift.
                                sql = "SELECT COUNT(*) FROM flexpooldb.assigned_skill " +
                                      "WHERE emp_id = " + requestBody["emp_id"][0] + " AND " +
                                      "skill_id = (SELECT skill_id FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ");";
                                Console.WriteLine(sql);
                                using (MySqlCommand cmdSkill = new MySqlCommand(sql, conn))
                                {
                                    object resultSkill = cmdSkill.ExecuteScalar();
                                    int empSkillFound = Convert.ToInt32(resultSkill);
                                    if (empSkillFound > 0)
                                    {
                                        //Check to see if there's room on this shift.
                                        sql = "SELECT (max_worker - num_worker) " +
                                              "FROM flexpooldb.shift " +
                                              "WHERE shift_id = " + requestBody["shift_id"][0] + ";";

                                        Console.WriteLine(sql);
                                        //Make a Command Object to then execute.
                                        using (MySqlCommand cmdShift = new MySqlCommand(sql, conn))
                                        {
                                            object result = cmdShift.ExecuteScalar();
                                            int shiftSpotsRemaining = Convert.ToInt32(result);

                                            //If there's 1 or more open positions
                                            if (shiftSpotsRemaining > 0)
                                            {
                                                //Insert a record into assignment table to complete the command.
                                                sql = "INSERT INTO flexpooldb.assigned_shift " +
                                                      "VALUES(" + requestBody["shift_id"][0] + ", " + requestBody["emp_id"][0] + ");";

                                                Console.WriteLine(sql);
                                                //Make a Command Object to then execute.
                                                using (MySqlCommand cmdInsert = new MySqlCommand(sql, conn))
                                                {
                                                    cmdInsert.ExecuteNonQuery();
                                                    //responseData.Add("response", new string[] { "success" });
                                                    //return responseData;
                                                }

                                                //Increment the counter
                                                sql = "UPDATE flexpooldb.shift " +
                                                      "SET num_worker = num_worker + 1 " +
                                                      "WHERE shift_id = " + requestBody["shift_id"][0] + ";";

                                                Console.WriteLine(sql);
                                                //Make a Command Object to then execute.
                                                using (MySqlCommand cmdInsertWorkerCount = new MySqlCommand(sql, conn))
                                                {
                                                    cmdInsertWorkerCount.ExecuteNonQuery();
                                                }

                                                //Increment the person's hours.
                                                sql = "UPDATE flexpooldb.person " +
                                                      "SET weekly_hours = weekly_hours + " +
                                                      "((SELECT duration FROM flexpooldb.shift WHERE shift_id = " + requestBody["shift_id"][0] + ") / 60) " +
                                                      "WHERE emp_id = " + requestBody["emp_id"][0] + ";";

                                                Console.WriteLine(sql);
                                                using (MySqlCommand cmdInsertHours = new MySqlCommand(sql, conn))
                                                {
                                                    cmdInsertHours.ExecuteNonQuery();
                                                }
                                                responseData.Add("response", new string[] { "success" });
                                                return responseData;
                                            }
                                            //If the number of positions available was 0 or less, 
                                            //return a failure message to let them know this shift is full.
                                            else
                                            {
                                                Console.WriteLine("FAILED: full shift");
                                                responseData.Add("response", new string[] { "failure" });
                                                responseData.Add("reason", new string[] { "full shift" });
                                                return responseData;
                                            }
                                        }
                                    }
                                    //If the employee doesn't have the skill for this shift,
                                    //return a failure message to let them know this employee doesn't have the skill.
                                    else
                                    {
                                        Console.WriteLine("FAILED: employee lacks the assigned skill.");
                                        responseData.Add("response", new string[] { "failure" });
                                        responseData.Add("reason", new string[] { "person missing skill" });
                                        return responseData;
                                    }
                                }
                            }
                            //If the person doesn't have enough hours in their weekly_cap, 
                            //return a failure message to let them know this person doesn't
                            //have enough weekly_hours.
                            else
                            {
                                Console.WriteLine("FAILED: not enough weekly_hours");
                                responseData.Add("response", new string[] { "failure" });
                                responseData.Add("reason", new string[] { "person lacks weekly hours" });
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
            catch (Exception e)
            {
                if (e.Message.Contains("Duplicate entry"))
                {
                    Console.WriteLine("ERROR: duplicate key");
                    responseData.Add("response", new string[] { "failure" });
                    responseData.Add("reason", new string[] { "duplicate entry in the database." });
                    return responseData;
                }
                else
                {
                    Console.WriteLine("ERROR: there was a problem executing the action.");
                    responseData.Add("response", new string[] { "failure" });
                    responseData.Add("reason", new string[] { "unspecified problem assigning skill." });
                    return responseData;
                }
            }
        }
    }
}
