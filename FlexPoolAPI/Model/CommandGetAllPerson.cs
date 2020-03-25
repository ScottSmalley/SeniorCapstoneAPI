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
    /// Returns all the person records from the database.
    /// </summary>
    class CommandGetAllPerson : ActionCommand
    {
        public CommandGetAllPerson(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Grab all the data related for a specific person.
                    string sql = "SELECT emp_id, name, email, phone_num, weekly_cap, emp_type_name, is_frozen FROM flexpooldb.person " +
                        "LEFT JOIN flexpooldb.employee_type " +
                        "ON flexpooldb.person.emp_type = employee_type.emp_type_id;";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                Person person = new Person()
                                {
                                    emp_id = (int)rdr[0],
                                    name = (string)rdr[1],
                                    email = (string)rdr[2],
                                    phone_num = (string)rdr[3],
                                    weekly_cap = (int)rdr[4],
                                    emp_type_name = (string)rdr[5],
                                    is_frozen = Int32.Parse(rdr[6].ToString())
                                };
                                responseData.Add(person.emp_id.ToString(), new string[] { JsonConvert.SerializeObject(person) });
                            }
                        }
                    }
                }
                return responseData;
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