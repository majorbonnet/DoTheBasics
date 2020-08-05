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
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private bool _showCompleted;

        public MainPage()
        {
            InitializeComponent();

            var goalDatabase = new GoalDatabase();

            BindingContext = new MainPageViewModel(goalDatabase, new NavigationService(Navigation));
        }

        public bool ShowCompleted
        {
            get
            {
                return _showCompleted;
            }

            private set
            {
                _showCompleted = value;
                this.OnPropertyChanged("ShowCompleted");
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await ((MainPageViewModel)BindingContext).OnAppearing();
        }

        private void ToggleCompletedTapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            this.ShowCompleted = !this.ShowCompleted;
        }
    }
}