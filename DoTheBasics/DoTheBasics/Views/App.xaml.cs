using DoTheBasics.Repo;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DoTheBasics.Views
{
    public partial class App : Application
    {
        private readonly GoalDatabase _goalDb;

        public App()
        {
            InitializeComponent();
            _goalDb = new GoalDatabase();
            MainPage = new NavigationPage(new MainPage());
        }

        protected async override void OnStart()
        {
            await _goalDb.EnsureTablesAreCreatedAsync();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
