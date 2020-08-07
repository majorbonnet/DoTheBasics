using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DoTheBasics.Views
{
    [QueryProperty("GoalIdStr", "goalId")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditGoal : ContentPage
    {
        private GoalDatabase _goalDb;
        private int? _goalId;

        public EditGoal()
        {
            InitializeComponent();
            _goalDb = new GoalDatabase();
        }

        public EditGoal(int goalId) : this()
        {
            _goalId = goalId;
        }

        public string GoalIdStr
        {
            set
            {
                this._goalId = int.Parse(value);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_goalId.HasValue)
            {
                await LoadGoal();
            }
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            Goal goal;

            if (_goalId.HasValue)
            {
                goal = await _goalDb.GetGoalAsync(_goalId.Value);

                goal.Title = this.TitleEntry.Text;
                goal.Description = this.DescEditor.Text;
                goal.GoalHour = this.GoalTimeSelector.Time.Hours;
                goal.GoalMinute = this.GoalTimeSelector.Time.Minutes;

                goal = await _goalDb.UpdateGoal(goal);
            }
            else
            {
                goal = await _goalDb.AddGoal(this.TitleEntry.Text, this.DescEditor.Text, this.GoalTimeSelector.Time.Hours, this.GoalTimeSelector.Time.Minutes);
            }

            DependencyService.Get<INotificationManager>().ScheduleNotification(goal.Id, goal.Title, goal.Description, DateTime.Now.Date.AddHours(goal.GoalHour).AddMinutes(goal.GoalMinute));

            await Navigation.PopAsync();
        }

        private async Task LoadGoal()
        {
            var goal = await _goalDb.GetGoalAsync(_goalId.Value);

            this.TitleEntry.Text = goal.Title;
            this.DescEditor.Text = goal.Description;
            this.GoalTimeSelector.Time = new TimeSpan(goal.GoalHour, goal.GoalMinute, 0);
        }
    }
}