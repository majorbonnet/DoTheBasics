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
        private readonly INavigation _navigation;

        public NavigationService(INavigation navigation)
        {
            _navigation = navigation;
        }

        public async Task NavigateToEditAsync()
        {
            await _navigation.PushAsync(new EditGoal());
        }

        public async Task NavigateToEditAsync(int goalId)
        {
            await _navigation.PushAsync(new EditGoal(goalId));
        }

        public async Task NavigateToMainAsync()
        {
            await _navigation.PushAsync(new MainPage());
        }
    }
}
