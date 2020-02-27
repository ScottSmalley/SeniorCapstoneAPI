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
    /// Executes the command given, 
    /// and returns the data from 
    /// the Command Object.
    /// </summary>
    class ActionExecutor
    {
        public Dictionary<string, string[]> ExecuteCommand(ActionCommand newCommand)
        {
            return newCommand.Execute();
        }
    }
}
