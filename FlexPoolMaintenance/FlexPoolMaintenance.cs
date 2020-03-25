 /*
  * Utah Valley University
  * CS4550 Capstone Project
  * FlexPool Project
  * Frontend: Brandon Bezzant
  * Frontend: Mike Daniel
  * Backend (meaning this entire solution project): Scott Smalley
  */

using System;
using MySql.Data.MySqlClient;
using Amazon.Lambda.Core;
using System.Globalization;
using System.Collections.Generic;

// https://aws.amazon.com/premiumsupport/knowledge-center/build-lambda-deployment-dotnet/ 
// $> dotnet lambda deploy-function --region us-east-2 --function-name flexpoolmaintenance
//
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace FlexPoolMaintenance
{
    public class FlexPoolMaintenance
    {
        //Toggles console messages.
        bool inDevMode = true;

        /// <summary>
        /// Queries and deletes entries in the database that are older than
        /// 3 months from today's date. Also checks for messages that were
        /// inserted in error with incorrect dates.
        /// -Scott Smalley
        /// </summary>
        public void PerformMessageMaintenance()
        {
            //1st of each month.
            //CRON 30 7 1 * ? *
            if (inDevMode)
            {
                Console.WriteLine("Delete Old Messages: working...");
            }
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();

                //Calculate what boundary value months to keep and what ones to delete.
                //We perform this task every 1st of the month, so it keeps the previous
                //3 months of messages. Meaning on April 1st, we should keep January, February
                //and March messages.
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

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes all the weeks and creates
        /// new ones for the new year.
        /// Only done on New Years Day.
        /// </summary>
        public void PerformYearlyCalendarMaintenance()
        {
            /*
             * Every year at 12:15am MST on New Year
             * CRON: 30 7 1 * ? *
             */
            int NUM_DAYS_IN_WEEK = 7;

            //Uses 53 weeks so that the
            //last week also gets an end date.
            int WEEKS_IN_YEAR = 53;

            if (inDevMode)
            {
                Console.WriteLine("Revamping Calendar Year: working...");
            }
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();

                //Calculate what boundary value months to keep and what ones to delete.
                //We perform this task every 1st of the month, so it keeps the previous
                //3 months of messages. Meaning on April 1st, we should keep January, February
                //and March messages.
                DateTime currentDate = new DateTime(DateTime.Now.Year, 1, 1, new GregorianCalendar());

                List<Week> weeks = new List<Week>();
                if (!(currentDate.DayOfWeek.Equals("Sunday")))
                {
                    //Starting date representing Dec 31.
                    int backDate = 31;

                    //Subtract the number of days from Dec 31st to get 
                    //the correct date for the Sunday of the week.
                    switch (currentDate.DayOfWeek.ToString())
                    {
                        case "Monday":
                            break;
                        case "Tuesday":
                            backDate -= 1;
                            break;
                        case "Wednesday":
                            backDate -= 2;
                            break;
                        case "Thursday":
                            backDate -= 3;
                            break;
                        case "Friday":
                            backDate -= 4;
                            break;
                        case "Saturday":
                            backDate -= 5;
                            break;
                    }
                    currentDate = new DateTime((DateTime.Now.Year - 1), 12, backDate, 7, 0, 0, new GregorianCalendar());
                }

                //Iterate through the calendar, collecting Sundays.
                //The current Sunday is the start of the week, and
                //the next Sunday,  representing the start of the following
                //week is the end of the week.
                for (int weekCtr = 0; weekCtr < WEEKS_IN_YEAR; weekCtr++)
                {
                    weeks.Add(new Week
                    {
                        startOfWeek = currentDate,
                        endOfWeek = currentDate.AddDays(NUM_DAYS_IN_WEEK)
                    });
                    currentDate = currentDate.AddDays(NUM_DAYS_IN_WEEK);
                }

                //Insert week values into database.
                string sql = "DELETE FROM flexpooldb.calendar_year;";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdDeleteOldWeek = new MySqlCommand(sql, conn))
                {
                    cmdDeleteOldWeek.ExecuteNonQuery();
                }

                for (int weekIndex = 0; weekIndex < weeks.Count; weekIndex++)
                {
                    //Insert week values into database.
                    sql = "INSERT INTO flexpooldb.calendar_year (week_id, start_date, end_date) " +
                        "VALUES (" + weekIndex + ", \"" + weeks[weekIndex].startOfWeek.ToString("yyyy-MM-dd HH:mm:ss") + "\", \"" + 
                        weeks[weekIndex].endOfWeek.ToString("yyyy-MM-dd HH:mm:ss") + "\");";

                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmdInsertWeek = new MySqlCommand(sql, conn))
                    {
                        cmdInsertWeek.ExecuteNonQuery();
                    }
                }

                PerformWorkHistoryMaintenance();
            }
        }
        
        /// <summary>
        /// Resets all the people's weekly work hours accumulation.
        /// </summary>
        private void PerformWorkHistoryMaintenance()
        {
            if (inDevMode)
            {
                Console.WriteLine("Resetting work history: working...");
            }
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();

                //Reset all the hours_worked since we're converting to the new year.
                string sql = "UPDATE flexpooldb.work_history " +
                             "SET hours_worked = 0" +
                             "WHERE week_id BETWEEN 1 AND 51";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdDeleteWorkHistory = new MySqlCommand(sql, conn))
                {
                    cmdDeleteWorkHistory.ExecuteNonQuery();
                }

                //Update the last week work hours record
                //so it reflects as the first work week 
                //in the new year.
                sql = "UPDATE flexpooldb.work_history " +
                             "SET week_id = 0 " +
                             "WHERE week_id = 52;";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdMoveLastWorkHourValues = new MySqlCommand(sql, conn))
                {
                    cmdMoveLastWorkHourValues.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Moves records of shifts, assigned_shifts, 
        /// and shift surveys into a storage table.
        /// </summary>
        public void PerformMonthlyShiftMaintenance()
        {
            /*
             * Every 1st of the month at 12:45am MST
             * CRON: 45 7 1 * ? *
             */
            if (inDevMode)
            {
                Console.WriteLine("Migrating shifts from 2 months ago: working...");
            }
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");

            int NUM_MONTHS_IN_YEAR = 12;

            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();

                //Calculate 2 months before the current month.
                int monthToMigrate = ((DateTime.Now.Month - 3) % NUM_MONTHS_IN_YEAR) + 1;

                //If the month post 2 month calculation is 11 or 12, 
                //we know we need to go back one year (meaning the current month is either Jan or Feb).
                int yearToMigrate = monthToMigrate > 10 ? (DateTime.Now.Year - 1) : DateTime.Now.Year;

                //Migrate the shift surveys from 2 months ago into storage.
                string sql = "REPLACE INTO flexpooldb.shift_survey_storage (emp_id, shift_id, mgr_id, rating, text) " +
                             "SELECT emp_id, flexpooldb.shift_survey.shift_id, flexpooldb.shift_survey.mgr_id, rating, text " +
                             "FROM flexpooldb.shift_survey " +
                             "INNER JOIN flexpooldb.shift " +
                             "ON flexpooldb.shift.shift_id = flexpooldb.shift_survey.shift_id " +
                             "WHERE date BETWEEN \"" + yearToMigrate + "-" + monthToMigrate + "-01 07:00:00\" " +
                             "AND \"" + yearToMigrate + "-" + (monthToMigrate+1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdMigrateShiftSurvey = new MySqlCommand(sql, conn))
                {
                    cmdMigrateShiftSurvey.ExecuteNonQuery();
                }

                //Migrate the shifts from 2 months ago into storage.
                sql = "REPLACE INTO flexpooldb.shift_storage (shift_id, date, duration, num_worker, max_worker, mgr_id, dept_id, skill_id, was_completed) " +
                             "SELECT shift_id, date, duration, num_worker, max_worker, mgr_id, dept_id, skill_id, 1 " + 
                             "FROM flexpooldb.shift " +
                             "WHERE date BETWEEN \"" + yearToMigrate + "-" + monthToMigrate + "-01 07:00:00\" " +
                             "AND \"" + yearToMigrate + "-" + (monthToMigrate + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdMigrateShift = new MySqlCommand(sql, conn))
                {
                    cmdMigrateShift.ExecuteNonQuery();
                }

                //Migrate the assigned_shifts from 2 months ago into storage.
                sql = "REPLACE INTO flexpooldb.assigned_shift_storage(shift_id, emp_id, was_completed) " +
                      "SELECT flexpooldb.shift.shift_id, flexpooldb.assigned_shift.emp_id, 1 " +
                      "FROM flexpooldb.shift " +
                      "INNER JOIN flexpooldb.assigned_shift " +
                      "ON flexpooldb.shift.shift_id = flexpooldb.assigned_shift.shift_id " +
                      "WHERE flexpooldb.shift.date BETWEEN \"" + yearToMigrate + "-" + monthToMigrate + "-01 07:00:00\" " +
                      "AND \"" + yearToMigrate + "-" + (monthToMigrate + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdMigrateAssignedShift = new MySqlCommand(sql, conn))
                {
                    cmdMigrateAssignedShift.ExecuteNonQuery();
                }

                //Select all the shift ids that need to be deleted.
                sql = "SELECT shift_id FROM flexpooldb.shift " +
                      "WHERE flexpooldb.shift.date BETWEEN \"" + DateTime.Now.Year + "-" + monthToMigrate + "-01 07:00:00\" " +
                      "AND \"" + DateTime.Now.Year + "-" + (monthToMigrate + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                List<int> shiftIdsToDelete = new List<int>();
                using (MySqlCommand cmdShiftToBeMigrated = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader rdr = cmdShiftToBeMigrated.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            shiftIdsToDelete.Add((int)rdr[0]);
                        }
                    }
                }

                if (shiftIdsToDelete.Count > 0)
                {
                    foreach (int shift_id in shiftIdsToDelete)
                    {
                        //Delete the surveys from 2 months ago now that a copy is in storage.
                        sql = "DELETE FROM flexpooldb.shift_survey " +
                              "WHERE flexpooldb.shift_survey.shift_id = " + shift_id + ";";

                        if (inDevMode)
                        {
                            Console.WriteLine(sql);
                        }
                        using (MySqlCommand cmdDeleteShiftSurvey = new MySqlCommand(sql, conn))
                        {
                            cmdDeleteShiftSurvey.ExecuteNonQuery();
                        }
                    }

                    foreach (int shift_id in shiftIdsToDelete)
                    {
                        //Delete the assigned_shifts from 2 months ago now that a copy is in storage.
                        sql = "DELETE FROM flexpooldb.assigned_shift " +
                              "WHERE flexpooldb.assigned_shift.shift_id = " + shift_id + ";";

                        if (inDevMode)
                        {
                            Console.WriteLine(sql);
                        }
                        using (MySqlCommand cmdDeleteAssignedShift = new MySqlCommand(sql, conn))
                        {
                            cmdDeleteAssignedShift.ExecuteNonQuery();
                        }
                    }

                    foreach (int shift_id in shiftIdsToDelete)
                    {
                        //Delete the shifts from 2 months ago now that a copy is in storage.
                        sql = "DELETE FROM flexpooldb.shift " +
                              "WHERE flexpooldb.shift.shift_id = " + shift_id + ";";

                        if (inDevMode)
                        {
                            Console.WriteLine(sql);
                        }
                        using (MySqlCommand cmdDeleteShift = new MySqlCommand(sql, conn))
                        {
                            cmdDeleteShift.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates monthly reports for anyone 
        /// in the app with the position of HR Manager.
        /// Includes:
        ///     1) Total Num Shifts Completed
        ///     2) Total Num of Deleted Shifts
        ///     3) Total Num of Positions to Fill
        ///     4) Total Num of Positions filled
        ///     5) Total Num of Employee Shift Cancel requests
        /// </summary>
        public void PerformMonthlyHRReports()
        {
            /*
            * Every 1st of the month at 1:00am MST
            * CRON: 00 8 1 * ? *
            */
            if (inDevMode)
            {
                Console.WriteLine("Building HR reports...");
            }
            string mySQLConnectionString = Environment.GetEnvironmentVariable("MYSQL_CONN");
            
            int NUM_MONTHS_IN_YEAR = 12;
            
            //Employee type to send the reports to.
            string REPORTING_PARTY = "HR Manager";
           
            //This code's reporting id for the message.
            int REPORTING_ID = 0;
            string report = "";
            using (MySqlConnection conn = new MySqlConnection(mySQLConnectionString))
            {
                conn.Open();
               
                //Calculate 1 month before the current month.
                int monthToReport = ((DateTime.Now.Month - 2) % NUM_MONTHS_IN_YEAR) + 1;
               
                //If the month post 1 month calculation is 12, 
                //we know we need to go back one year (meaning the current month is January).
                int yearToReport = monthToReport > 11 ? (DateTime.Now.Year - 1) : DateTime.Now.Year;
                
                //Number of shifts completed.
                string sql = "SELECT COUNT(*) " +
                             "FROM flexpooldb.shift " +
                             "WHERE date " +
                             "BETWEEN \"" + yearToReport + "-" + monthToReport + "-01 07:00:00\" " +
                             "AND \"" + yearToReport + "-" + (monthToReport + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdNumShiftsCompleted = new MySqlCommand(sql, conn))
                {
                    object data = cmdNumShiftsCompleted.ExecuteScalar();
                    report += $"Completed Shifts: {Convert.ToInt32(data)} ";
                }

                //Number of shifts deleted.
                sql = "SELECT COUNT(*) " +
                      "FROM flexpooldb.shift_storage " +
                      "WHERE date " +
                      "BETWEEN \"" + yearToReport + "-" + monthToReport + "-01 07:00:00\" " +
                      "AND \"" + yearToReport + "-" + (monthToReport + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdNumShiftsDeleted = new MySqlCommand(sql, conn))
                {
                    object data = cmdNumShiftsDeleted.ExecuteScalar();
                    report += $" Deleted Shifts: {Convert.ToInt32(data)} ";
                }

                //Number of total positions.
                sql = "SELECT SUM(flexpooldb.shift.max_worker) " +
                      "FROM flexpooldb.shift " +
                      "WHERE date " +
                      "BETWEEN \"" + yearToReport + "-" + monthToReport + "-01 07:00:00\" " +
                      "AND \"" + yearToReport + "-" + (monthToReport + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdTotalPositionsToFill = new MySqlCommand(sql, conn))
                {
                    object data = cmdTotalPositionsToFill.ExecuteScalar();
                    report += $" Total Positions to be Filled: {Convert.ToInt32(data)} ";
                }

                //Number of positions filled.
                sql = "SELECT SUM(flexpooldb.shift.num_worker) " +
                      "FROM flexpooldb.shift " +
                      "WHERE date " +
                      "BETWEEN \"" + yearToReport + "-" + monthToReport + "-01 07:00:00\" " +
                      "AND \"" + yearToReport + "-" + (monthToReport + 1) + "-01 06:59:59\";";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdTotalPositionsFilled = new MySqlCommand(sql, conn))
                {
                    object data = cmdTotalPositionsFilled.ExecuteScalar();
                    report += $" Total Positions Filled: {Convert.ToInt32(data)} ";
                }

                //Number of employee cancellation requests.
                sql = "SELECT COUNT(*) " +
                      "FROM flexpooldb.cancel_shift_request " +
                      "INNER JOIN (SELECT shift_id FROM flexpooldb.shift WHERE date BETWEEN \"" + 
                            yearToReport + "-" + monthToReport + "-01 07:00:00\" AND \"" + yearToReport + "-" + 
                                    (monthToReport + 1) + "-01 06:59:59\") AS MonthShifts " +
                      "ON flexpooldb.cancel_shift_request.shift_id = MonthShifts.shift_id;";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                using (MySqlCommand cmdNumEmpCancellations = new MySqlCommand(sql, conn))
                {
                    object data = cmdNumEmpCancellations.ExecuteScalar();
                    report += $" Total Shift Cancel Requests: {Convert.ToInt32(data)} ";
                }

                if (inDevMode)
                {
                    Console.WriteLine(report);
                }

                //Get all the HR Managers to send the report to.
                sql = "SELECT emp_id FROM person " +
                      "WHERE emp_type = " +
                      "(SELECT flexpooldb.employee_type.emp_type_id " +
                      "FROM flexpooldb.employee_type " +
                      "WHERE flexpooldb.employee_type.emp_type_name = \"" + REPORTING_PARTY + "\");";

                if (inDevMode)
                {
                    Console.WriteLine(sql);
                }
                List<int> hrManagers = new List<int>();
                using (MySqlCommand cmdGetHRManagerIDs = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader rdr = cmdGetHRManagerIDs.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            hrManagers.Add((int)rdr[0]);
                        }
                    }
                }

                //Send message to the reporting parties.
                foreach (int emp_id in hrManagers)
                {
                    sql = "INSERT INTO flexpooldb.message (sender_id, receiver_id, date_sent, msg_text) " +
                          "VALUES(" + REPORTING_ID + ", " + emp_id + ", \"" + DateTime.UtcNow + "\", \"" + report + "\"); ";
                    if (inDevMode)
                    {
                        Console.WriteLine(sql);
                    }
                    using (MySqlCommand cmdMessageReport = new MySqlCommand(sql, conn))
                    {
                        cmdMessageReport.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
