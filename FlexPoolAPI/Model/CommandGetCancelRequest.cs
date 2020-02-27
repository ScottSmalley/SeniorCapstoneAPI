/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Gets all the shift cancellation requests from the database.
    /// </summary>
    class CommandGetCancelRequest : ActionCommand
    {
        public CommandGetCancelRequest(Action newAction): base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                List<Message> result = new List<Message>();
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Get the cancellation requests based on the manager sent.
                    string sql = "SELECT flexpooldb.cancel_shift_request.shift_id, emp_id, text, reviewed, is_approved FROM flexpooldb.cancel_shift_request " +
                                 "INNER JOIN(SELECT shift_id FROM flexpooldb.shift " +
                                 "WHERE mgr_id = " + requestBody["mgr_id"][0] + ") AS shifts " +
                                 "WHERE flexpooldb.cancel_shift_request.shift_id = shifts.shift_id;";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            int requestCtr = 0;
                            while (rdr.Read())
                            {
                                CancelRequest cancelRequest = new CancelRequest()
                                {
                                    shift_id = (int)rdr[0],
                                    emp_id = (int)rdr[1],
                                    text = (string)rdr[2],
                                    reviewed = (bool)rdr[3],
                                    is_approved = (bool)rdr[4]
                                };
                                responseData.Add($"Request {requestCtr++}", new string[] { JsonConvert.SerializeObject(cancelRequest) });
                            }
                            return responseData;
                        }
                    }
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing item in dictionary.");
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
