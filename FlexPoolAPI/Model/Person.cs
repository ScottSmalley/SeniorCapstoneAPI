/*
* Scott Smalley
* Senior - Software Engineering
* Utah Valley University
* scottsmalley90@gmail.com
*/

namespace FlexPoolAPI.Model
{
    /// <summary>
    /// Represents a person or employee in the database.
    /// </summary>
    class Person
    {
        public int emp_id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string phone_num { get; set; }
        public int weekly_hours { get; set; }
        public int weekly_cap { get; set; }
        public int emp_type { get; set; }
        public string emp_type_name { get; set; }
    }
}
