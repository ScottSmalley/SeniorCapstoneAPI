/*
 * Used to return errors in basic
 * communications. For the moment, 
 * if a user forgets the required "action" 
 * item in their request body, it'll send back
 * an error.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexPoolAPI.Model
{
    class CommandError : ActionCommand
    {
        public CommandError(Action newAction) : base(newAction) { }
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            responseData.Add("response", new string[] { "failure" });
            responseData.Add("reason", new string[] { "action item missing in dictionary." });
            return responseData;
        }
    }
}
