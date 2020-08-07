using DoTheBasics.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DoTheBasics
{
    public class NavigationService : INavigationService
    {
        public NavigationService()
        {
        }

        public async Task NavigateToEditAsync()
        {
            await Shell.Current.GoToAsync("editgoal");
        }

        public async Task NavigateToEditAsync(int goalId)
        {
            await Shell.Current.GoToAsync($"editgoal?goalId={goalId}");
        }

        public async Task NavigateToMainAsync()
        {
            await Shell.Current.GoToAsync("//goals/opengoals");
        }
    }
}
