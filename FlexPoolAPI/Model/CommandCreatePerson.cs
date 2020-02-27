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
    /// Creates a person record in the database.
    /// </summary>
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
                    //This command allows a minimum of an emp_id and password. If the user
                    //sends more then those, we need to check the Dictionary for them and
                    //add them to the SQL statement.
                    string insertStatement = "INSERT INTO flexpooldb.person (emp_id, password";
                    string valueStatement = " VALUES(" + requestBody["emp_id"][0] + ", \"" + requestBody["password"][0] + "\"";
                    if (requestBody.ContainsKey("name"))
                    {
                        insertStatement += ", name";
                        valueStatement += ", \"" + requestBody["name"][0] + "\"";
                    }
                    if (requestBody.ContainsKey("email"))
                    {
                        insertStatement += ", email";
                        valueStatement += ", \"" + requestBody["email"][0] + "\"";
                    }
                    if (requestBody.ContainsKey("phone_num"))
                    {
                        insertStatement += ", phone_num";
                        valueStatement += ", \"" + requestBody["phone_num"][0] + "\"";
                    }
                    if (requestBody.ContainsKey("weekly_cap"))
                    {
                        insertStatement += ", weekly_cap";
                        valueStatement += ", " + requestBody["weekly_cap"][0];
                    }
                    if (requestBody.ContainsKey("emp_type_name"))
                    {
                        insertStatement += ", emp_type";
                        valueStatement += ", (SELECT emp_type_id FROM flexpooldb.employee_type " +
                                "WHERE emp_type_name = \"" + requestBody["emp_type_name"][0] + "\")";
                    }
                    insertStatement += ")";
                    valueStatement += ");";

                    string sql = insertStatement + valueStatement;

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