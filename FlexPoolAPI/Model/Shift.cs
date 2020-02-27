/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/
using System;

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Represents a shift that people
    /// sign up for.
    /// </summary>
    class Shift
    {
        public int shift_id { get; set; }
        public DateTime date { get; set; }
        public int duration { get; set; }
        public int num_worker { get; set; }
        public int max_worker { get; set; }
        public int mgr_id { get; set; }
        public int skill_id { get; set; }
        public string skill_name { get; set; }
        public int dept_id { get; set; }
        public string dept_name { get; set; }
    }
}
