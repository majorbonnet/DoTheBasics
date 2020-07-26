using System;
using System.Collections.Generic;
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
        public List<BasicGoal> BasicsToDo { get; set; }

        public MainPage()
        {
            InitializeComponent();

            this.BasicsToDo = new List<BasicGoal>();
            this.BasicsToDo.Add(new BasicGoal { Description = "Take a shower", IsDone = false });

            this.ToDosListView.ItemsSource = this.BasicsToDo;
        }

        private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }
    }
}