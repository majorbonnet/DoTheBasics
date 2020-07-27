using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DoTheBasics
{
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
            var goal = await _goalDb.AddGoal(this.TitleEntry.Text, this.DescEditor.Text, this.GoalTimeSelector.Time.Hours, this.GoalTimeSelector.Time.Minutes);

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