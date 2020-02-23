/*
 * Represents a shift cancellation request.
 * -Scott Smalley
 */

namespace FlexPoolAPI.Model
{
    public class CancelRequest
    {
        public int shift_id { get; set; }
        public int emp_id { get; set; }
        public string text { get; set; }
        public bool reviewed { get; set; }
        public bool is_approved { get; set; }
    }
}
