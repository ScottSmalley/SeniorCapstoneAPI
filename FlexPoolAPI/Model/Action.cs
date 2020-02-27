/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;
using System.Collections.Generic;

namespace FlexPoolAPI.Model
{
    /// <summary>
    ///  Represents the request given by
    ///  the user.This stores helpful information
    ///  such as the request Dictionary, Time conversion methods,
    ///  our MySQL connection, and getter for the "action" item.
    /// </summary>
    public class Action
    {
        private Dictionary<string, string[]> requestBody;
        private string sqlConn;
        public Action(Dictionary<string, string[]> requestBody, string sqlConn)
        {
            this.requestBody = requestBody;
            this.sqlConn = sqlConn;
        }

        /// <summary>
        /// Returns the "action" item 
        /// in the dictionary.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Allows a user to replace the "action" item
        /// in the Dictionary.
        /// </summary>
        /// <param name="newData"></param>
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

        /// <summary>
        /// Add a data item to the Dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public void AddRequestItem(string key, string[] data)
        {
            requestBody.Add(key, data);
        }

        /// <summary>
        /// Returns the Dictionary of items sent by the user.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string[]> GetRequestBody()
        {
            return requestBody;
        }

        /// <summary>
        /// Returns the sql connection string.
        /// </summary>
        /// <returns></returns>
        public string GetSQLConn()
        {
            return sqlConn;
        }

        /// <summary>
        /// Returns the Universal Time currently
        /// in the format that MySQL requires.
        /// </summary>
        /// <returns></returns>
        public string GetDateTimeUTC()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
