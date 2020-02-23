/*
 * Represents a shift survey that
 * employees and managers fill out after every shift.
 * -Scott Smalley
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace FlexPoolAPI.Model
{
    class Survey
    {
        public int shift_id { get; set; }
        public int emp_id { get; set; }
        public int mgr_id { get; set; }
        public int rating { get; set; }
        public string text { get; set; }
    }
}
