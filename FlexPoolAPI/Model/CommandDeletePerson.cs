/*
 * Deletes a specified person in the database.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FlexPoolAPI.Model
{
    class CommandDeletePerson : ActionCommand
    {
        public CommandDeletePerson(Action newAction) : base(newAction) { }

        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(newAction.GetSQLConn()))
                {
                    conn.Open();
                    //Creates a storage record for the person to be deleted.
                    string sql = "INSERT INTO flexpooldb.person_storage (emp_id, name, email, password, phone_num, weekly_hours, weekly_cap, emp_type, is_frozen) " + 
                                 "SELECT emp_id, name, email, password, phone_num, weekly_hours, weekly_cap, emp_type, is_frozen FROM flexpooldb.person " +
                                 "WHERE emp_id = " + requestBody["emp_id"][0] + ";";
                    Console.WriteLine(sql);
                    using (MySqlCommand cmdMigratePerson = new MySqlCommand(sql, conn))
                    {
                        cmdMigratePerson.ExecuteNonQuery();
                    }

                    //Deletes person from the person table.
                    sql = "DELETE FROM flexpooldb.person WHERE emp_id = " + requestBody["emp_id"][0] + ";";
                    Console.WriteLine(sql);
                    //Make a Command Object to then execute.
                    using (MySqlCommand cmdDeletePerson = new MySqlCommand(sql, conn))
                    {
                        cmdDeletePerson.ExecuteNonQuery();
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
