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
    /// Allows a user to edit a person.
    /// Note: They cannot change employee ids.
    /// </summary>
    class CommandEditPerson : ActionCommand
    {
        public CommandEditPerson(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Checks the Dictionary for each field available,
                    //if it exists, add the change to our SQL statement.
                    string sql = "UPDATE flexpooldb.person " +
                                "SET ";
                    /* isFirstField:
                     * 1) Acts as a format helper to get the
                     * commas where they need to go.
                     * 2) Tells me after the loop if there
                     * was any fields in the requestBody.
                     */
                    bool isFirstField = true;
                    //Cycle through Dictionary for fields to update.
                    foreach (string field in requestBody.Keys)
                    {
                        if (!field.Equals("action") && !field.Equals("emp_id"))
                        {
                            //Because weekly_cap is type INT in 
                            //the DB, VARCHAR types require quotes
                            //in the SQL string.
                            if (field.Equals("weekly_cap"))
                            {
                                if (isFirstField)
                                {
                                    sql += field + "=" + requestBody[field][0];
                                    isFirstField = false;
                                }
                                else
                                {
                                    sql += ", " + field + "=" + requestBody[field][0];
                                }
                            }
                            //Have to grab the emp_type_id from the employee_type DB to store
                            //in the person table.
                            else if (field.Equals("emp_type_name"))
                            {
                                if (isFirstField)
                                {
                                    sql += "emp_type = " +
                                        "(SELECT emp_type_id " +
                                        "FROM flexpooldb.employee_type " +
                                        "WHERE emp_type_name = \"" + requestBody[field][0] + "\")";
                                    isFirstField = false;
                                }
                                else
                                {
                                    sql += ", emp_type = " +
                                        "(SELECT emp_type_id " +
                                        "FROM flexpooldb.employee_type " +
                                        "WHERE emp_type_name = \"" + requestBody[field][0] + "\")";
                                }
                            }
                            else
                            {
                                if (isFirstField)
                                {
                                    sql += field + "=\"" + requestBody[field][0] + "\"";
                                    isFirstField = false;
                                }
                                else
                                {
                                    sql += ", " + field + "=\"" + requestBody[field][0] + "\"";
                                }
                            }
                        }
                    }
                    if (!isFirstField)
                    {
                        sql += " WHERE emp_id = " + requestBody["emp_id"][0] + ";";
                    }
                    else
                    {
                        if (inDevMode)
                        {
                            Console.WriteLine(sql);
                        }
                        Console.WriteLine("ERROR: problem with building the sql statement.");
                        responseData.Add("response", new string[] { "failure" });
                        responseData.Add("reason", new string[] { "internal problem editing." });
                        return responseData;
                    }

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
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing key in the dictionary.");
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
