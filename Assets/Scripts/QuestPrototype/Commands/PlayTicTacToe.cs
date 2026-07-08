using Naninovel;
using QuestPrototype.MiniGames.TicTacToe;

namespace Naninovel.Commands
{
    [CommandAlias("playtictactoe")]
    public class PlayTicTacToe : Command
    {
        public override async UniTask Execute(AsyncToken token = default)
        {
            var variableManager = Engine.GetService<ICustomVariableManager>();
            if (variableManager is null)
            {
                Warn("Can't save tic-tac-toe result: custom variable manager is not initialized.");
                return;
            }

            var won = await TicTacToeMiniGameController.PlayAsync(token);
            if (token.Canceled) return;
            variableManager.TrySetVariableValue("TicTacToeWon", won);
        }
    }
}
