using Naninovel;
using QuestPrototype.Interaction;

namespace Naninovel.Commands
{
    [CommandAlias("showlocints")]
    public class ShowLocationInteractions : Command
    {
        [ParameterAlias("location"), RequiredParameter]
        public StringParameter LocationId;

        public override UniTask Execute(AsyncToken token = default)
        {
            var service = Engine.GetService<IQuestInteractionService>();
            if (service is null)
            {
                Warn("Quest interaction service is not initialized.");
                return UniTask.CompletedTask;
            }

            service.Show(LocationId);
            return UniTask.CompletedTask;
        }
    }
}