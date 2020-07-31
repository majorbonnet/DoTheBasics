using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DoTheBasics
{
    public class GoalViewModel : INotifyPropertyChanged
    { 
        private bool isSelected = false;

        public GoalViewModel(Goal goal)
        {
            this.Id = goal.Id;
            this.Title = goal.Title;
            this.Description = goal.Description;
            this.GoalHour = goal.GoalHour;
            this.GoalMinute = goal.GoalMinute;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public int GoalHour { get; set; }
        public int GoalMinute { get; set; }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        //OnProperty changed method
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
