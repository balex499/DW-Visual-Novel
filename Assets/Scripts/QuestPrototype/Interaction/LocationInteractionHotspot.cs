using System;
using Naninovel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuestPrototype.Interaction
{
    public enum HotspotInteractionMode
    {
        Dialogue,
        Collect,
        CollectViaMiniGame,
        Object
    }

    [Serializable]
    public struct HotspotVariableCondition
    {
        [SerializeField] private string variableName;
        [SerializeField] private bool expectedValue;
        [SerializeField] private bool treatMissingAsVisible;

        public HotspotVariableCondition(string variableName, bool expectedValue, bool treatMissingAsVisible = false)
        {
            this.variableName = variableName;
            this.expectedValue = expectedValue;
            this.treatMissingAsVisible = treatMissingAsVisible;
        }

        public bool IsMet(ICustomVariableManager variableManager)
        {
            if (string.IsNullOrWhiteSpace(variableName)) return true;
            if (!variableManager.TryGetVariableValue(variableName, out bool actualValue)) return treatMissingAsVisible;
            return actualValue == expectedValue;
        }
    }

    public class LocationInteractionHotspot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private string hotspotId;
        [SerializeField] private string targetLabel;
        [SerializeField] private HotspotInteractionMode interactionMode = HotspotInteractionMode.Object;
        [SerializeField] private GameObject root;
        [SerializeField] private HotspotVariableCondition[] visibleWhen = Array.Empty<HotspotVariableCondition>();

        private LocationInteractionService interactionService;
        private ICustomVariableManager variableManager;
        private Button cachedButton;

        public string TargetLabel => targetLabel;
        public HotspotInteractionMode InteractionMode => interactionMode;

        public void Bind(LocationInteractionService service, ICustomVariableManager variables)
        {
            interactionService = service;
            variableManager = variables;
            EnsureRaycastTarget();
            BindButton();
            RefreshVisibility();
        }

        public void ConfigureRuntime(GameObject rootObject, string id, string label, HotspotInteractionMode mode, HotspotVariableCondition[] conditions)
        {
            root = rootObject;
            hotspotId = id;
            targetLabel = label;
            interactionMode = mode;
            visibleWhen = conditions ?? Array.Empty<HotspotVariableCondition>();
        }

        public void RefreshVisibility()
        {
            var visible = ShouldBeVisible();
            var target = root ? root : gameObject;
            target.SetActive(visible);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!ShouldBeVisible()) return;
            interactionService?.ActivateHotspot(this);
        }

        private void BindButton()
        {
            cachedButton = GetComponent<Button>();
            if (!cachedButton) cachedButton = GetComponentInChildren<Button>(true);
            if (!cachedButton) return;

            cachedButton.onClick.RemoveListener(HandleButtonClicked);
            cachedButton.onClick.AddListener(HandleButtonClicked);
        }

        private void EnsureRaycastTarget()
        {
            var graphic = GetComponent<Graphic>();
            if (!graphic)
            {
                var image = gameObject.AddComponent<Image>();
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
                return;
            }

            graphic.raycastTarget = true;
        }

        private void HandleButtonClicked()
        {
            if (!ShouldBeVisible()) return;
            interactionService?.ActivateHotspot(this);
        }

        private bool ShouldBeVisible()
        {
            if (string.IsNullOrWhiteSpace(targetLabel)) return false;
            if (variableManager is null) return true;

            for (var i = 0; i < visibleWhen.Length; i++)
                if (!visibleWhen[i].IsMet(variableManager))
                    return false;

            return true;
        }
    }
}