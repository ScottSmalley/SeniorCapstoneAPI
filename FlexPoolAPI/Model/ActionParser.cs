/*
 * Used to determine which type of command to use.
 * -Scott Smalley
 */
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
                case "edit_person_lockout":
                    return new CommandEditPersonLockout(newAction);
                case "freeze_person":
                    return new CommandFreezePerson(newAction);
                case "unfreeze_person":
                    return new CommandUnfreezePerson(newAction);
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
                case "get_all_skill":
                    return new CommandGetAllSkill(newAction);
                case "get_emp_skill":
                    return new CommandGetEmpSkill(newAction);
                case "send_message":
                    return new CommandSendMessage(newAction);
                case "delete_message":
                    return new CommandDeleteMessage(newAction);
                case "get_sent_message":
                    return new CommandGetSentMessage(newAction);
                case "get_received_message":
                    return new CommandGetReceivedMessage(newAction);
                case "get_all_message":
                    return new CommandGetAllMessage(newAction);
                case "create_shift":
                    return new CommandCreateShift(newAction);
                case "delete_shift":
                    return new CommandDeleteShift(newAction);
                case "get_all_shift":
                    return new CommandGetAllShift(newAction);
                case "assign_shift":
                    return new CommandAssignShift(newAction);
                case "remove_shift":
                    return new CommandRemoveShift(newAction);
                case "edit_dept_mgr":
                    return new CommandEditDeptMgr(newAction);
                case "get_shift_by_dept":
                    return new CommandGetShiftByDept(newAction);
                case "get_shift_by_emp":
                    return new CommandGetShiftByEmp(newAction);
                case "get_shift_by_skill":
                    return new CommandGetShiftBySkill(newAction);
                case "send_cancel_request":
                    return new CommandSendCancelRequest(newAction);
                case "delete_cancel_request":
                    return new CommandDeleteCancelRequest(newAction);
                case "get_cancel_request":
                    return new CommandGetCancelRequest(newAction);
                case "approve_cancel_request":
                    return new CommandApproveCancelRequest(newAction);
                case "unapprove_cancel_request":
                    return new CommandUnapproveCancelRequest(newAction);
                case "get_person_and_shift":
                    return new CommandGetPersonAndShift(newAction);
                case "get_all_dept":
                    return new CommandGetAllDept(newAction);
                case "create_survey":
                    return new CommandCreateSurvey(newAction);
                case "delete_survey":
                    return new CommandDeleteSurvey(newAction);
                case "get_all_survey":
                    return new CommandGetAllSurvey(newAction);
            }
            return new CommandError(newAction);
        }
    }
}
