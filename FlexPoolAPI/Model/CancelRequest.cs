/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Represents a shift cancellation
    /// request.
    /// </summary>
    public class CancelRequest
    {
        public int shift_id { get; set; }
        public int emp_id { get; set; }
        public string text { get; set; }
        public bool reviewed { get; set; }
        public bool is_approved { get; set; }
    }
}
