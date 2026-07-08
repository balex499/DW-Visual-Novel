using Naninovel;
using QuestPrototype.Navigation;

namespace Naninovel.Commands
{
    [CommandAlias("hidelocnav")]
    public class HideLocationNavigation : Command
    {
        public override UniTask Execute(AsyncToken token = default)
        {
            var service = Engine.GetService<IQuestNavigationService>();
            if (service is null)
            {
                Warn("Quest navigation service is not initialized.");
                return UniTask.CompletedTask;
            }

            service.Hide();
            return UniTask.CompletedTask;
        }
    }
}
