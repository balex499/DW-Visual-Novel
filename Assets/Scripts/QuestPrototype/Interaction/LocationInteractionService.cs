using Naninovel;
using QuestPrototype.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace QuestPrototype.Interaction
{
    [InitializeAtRuntime]
    public class LocationInteractionService : IQuestInteractionService
    {
        private const string PrefabPathPrefix = "Interactions/";
        private const string PrefabPathSuffix = "InteractionLayer";

        private IScriptPlayer scriptPlayer;
        private ICustomVariableManager variableManager;
        private LocationInteractionsView activeView;
        private string activeLocation;
        private bool handlingInteraction;

        public UniTask InitializeService()
        {
            scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            variableManager = Engine.GetServiceOrErr<ICustomVariableManager>();
            variableManager.OnVariableUpdated += HandleVariableUpdated;
            return UniTask.CompletedTask;
        }

        public void ResetService()
        {
            handlingInteraction = false;
            Hide();
            DestroyActiveView();
            activeLocation = null;
        }

        public void DestroyService()
        {
            if (variableManager != null)
                variableManager.OnVariableUpdated -= HandleVariableUpdated;

            DestroyActiveView();
            activeLocation = null;
            scriptPlayer = null;
            variableManager = null;
        }

        public void Show(string locationId)
        {
            if (string.IsNullOrWhiteSpace(locationId)) return;

            if (!activeView || !string.Equals(activeLocation, locationId, System.StringComparison.OrdinalIgnoreCase))
                ReplaceView(locationId);

            if (!activeView)
            {
                Engine.Warn($"Location interaction layer for '{locationId}' is unavailable.");
                return;
            }

            activeView.RefreshVisibility();
            activeView.SetVisible(true);

            var hotspots = Object.FindObjectsOfType<LocationInteractionHotspot>(true);
            if (hotspots == null || hotspots.Length == 0)
                Engine.Warn($"Interaction layer '{locationId}' was shown, but no active hotspot components were found in hierarchy.");
        }

        public void Hide()
        {
            if (activeView) activeView.SetVisible(false);
        }

        public void ActivateHotspot(LocationInteractionHotspot hotspot)
        {
            if (handlingInteraction || hotspot is null) return;
            ActivateHotspotAsync(hotspot).Forget();
        }

        private async UniTaskVoid ActivateHotspotAsync(LocationInteractionHotspot hotspot)
        {
            handlingInteraction = true;

            Hide();
            Engine.GetService<IQuestNavigationService>()?.Hide();

            if (!scriptPlayer.PlayedScript)
            {
                Engine.Warn("Can't start location interaction: no played script in script player.");
                handlingInteraction = false;
                return;
            }

            var scriptPath = scriptPlayer.PlayedScript.Path;
            await UniTask.Yield();
            scriptPlayer.PlayAtLabel(scriptPath, hotspot.TargetLabel);
            handlingInteraction = false;
        }

        private void HandleVariableUpdated(CustomVariableUpdatedArgs _)
        {
            if (activeView) activeView.RefreshVisibility();
        }

        private void ReplaceView(string locationId)
        {
            DestroyActiveView();

            activeLocation = locationId;
            activeView = LoadView(locationId);
            if (!activeView) return;

            activeView.Bind(this, variableManager);
            activeView.SetVisible(false);
        }

        private LocationInteractionsView LoadView(string locationId)
        {
            var prefab = Resources.Load<GameObject>($"{PrefabPathPrefix}{locationId}{PrefabPathSuffix}");
            if (prefab)
            {
                var instance = Object.Instantiate(prefab);
                NormalizeRootTransform(instance.transform);

                var view = instance.GetComponent<LocationInteractionsView>() ?? instance.GetComponentInChildren<LocationInteractionsView>(true);
                if (view)
                {
                    Object.DontDestroyOnLoad(instance.gameObject);
                    return view;
                }

                Engine.Warn($"Interaction prefab '{prefab.name}' is loaded, but it doesn't contain {nameof(LocationInteractionsView)}.");
                Object.Destroy(instance);
            }

            return CreateFallbackView(locationId);
        }

        private void DestroyActiveView()
        {
            if (activeView) Object.Destroy(activeView.gameObject);
            activeView = null;
        }

        private static void NormalizeRootTransform(Transform rootTransform)
        {
            if (!rootTransform) return;

            rootTransform.localScale = Vector3.one;
            rootTransform.localRotation = Quaternion.identity;
            rootTransform.localPosition = Vector3.zero;

            if (rootTransform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        private static LocationInteractionsView CreateFallbackView(string locationId)
        {
            var canvasGO = new GameObject($"{locationId}InteractionLayer_Runtime", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 450;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var rootGO = new GameObject("Root", typeof(RectTransform));
            rootGO.transform.SetParent(canvasGO.transform, false);
            var rootRect = (RectTransform)rootGO.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var hotspots = CreateFallbackHotspots(locationId, rootGO.transform);
            var view = canvasGO.AddComponent<LocationInteractionsView>();
            view.ConfigureRuntime(locationId, rootGO, hotspots);
            Object.DontDestroyOnLoad(canvasGO);
            return view;
        }

        private static LocationInteractionHotspot[] CreateFallbackHotspots(string locationId, Transform parent)
        {
            switch (locationId)
            {
                case "Corridor":
                    return new[]
                    {
                        CreateHotspot(parent, "IchigoHotspot", "Ichigo", "IchigoInteraction", HotspotInteractionMode.Dialogue,
                            new Vector2(-500f, -20f), new Vector2(260f, 420f))
                    };
                case "Office":
                    return new[]
                    {
                        CreateHotspot(parent, "SafeHotspot", "Safe", "Safe", HotspotInteractionMode.Object,
                            new Vector2(520f, -40f), new Vector2(280f, 260f),
                            new HotspotVariableCondition("HasItem", false))
                    };
                case "Classroom":
                    return new[]
                    {
                        CreateHotspot(parent, "KeyHotspot", "Key", "Key", HotspotInteractionMode.CollectViaMiniGame,
                            new Vector2(420f, -260f), new Vector2(180f, 120f),
                            new HotspotVariableCondition("HasKey", false),
                            new HotspotVariableCondition("HasItem", false))
                    };
                default:
                    return System.Array.Empty<LocationInteractionHotspot>();
            }
        }

        private static LocationInteractionHotspot CreateHotspot(Transform parent, string objectName, string labelText, string targetLabel,
            HotspotInteractionMode mode, Vector2 anchoredPosition, Vector2 size, params HotspotVariableCondition[] conditions)
        {
            var hotspotGO = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(LocationInteractionHotspot));
            hotspotGO.transform.SetParent(parent, false);

            var rect = (RectTransform)hotspotGO.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = hotspotGO.GetComponent<Image>();
            image.color = new Color(1f, 0.85f, 0.2f, 0.18f);
            image.raycastTarget = true;

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(hotspotGO.transform, false);
            var textRect = (RectTransform)textGO.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.GetComponent<Text>();
            text.text = labelText;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            text.fontSize = 28;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var hotspot = hotspotGO.GetComponent<LocationInteractionHotspot>();
            hotspot.ConfigureRuntime(hotspotGO, objectName, targetLabel, mode, conditions);
            return hotspot;
        }
    }
}