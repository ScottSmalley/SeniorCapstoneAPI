using System;
using System.Collections.Generic;
using System.Text;

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
