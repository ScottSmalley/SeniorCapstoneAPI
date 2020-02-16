/*
 * Represents the request given by
 * the user. This stores helpful information
 * such as the request Dictionary, Time conversion,
 * our MySQL connection, and getter for the "action" item.
 * 
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;

namespace FlexPoolAPI.Model
{
    public class Action
    {
        private Dictionary<string, string[]> requestBody;
        private string sqlConn;
        public Action(Dictionary<string, string[]> requestBody, string sqlConn)
        {
            this.requestBody = requestBody;
            this.sqlConn = sqlConn;
        }

        public string GetActionItem()
        {
            try
            {
                return requestBody["action"][0];
            }
            catch (KeyNotFoundException)
            {
                return "Not Found.";
            }
        }
        public void SetActionItem(string newData)
        {
            try
            {
                requestBody["action"][0] = newData;
            }
            catch (KeyNotFoundException)
            {
                requestBody["action"][0] = "failure";
                requestBody.Add("reason", new string[] { "Action Key not found." });
            }
        }

        public void AddRequestItem(string key, string[] data)
        {
            requestBody.Add(key, data);
        }

        public Dictionary<string, string[]> GetRequestBody()
        {
            return requestBody;
        }

        public string GetSQLConn()
        {
            return sqlConn;
        }

        /// <summary>
        /// Returns the Universal Time currently
        /// in the format that MySQL requires.
        /// -Scott Smalley
        /// </summary>
        /// <returns></returns>
        public string GetDateTimeUTC()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
