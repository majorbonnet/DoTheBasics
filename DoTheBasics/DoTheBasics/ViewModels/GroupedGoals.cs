using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DoTheBasics
{
    public class GroupedGoals : ObservableCollection<GoalViewModel>
    {
        public GroupedGoals(string longName, string shortName) : base()
        {
            this.LongName = longName;
            this.ShortName = shortName;
        }

        public string LongName { get; set; }
        public string ShortName { get; set; }
    }
}
