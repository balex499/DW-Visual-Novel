using Naninovel;
using QuestPrototype.Navigation;

namespace Naninovel.Commands
{
    [CommandAlias("goloc")]
    public class NavigateToLocation : Command
    {
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringParameter Location;

        public override UniTask Execute(AsyncToken token = default)
        {
            var service = Engine.GetService<IQuestNavigationService>();
            if (service is null)
            {
                Warn("Quest navigation service is not initialized.");
                return UniTask.CompletedTask;
            }

            service.NavigateTo(Location);
            return UniTask.CompletedTask;
        }
    }
}
