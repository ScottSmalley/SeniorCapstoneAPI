/*
 * Used to determine which type of command to use.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// This class is to determine which ActionCommand object to return to the caller.
    /// </summary>
    class ActionParser
    {
        public ActionCommand DetermineCommandType(Action newAction)
        {
            switch (newAction.GetActionItem())
            {
                case "get_person":
                    return new CommandGetPerson(newAction);
                case "create_person":
                    return new CommandCreatePerson(newAction);
                case "delete_person":
                    return new CommandDeletePerson(newAction);
                case "edit_person":
                    return new CommandEditPerson(newAction);
                case "authenticate":
                    return new CommandAuthenticate(newAction);
                case "assign_skill":
                    return new CommandAssignSkill(newAction);
                case "withdraw_skill":
                    return new CommandWithdrawSkill(newAction);
                case "create_skill":
                    return new CommandCreateSkill(newAction);
                case "delete_skill":
                    return new CommandDeleteSkill(newAction);
                case "edit_skill":
                    return new CommandEditSkill(newAction);
                case "get_emp_skill":
                    return new CommandGetEmpSkill(newAction);
                case "send_message":
                    return new CommandSendMessage(newAction);
                case "get_sent_message":
                    return new CommandGetSentMessage(newAction);
                case "get_received_message":
                    return new CommandGetReceivedMessage(newAction);
                case "delete_message":
                    return new CommandDeleteMessage(newAction);
            }
            return new CommandError(newAction);
        }
    }
}
