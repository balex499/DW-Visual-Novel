using Naninovel;
using QuestPrototype.Interaction;

namespace Naninovel.Commands
{
    [CommandAlias("hidelocints")]
    public class HideLocationInteractions : Command
    {
        public override UniTask Execute(AsyncToken token = default)
        {
            var service = Engine.GetService<IQuestInteractionService>();
            if (service is null)
            {
                Warn("Quest interaction service is not initialized.");
                return UniTask.CompletedTask;
            }

            service.Hide();
            return UniTask.CompletedTask;
        }
    }
}