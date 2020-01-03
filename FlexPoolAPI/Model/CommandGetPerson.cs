/*
 * Generates a Dictionary of Person objects that 
 * are pulled from the DB. 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetPerson : ActionCommand
    {
        public CommandGetPerson(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    string sql = "SELECT emp_id, name, email, phone_num, weekly_hours, weekly_cap, flexpooldb.employee_type.emp_type_name FROM flexpooldb.person " +
                        "INNER JOIN flexpooldb.employee_type " +
                        "ON flexpooldb.person.emp_type = employee_type.emp_type_id " +
                        "AND emp_id IN (";
                    foreach (string emp in requestBody["emp_ids"])
                    {
                        sql += emp + ",";
                    }
                    //Remove excess "," from sql statement.
                    sql = sql.Remove(sql.Length - 1);
                    sql += ");";
                    Console.WriteLine(sql);
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        //Executes the command, and returns the result as an array.
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
                                    weekly_hours = (int)rdr[4],
                                    weekly_cap = (int)rdr[5],
                                    emp_type_name = (string)rdr[6]
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
                responseData.Add("response", new string[] { "failure" });
                responseData.Add("reason", new string[] { "missing item in dictionary." });
                return responseData;
            }
        }
    }
}