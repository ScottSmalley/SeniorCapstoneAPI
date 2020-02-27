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
    /// Verification if an employee id (username) exists,
    /// and if the user sent the correct password assigned to said employee id.
    /// </summary>
    class CommandAuthenticate : ActionCommand
    {
        public CommandAuthenticate(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            
            using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
            {
                conn.Open();
                //Select all the records that have the appropriate emp_id AND password.
                //If it comes back with 0 records, then they have invalid authentication.
                //If it comes back with 1 record, it's authenticated.
                string sql = "SELECT emp_id " +
                                "FROM flexpooldb.person " +
                                "WHERE emp_id = \"" + requestBody["emp_id"][0] + "\" " +
                                "AND password = \"" + requestBody["password"][0] + "\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        try
                        {
                            int emp_id = -9999;
                            //Count how many records we got back.
                            int resultCtr = 0;
                            while (rdr.Read())
                            {
                                emp_id = (int)rdr[0];
                                resultCtr++;
                            }
                            //Double checking that we're only getting 1 result back.
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
                        catch (KeyNotFoundException)
                        {
                            Console.WriteLine("ERROR: missing key in the dictionary.");
                            responseData.Add("response", new string[] { "failure" });
                            responseData.Add("reason", new string[] { "missing item in dictionary." });
                            return responseData;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Console.WriteLine("There was a problem accessing the result from the DB.");
                            responseData.Add("response", new string[] { "failure" });
                            responseData.Add("reason", new string[] { "internal problem selecting login match." });
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
        }
    }
}
