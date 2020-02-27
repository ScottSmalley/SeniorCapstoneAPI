/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;
using System.Collections.Generic;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Gets all the information on a specified person,
    /// in addition all the shifts they've signed up for,
    /// and the rest of the shifts that are available.
    /// </summary>
    class CommandGetPersonAndShift : ActionCommand
    {
        public CommandGetPersonAndShift(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();

            try
            {
                //Modify the dictionary to mimic a get_person request.
                //Then send the data to a new Command Object and store the results.
                newAction.SetActionItem("get_person");
                newAction.AddRequestItem("emp_ids", new string[] { requestBody["emp_id"][0] });
                responseData = new CommandGetPerson(newAction).Execute();

                //Modify the dictionary to mimic a get_shift_by_emp request.
                //Then send the data to a new Command Object and store the results.
                newAction.SetActionItem("get_shift_by_emp");
                Dictionary<string, string[]> employeeShifts = new CommandGetShiftByEmp(newAction).Execute();

                //Modify the dictionary to mimic a get_all_shift request.
                //Then send the data to a new Command Object and store the results.
                newAction.SetActionItem("get_all_shift");
                Dictionary<string, string[]> allShifts = new CommandGetAllShift(newAction).Execute();

                //If a shift is found in the employee dictionary of shifts,
                //mark it as assigned, otherwise just add it to the dictionary.
                foreach (string key in allShifts.Keys)
                {
                    string empKey = "assigned " + key;
                    if (employeeShifts.ContainsKey(empKey))
                    {
                        responseData.Add(empKey, employeeShifts[empKey]);
                    }
                    else
                    {
                        responseData.Add(key, allShifts[key]);
                    }
                }

                //If something went wrong with changing action items, make sure to flag it as such
                //and send an error message back.
                if (!requestBody["action"][0].Equals("failure"))
                {
                    return responseData;
                }
                else
                {
                    Console.WriteLine("ERROR: Problem with the multi-commands, one of them went awry.");
                    responseData = new Dictionary<string, string[]>();
                    responseData.Add("response", new string[] { "failure" });
                    responseData.Add("reason", new string[] { "missing item in dictionary." });
                    return responseData;
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