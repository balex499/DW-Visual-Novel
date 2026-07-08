using Naninovel;

namespace QuestPrototype.Navigation
{
    public interface IQuestNavigationService : IEngineService
    {
        void Show(string currentLocation);
        void Hide();
        void NavigateTo(string locationLabel);
    }
}
