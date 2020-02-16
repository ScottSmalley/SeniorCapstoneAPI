/*
 * Returns an employee's information, the shifts they're
 * signed up for, and all the shifts currently available.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace FlexPoolAPI.Model
{
    class CommandGetPersonAndShift : ActionCommand
    {
        public CommandGetPersonAndShift(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> requestBody = newAction.GetRequestBody();
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            
            //Get Person information.
            newAction.SetActionItem("get_person");
            newAction.AddRequestItem("emp_ids", new string[] { requestBody["emp_id"][0] });
            responseData = new CommandGetPerson(newAction).Execute();

            //Get all the shifts that are assigned to this person's id.
            newAction.SetActionItem("get_shift_by_emp");
            Dictionary<string, string[]> employeeShifts = new CommandGetShiftByEmp(newAction).Execute();

            //Get all the shifts for the current month.
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
    }
}