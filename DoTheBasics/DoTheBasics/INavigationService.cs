using System.Threading.Tasks;

namespace DoTheBasics
{
    public interface INavigationService
    {
        Task NavigateToEditAsync();
        Task NavigateToEditAsync(int goalId);
        Task NavigateToMainAsync();
    }
}
