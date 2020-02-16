/*
 * Verifies the emp_id and password
 * given match what's in the database.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandAuthenticate : ActionCommand
    {
        public CommandAuthenticate(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            if (requestBody.ContainsKey("emp_id") && requestBody.ContainsKey("password"))
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    string sql = "SELECT emp_id " +
                                 "FROM flexpooldb.person " +
                                 "WHERE emp_id = \"" + requestBody["emp_id"][0] + "\" " +
                                 "AND password = \"" + requestBody["password"][0] + "\";";
                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        //Executes the command, and returns the result as an array.
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            try
                            {
                                int emp_id = -9999;
                                int resultCtr = 0;
                                while (rdr.Read())
                                {
                                    emp_id = (int)rdr[0];
                                    resultCtr++;
                                }
                                //Double checking for now that we're only getting 1 result back.
                                if (resultCtr <= 1)
                                {
                                    if (emp_id != -9999)
                                    {
                                        responseData.Add("response", new string[] { "success" });
                                    }
                                    else
                                    {
                                        responseData.Add("response", new string[] { "failure" });
                                        responseData.Add("reason", new string[] { "invalid id or password." });
                                    }
                                    return responseData;
                                }
                                else
                                {
                                    Console.WriteLine("There was a problem with the results from the DB.");
                                    responseData.Add("response", new string[] { "failure" });
                                    responseData.Add("reason", new string[] { "internal problem finding login match." });
                                    return responseData;
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                Console.WriteLine("There was a problem accessing the result from the DB.");
                                responseData.Add("response", new string[] { "failure" });
                                responseData.Add("reason", new string[] { "internal problem selecting login match." });
                                return responseData;
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("ERROR: missing action item.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
        }
    }
}
