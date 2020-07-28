using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DoTheBasics
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private GoalDatabase _goalDb;

        private ObservableCollection<GoalViewModel> _goalsToComplete;
        private ObservableCollection<GoalViewModel> _completedGoals;

        public MainPage()
        {
            InitializeComponent();

            _goalDb = new GoalDatabase();
            _goalsToComplete = new ObservableCollection<GoalViewModel>();
            _completedGoals = new ObservableCollection<GoalViewModel>();

            this.ToDosListView.ItemsSource = _goalsToComplete;
            this.DoneListView.ItemsSource = _completedGoals;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var goalsFromDb = await _goalDb.GetGoalsAsync();

            _goalsToComplete.Clear();
            _completedGoals.Clear();

            foreach (var goal in goalsFromDb)
            {
                if ((DateTime.Now.Date - goal.LastCompletion.Date).TotalDays == 0)
                {
                    _completedGoals.Add(new GoalViewModel(goal));
                }
                else
                {
                    _goalsToComplete.Add(new GoalViewModel(goal));
                }
            }
        }

        private async void AddButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditGoal());
        }

        private async void EditSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            await Navigation.PushAsync(new EditGoal(goalId));
        }

        private async void DeleteSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            await Navigation.PushAsync(new EditGoal(goalId));
        }

        private async void CompleteSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            var goal = await _goalDb.GetGoalAsync(goalId);
            var updatedGoal = await _goalDb.AddGoalCompletion(goal, DateTime.Now);

            _goalsToComplete.Remove(_goalsToComplete.FirstOrDefault(g => g.Id == goalId));
            _completedGoals.Add(new GoalViewModel(updatedGoal));
        }

        private async void UndoSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            var goal = await _goalDb.GetGoalAsync(goalId);
            var updatedGoal = await _goalDb.UndoGoalCompletion(goalId);

            _completedGoals.Remove(_completedGoals.FirstOrDefault(g => g.Id == goalId));
            _goalsToComplete.Add(new GoalViewModel(updatedGoal));
        }
    }
}