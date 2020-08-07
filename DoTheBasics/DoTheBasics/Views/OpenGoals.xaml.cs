using DoTheBasics.Repo;
using DoTheBasics.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DoTheBasics.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OpenGoals : ContentPage
    {
        public OpenGoals()
        {
            InitializeComponent();

            var goalDatabase = new GoalDatabase();

            BindingContext = new MainPageViewModel(goalDatabase, new NavigationService());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ((MainPageViewModel)BindingContext).OnAppearing();
        }
    }
}