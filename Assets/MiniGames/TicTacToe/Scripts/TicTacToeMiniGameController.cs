using System.Threading;
using System.Collections.Generic;
using Naninovel;
using UnityEngine;

namespace QuestPrototype.MiniGames.TicTacToe
{
    public class TicTacToeMiniGameController : MonoBehaviour
    {
        private const string PrefabPath = "MiniGames/TicTacToe/TicTacToeCanvas";
        private const int FinishDelayMs = 650;

        private readonly int[] board = new int[TicTacToeMiniGameView.CellCount];
        private readonly List<int> availableMoves = new List<int>(TicTacToeMiniGameView.CellCount);

        private UniTaskCompletionSource<bool> completionSource;
        private TicTacToeMiniGameView view;
        private CancellationToken cancellationToken;
        private bool gameFinished;

        public static async UniTask<bool> PlayAsync(AsyncToken token = default)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            if (!prefab)
            {
                Engine.Warn($"Mini-game prefab '{PrefabPath}' was not found in Resources.");
                return false;
            }

            var instance = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(instance);

            var controller = instance.GetComponent<TicTacToeMiniGameController>();
            if (!controller)
            {
                Engine.Warn($"Mini-game prefab '{PrefabPath}' doesn't contain {nameof(TicTacToeMiniGameController)}.");
                Object.Destroy(instance);
                return false;
            }

            var view = instance.GetComponent<TicTacToeMiniGameView>() ?? instance.GetComponentInChildren<TicTacToeMiniGameView>(true);
            if (!view)
            {
                Engine.Warn($"Mini-game prefab '{PrefabPath}' doesn't contain {nameof(TicTacToeMiniGameView)}.");
                Object.Destroy(instance);
                return false;
            }

            if (!view.IsConfigured())
            {
                Engine.Warn($"Mini-game view '{view.name}' is not configured. Assign title, status, close button, and 9 cell button/text pairs in the prefab.");
                Object.Destroy(instance);
                return false;
            }

            return await controller.RunAsync(view, token);
        }

        private async UniTask<bool> RunAsync(TicTacToeMiniGameView gameView, AsyncToken token)
        {
            view = gameView;
            cancellationToken = token.CancellationToken;
            completionSource = new UniTaskCompletionSource<bool>();
            gameFinished = false;

            view.OnCellPressed += HandleCellPressed;
            view.OnClosePressed += HandleClosePressed;

            StartGame();

            using var cancelReg = cancellationToken.Register(() => Finish(false, string.Empty));
            return await completionSource.Task;
        }

        private void StartGame()
        {
            for (var i = 0; i < board.Length; i++)
            {
                board[i] = 0;
                view.SetCell(i, '\0');
                view.SetCellInteractable(i, true);
            }

            view.SetTitle("Tic-Tac-Toe");
            view.SetStatus("Win to collect the key.");
            view.SetBoardInteractable(true);
        }

        private void HandleCellPressed(int index)
        {
            if (gameFinished) return;
            if (index < 0 || index >= board.Length) return;
            if (board[index] != 0) return;

            MakeMove(index, 1);
            if (HasWinner(1))
            {
                Finish(true, "You won!");
                return;
            }

            if (IsBoardFull())
            {
                Finish(false, "Draw. Try again.");
                return;
            }

            MakeAiMove();
            if (HasWinner(2))
            {
                Finish(false, "You lost. Try again.");
                return;
            }

            if (IsBoardFull())
                Finish(false, "Draw. Try again.");
        }

        private void HandleClosePressed()
        {
            if (gameFinished) return;
            Finish(false, "Mini-game closed.");
        }

        private void MakeMove(int index, int marker)
        {
            board[index] = marker;
            view.SetCell(index, marker == 1 ? 'X' : 'O');
            view.SetCellInteractable(index, false);
        }

        private void MakeAiMove()
        {
            availableMoves.Clear();
            for (var i = 0; i < board.Length; i++)
                if (board[i] == 0)
                    availableMoves.Add(i);

            if (availableMoves.Count == 0) return;

            var aiMove = availableMoves[Random.Range(0, availableMoves.Count)];
            MakeMove(aiMove, 2);
        }

        private bool HasWinner(int marker)
        {
            return (board[0] == marker && board[1] == marker && board[2] == marker)
                   || (board[3] == marker && board[4] == marker && board[5] == marker)
                   || (board[6] == marker && board[7] == marker && board[8] == marker)
                   || (board[0] == marker && board[3] == marker && board[6] == marker)
                   || (board[1] == marker && board[4] == marker && board[7] == marker)
                   || (board[2] == marker && board[5] == marker && board[8] == marker)
                   || (board[0] == marker && board[4] == marker && board[8] == marker)
                   || (board[2] == marker && board[4] == marker && board[6] == marker);
        }

        private bool IsBoardFull()
        {
            for (var i = 0; i < board.Length; i++)
                if (board[i] == 0)
                    return false;

            return true;
        }

        private void Finish(bool won, string status)
        {
            if (gameFinished) return;
            gameFinished = true;

            view.SetStatus(status);
            view.SetBoardInteractable(false);
            CloseAfterDelayAsync(won).Forget();
        }

        private async UniTaskVoid CloseAfterDelayAsync(bool won)
        {
            await UniTask.Delay(FinishDelayMs, cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (view)
            {
                view.OnCellPressed -= HandleCellPressed;
                view.OnClosePressed -= HandleClosePressed;
            }

            completionSource.TrySetResult(won);
            Destroy(gameObject);
        }
    }
}
