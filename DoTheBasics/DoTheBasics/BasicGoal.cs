using System;
using System.Collections.Generic;
using System.Text;

namespace DoTheBasics
{
    public class BasicGoal
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
        public DateTime AlertTime { get; set; }
    }
}
