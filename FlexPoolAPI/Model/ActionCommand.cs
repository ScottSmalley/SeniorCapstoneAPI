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
    ///  Abstract Class that represents 
    ///  an ActionCommand object which
    ///  composes an Action object 
    ///  with a Command to perform
    ///  an action in the database.
    /// </summary>
    abstract public class ActionCommand
    {
        //If it's in debug mode, the Command Objects
        //will perform the request without doing
        //any safety or logical checks.
        protected static readonly bool inDebugMode = false;

        //If it's in dev mode, all the print line statements
        //execute so we can see in AWS CloudWatch sql commands
        //or connection values to see if an obvious problem exists.
        protected static readonly bool inDevMode = true;

        protected Action newAction;
        protected ActionCommand(Action newAction)
        {
            this.newAction = newAction;
        }

        /// <summary>
        /// All command objects are forced to
        /// have an Execute method.
        /// </summary>
        /// <returns></returns>
        abstract public Dictionary<string, string[]> Execute();
    }
}
