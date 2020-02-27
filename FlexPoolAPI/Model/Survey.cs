/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Represents a shift survey completed
    /// at the end of a shift.
    /// </summary>
    class Survey
    {
        public int shift_id { get; set; }
        public int emp_id { get; set; }
        public int mgr_id { get; set; }
        public int rating { get; set; }
        public string text { get; set; }
    }
}
