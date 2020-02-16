 /*
  * Utah Valley University
  * CS4550 Capstone Project
  * FlexPool Project
  * Brandon Bezzant
  * Mike Daniel
  * Scott Smalley
  */

using System;
using MySql.Data.MySqlClient;
using Amazon.Lambda.Core;

// https://aws.amazon.com/premiumsupport/knowledge-center/build-lambda-deployment-dotnet/ 
// $> dotnet lambda deploy-function --region us-east-2 --function-name flexpoolmaintenance
//
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FlexPoolMaintenance
{
    public class FlexPoolMessageMaintenance
    {
        /// <summary>
        /// Queries and deletes entries in the database that are older than
        /// 3 months from today's date. Also checks for messages that were
        /// inserted in error with incorrect dates.
        /// -Scott Smalley
        /// </summary>
        public void DeleteOldMessages()
        {
            Console.WriteLine("Delete Old Messages: working...");
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");
            //string mySQLConnectionString = "server=<Server>;user=<User>;database=flexpooldb;port=<Port>;password=<Password>;";

            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();
                DateTime currentDate = DateTime.UtcNow;
                int currentMonth = currentDate.Month;
                int threeMonthThreshold = currentMonth - 3;
                string sql = "";
                //For special cases January, February, and March where
                //December and earlier are included in the 3-months that we
                //save messages. Else delete where the mathematical months don't
                //get messy.
                if (threeMonthThreshold < 1)
                {
                    sql = "DELETE FROM flexpooldb.message " +
                        "WHERE date_sent < \"" + (currentDate.Year - 1) + "-" + String.Format("{0:D2}", (12 - (threeMonthThreshold * -1))) + "-01\" " +
                        "OR date_sent > \"" + currentDate.Year + "-" + String.Format("{0:D2}", currentMonth) + "-" + String.Format("{0:D2}", (currentDate.Day + 1)) + "\";";
                }
                else
                {
                    sql = "DELETE FROM flexpooldb.message " +
                        "WHERE date_sent < \"" + currentDate.Year + "-" + String.Format("{0:D2}", threeMonthThreshold) + "-01\" " +
                        "OR date_sent > \"" + currentDate.Year + "-" + String.Format("{0:D2}", currentMonth) + "-" + String.Format("{0:D2}", currentDate.Day) + "\";";
                }
                Console.WriteLine(sql);
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
