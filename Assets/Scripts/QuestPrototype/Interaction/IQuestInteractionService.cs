using Naninovel;

namespace QuestPrototype.Interaction
{
    public interface IQuestInteractionService : IEngineService
    {
        void Show(string locationId);
        void Hide();
    }
}