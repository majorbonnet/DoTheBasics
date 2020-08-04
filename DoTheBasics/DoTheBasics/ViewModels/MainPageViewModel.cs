using DoTheBasics.Repo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DoTheBasics.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly GoalDatabase _goalDatabase;
        private readonly INavigationService _navigationService;
        private bool _noCompleted = false;
        private bool _hasCompleted = false;

        private GroupedGoals _morningGoals;
        private GroupedGoals _afternoonGoals;
        private GroupedGoals _eveningGoals;

        public MainPageViewModel(GoalDatabase goalDatabase, INavigationService navigationService)
        {
            _goalDatabase = goalDatabase;
            _navigationService = navigationService;

            this.Todo = new ObservableCollection<GroupedGoals>();
            this.Completed = new ObservableCollection<Goal>();

            this.NoCompleted = true;
            this.HasCompleted = false;

            _morningGoals = new GroupedGoals("Morning", "AM");
            _afternoonGoals = new GroupedGoals("Afternoon", "PM");
            _eveningGoals = new GroupedGoals("Evening", "PM");

            this.Todo.Add(_morningGoals);
            this.Todo.Add(_afternoonGoals);
            this.Todo.Add(_eveningGoals);

            this.Completed.CollectionChanged += Completed_CollectionChanged;

            this.AddCommand = new Command(async () =>
            {
                await _navigationService.NavigateToEditAsync();
            });

            this.EditCommand = new Command(async (obj) =>
            {
                await _navigationService.NavigateToEditAsync((int)obj);
            });

            this.DeleteCommand = new Command(async (obj) =>
            {
                int goalId = (int)obj;

                await _goalDatabase.DeactivateGoal(goalId);

                Completed.Remove(Completed.FirstOrDefault(g => g.Id == goalId));
                RemoveGoalFromToDoList(goalId);

                DependencyService.Get<INotificationManager>().Cancel(goalId);
            });

            this.CompleteCommand = new Command(async (obj) =>
            {
                int goalId = (int)obj;

                var goal = await _goalDatabase.GetGoalAsync(goalId);
                var updatedGoal = await _goalDatabase.AddGoalCompletion(goal, DateTime.Now);

                RemoveGoalFromToDoList(goalId);
                Completed.Add(updatedGoal);
            });

            this.UndoCompleteCommand = new Command(async (obj) =>
            {
                int goalId = (int)obj;

                var goal = await _goalDatabase.GetGoalAsync(goalId);
                var updatedGoal = await _goalDatabase.UndoGoalCompletion(goalId);

                Completed.Remove(Completed.FirstOrDefault(g => g.Id == goalId));

                AddGoalToToDoList(new GoalViewModel(updatedGoal));
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GroupedGoals> Todo { get; private set; }
        public ObservableCollection<Goal> Completed { get; private set; }
        public bool NoCompleted 
        { 
            get
            {
                return _noCompleted;
            }

            private set
            {
                _noCompleted = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasCompleted
        {
            get
            {
                return _hasCompleted;
            }

            private set
            {
                _hasCompleted = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand CompleteCommand { get; private set; }
        public ICommand UndoCompleteCommand { get; private set; }
        public async Task OnAppearing()
        {

            var goalsFromDb = await _goalDatabase.GetGoalsAsync();

            _morningGoals.Clear();
            _afternoonGoals.Clear();
            _eveningGoals.Clear();

            Completed.Clear();

            foreach (var goal in goalsFromDb)
            {
                if ((DateTime.Now.Date - goal.LastCompletion.Date).TotalDays == 0)
                {
                    Completed.Add(goal);
                }
                else
                {
                    AddGoalToToDoList(new GoalViewModel(goal));
                }
            }
        }

        private void Completed_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                && this.Completed.Count == e.NewItems.Count)
            {
                NoCompleted = false;
                HasCompleted = true;
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                && this.Completed.Count == 0)
            {
                NoCompleted = true;
                HasCompleted = false;
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AddGoalToToDoList(GoalViewModel goal)
        {
            if (goal.GoalHour < 12)
            {
                _morningGoals.Add(goal);
            }
            else if (goal.GoalHour >= 12 && goal.GoalHour < 17)
            {
                _afternoonGoals.Add(goal);
            }
            else
            {
                _eveningGoals.Add(goal);
            }
        }

        private void RemoveGoalFromToDoList(int goalId)
        {
            TryRemoveGoalFromGroupedList(goalId, _morningGoals);
            TryRemoveGoalFromGroupedList(goalId, _afternoonGoals);
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
