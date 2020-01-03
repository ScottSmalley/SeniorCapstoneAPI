﻿/*
 * Inserts a Person into our database
 * person table.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    public class CommandCreatePerson : ActionCommand
    {
        public CommandCreatePerson(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    string sql = "INSERT INTO flexpooldb.person (emp_id, name, email, password, phone_num, weekly_cap, emp_type)" +
                                " VALUES(" + requestBody["emp_id"][0] + ", \"" +
                                requestBody["name"][0] + "\", \"" + requestBody["email"][0] + "\", \"" +
                                requestBody["password"][0] + "\", \"" + requestBody["phone_num"][0] + "\", " +
                                requestBody["weekly_cap"][0] + ", (SELECT emp_type_id FROM flexpooldb.employee_type " +
                                "WHERE emp_type_name = \"" + requestBody["emp_type_name"][0] + "\"));";

                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                        responseData.Add("response", new string[] { "success" });
                        return responseData;
                    }
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("ERROR: problem formatting.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "formatting error." });
                return responseData;
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("ERROR: missing key in the dictionary.");
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
        }
    }
}