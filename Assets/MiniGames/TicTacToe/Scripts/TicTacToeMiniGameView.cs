using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace QuestPrototype.MiniGames.TicTacToe
{
    public class TicTacToeMiniGameView : MonoBehaviour
    {
        public const int CellCount = 9;

        public event Action<int> OnCellPressed;
        public event Action OnClosePressed;

        public TMPro.TMP_Text TitleText;
        public TMPro.TMP_Text StatusText;
        public Button CloseButton;
        public Button[] CellButtons = new Button[CellCount];
        public TMPro.TMP_Text[] CellTexts = new TMPro.TMP_Text[CellCount];

        private readonly UnityAction[] cellHandlers = new UnityAction[CellCount];
        private UnityAction closeHandler;

        private void Awake()
        {
            BindControls();
        }

        private void OnDestroy()
        {
            UnbindControls();
        }

        public void SetStatus(string text)
        {
            if (StatusText) StatusText.text = text;
        }

        public void SetTitle(string text)
        {
            if (TitleText) TitleText.text = text;
        }

        public void SetCell(int index, char marker)
        {
            if (index < 0 || index >= CellCount) return;
            if (CellTexts[index]) CellTexts[index].text = marker == '\0' ? string.Empty : marker.ToString();
        }

        public void SetBoardInteractable(bool interactable)
        {
            for (var i = 0; i < CellCount; i++)
                if (CellButtons[i])
                    CellButtons[i].interactable = interactable && CellTexts[i] && string.IsNullOrEmpty(CellTexts[i].text);

            if (CloseButton) CloseButton.interactable = true;
        }

        public void SetCellInteractable(int index, bool interactable)
        {
            if (index < 0 || index >= CellCount) return;
            if (CellButtons[index]) CellButtons[index].interactable = interactable;
        }

        public bool IsConfigured()
        {
            if (!TitleText || !StatusText || !CloseButton) return false;
            if (CellButtons == null || CellButtons.Length != CellCount) return false;
            if (CellTexts == null || CellTexts.Length != CellCount) return false;

            for (var i = 0; i < CellCount; i++)
                if (!CellButtons[i] || !CellTexts[i])
                    return false;

            return true;
        }

        private void BindControls()
        {
            NormalizeAssignedArrays();

            for (var i = 0; i < CellCount; i++)
            {
                if (!CellButtons[i]) continue;

                var index = i;
                cellHandlers[i] ??= () => OnCellPressed?.Invoke(index);
                CellButtons[i].onClick.RemoveListener(cellHandlers[i]);
                CellButtons[i].onClick.AddListener(cellHandlers[i]);
            }

            if (!CloseButton) return;

            closeHandler ??= () => OnClosePressed?.Invoke();
            CloseButton.onClick.RemoveListener(closeHandler);
            CloseButton.onClick.AddListener(closeHandler);
        }

        private void UnbindControls()
        {
            for (var i = 0; i < CellCount; i++)
            {
                if (!CellButtons[i] || cellHandlers[i] is null) continue;
                CellButtons[i].onClick.RemoveListener(cellHandlers[i]);
            }

            if (CloseButton && closeHandler != null)
                CloseButton.onClick.RemoveListener(closeHandler);
        }

        private void NormalizeAssignedArrays()
        {
            if (CellButtons == null || CellButtons.Length != CellCount)
                CellButtons = new Button[CellCount];

            if (CellTexts == null || CellTexts.Length != CellCount)
                CellTexts = new TMPro.TMP_Text[CellCount];
        }
    }
}
