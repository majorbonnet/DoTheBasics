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

        private ObservableCollection<GroupedGoals> _goalsToComplete;
        private ObservableCollection<GoalViewModel> _completedGoals;

        private GroupedGoals _morningGoals;
        private GroupedGoals _noonGoals;
        private GroupedGoals _eveningGoals;

        public MainPage()
        {
            InitializeComponent();

            _goalDb = new GoalDatabase();

            _goalsToComplete = new ObservableCollection<GroupedGoals>();
            _completedGoals = new ObservableCollection<GoalViewModel>();

            _morningGoals = new GroupedGoals("Morning", "AM");
            _noonGoals = new GroupedGoals("Afternoon", "PM");
            _eveningGoals = new GroupedGoals("Evening", "PM");

            _goalsToComplete.Add(_morningGoals);
            _goalsToComplete.Add(_noonGoals);
            _goalsToComplete.Add(_eveningGoals);

            this.ToDosListView.ItemsSource = _goalsToComplete;
            this.DoneListView.ItemsSource = _completedGoals;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                if (_completedGoals.Count == 0)
                {
                    EmptyDoneListLabel.IsVisible = !EmptyDoneListLabel.IsVisible;
                }
                else
                {
                    DoneListView.IsVisible = !DoneListView.IsVisible;
                }
            };

            this.ToggleCompletedGoals.GestureRecognizers.Add(tapGestureRecognizer);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var goalsFromDb = await _goalDb.GetGoalsAsync();

            _morningGoals.Clear();
            _noonGoals.Clear();
            _eveningGoals.Clear();

            _completedGoals.Clear();

            foreach (var goal in goalsFromDb)
            {
                if ((DateTime.Now.Date - goal.LastCompletion.Date).TotalDays == 0)
                {
                    _completedGoals.Add(new GoalViewModel(goal));
                }
                else
                {
                    AddGoalToToDoList(new GoalViewModel(goal));
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

            await _goalDb.DeactivateGoal(goalId);

            _completedGoals.Remove(_completedGoals.FirstOrDefault(g => g.Id == goalId));

            if (_completedGoals.Count == 0 && DoneListView.IsVisible)
            {
                DoneListView.IsVisible = false;
                EmptyDoneListLabel.IsVisible = true;
            }

            RemoveGoalFromToDoList(goalId);

            DependencyService.Get<INotificationManager>().Cancel(goalId);
        }

        private async void CompleteSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            var goal = await _goalDb.GetGoalAsync(goalId);
            var updatedGoal = await _goalDb.AddGoalCompletion(goal, DateTime.Now);

            RemoveGoalFromToDoList(goalId);
            _completedGoals.Add(new GoalViewModel(updatedGoal));

            if (_completedGoals.Count > 0 && EmptyDoneListLabel.IsVisible)
            {
                DoneListView.IsVisible = true;
                EmptyDoneListLabel.IsVisible = false;
            }
        }

        private async void UndoSwipeItem_Invoked(object sender, EventArgs e)
        {
            int goalId = (int)((SwipeItem)sender).CommandParameter;

            var goal = await _goalDb.GetGoalAsync(goalId);
            var updatedGoal = await _goalDb.UndoGoalCompletion(goalId);

            _completedGoals.Remove(_completedGoals.FirstOrDefault(g => g.Id == goalId));

            if (_completedGoals.Count == 0 && DoneListView.IsVisible)
            {
                DoneListView.IsVisible = false;
                EmptyDoneListLabel.IsVisible = true;
            }

            AddGoalToToDoList(new GoalViewModel(updatedGoal));
        }

        private void AddGoalToToDoList(GoalViewModel goal)
        {
            if (goal.GoalHour < 12)
            {
                _morningGoals.Add(goal);
            }
            else if (goal.GoalHour >= 12 && goal.GoalHour < 17)
            {
                _noonGoals.Add(goal);
            }
            else
            {
                _eveningGoals.Add(goal);
            }
        }

        private void RemoveGoalFromToDoList(int goalId)
        {
            TryRemoveGoalFromGroupedList(goalId, _morningGoals);
            TryRemoveGoalFromGroupedList(goalId, _noonGoals);
            TryRemoveGoalFromGroupedList(goalId, _eveningGoals);
        }

        private bool TryRemoveGoalFromGroupedList(int goalId, GroupedGoals groupedGoals)
        {
            GoalViewModel toRemove = null;
            if ((toRemove = groupedGoals.FirstOrDefault(g => g.Id == goalId)) != null)
            {
                groupedGoals.Remove(toRemove);

                return true;
            }

            return false;
        }
    }
}