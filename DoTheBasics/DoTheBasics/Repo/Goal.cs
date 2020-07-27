using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoTheBasics.Repo
{
    public class Goal
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int GoalHour { get; set; }
        public int GoalMinute { get; set; }
        public DateTime LastCompletion { get; set; }
        public bool IsActive { get; set; }
    }
}
