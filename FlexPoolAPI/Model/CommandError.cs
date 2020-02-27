/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System.Collections.Generic;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Used to return errors to the user if
    /// they didn't include an "action" item
    /// in their sent data.
    /// </summary>
    class CommandError : ActionCommand
    {
        public CommandError(Action newAction) : base(newAction) { }
        
        //Return back a failure message and the reason.
        public override Dictionary<string, string[]> Execute()
        {
            Dictionary<string, string[]> responseData = new Dictionary<string, string[]>();
            responseData.Add("response", new string[] { "failure" });
            responseData.Add("reason", new string[] { "action item missing in dictionary." });
            return responseData;
        }
    }
}
