using Naninovel;
using QuestPrototype.Navigation;

namespace Naninovel.Commands
{
    [CommandAlias("showlocnav")]
    public class ShowLocationNavigation : Command
    {
        [ParameterAlias("current")]
        public StringParameter CurrentLocation;

        public override UniTask Execute(AsyncToken token = default)
        {
            var service = Engine.GetService<IQuestNavigationService>();
            if (service is null)
            {
                Warn("Quest navigation service is not initialized.");
                return UniTask.CompletedTask;
            }

            service.Show(Assigned(CurrentLocation) ? CurrentLocation.Value : null);
            return UniTask.CompletedTask;
        }
    }
}
