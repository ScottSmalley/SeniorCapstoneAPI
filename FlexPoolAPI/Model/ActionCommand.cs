/*
 * Abstract Class that represents 
 * an ActionCommand object which
 * composes an Action object which
 * stores request data and a database
 * connection.
 */
using System.Collections.Generic;

namespace FlexPoolAPI.Model
{
    abstract public class ActionCommand
    {
        protected Action newAction;
        protected static readonly bool inDebugMode = false;
        protected ActionCommand(Action newAction)
        {
            this.newAction = newAction;
        }
        abstract public Dictionary<string, string[]> Execute();
    }
}
