/*
 * Used to execute the given command.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexPoolAPI.Model
{
    class ActionExecutor
    {
        public Dictionary<string, string[]> ExecuteCommand(ActionCommand newCommand)
        {
            return newCommand.Execute();
        }
    }
}
