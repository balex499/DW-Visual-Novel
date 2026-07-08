using Naninovel;
using QuestPrototype.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace QuestPrototype.Navigation
{
    [InitializeAtRuntime]
    public class QuestNavigationService : IQuestNavigationService
    {
        private const string FallbackPrefabPath = "UI/LocationNavigationView";

        private IScriptPlayer scriptPlayer;
        private LocationNavigationView navigationView;
        private bool isNavigating;

        public UniTask InitializeService()
        {
            scriptPlayer = Engine.GetServiceOrErr<IScriptPlayer>();
            navigationView = FindOrCreateView();

            if (navigationView)
            {
                navigationView.Bind(this);
                navigationView.SetVisible(false);
            }
            else Engine.Warn("Quest navigation view is not found and runtime fallback creation failed.");

            return UniTask.CompletedTask;
        }

        public void ResetService()
        {
            isNavigating = false;
            if (navigationView) navigationView.SetVisible(false);
        }

        public void DestroyService()
        {
            if (navigationView) Object.Destroy(navigationView.gameObject);
            navigationView = null;
            scriptPlayer = null;
        }

        public void Show(string currentLocation)
        {
            if (!navigationView)
            {
                Engine.Warn("Location navigation UI is unavailable. Ensure prefab exists or let fallback build succeed.");
                return;
            }
            navigationView.SetCurrentLocation(currentLocation);
            navigationView.SetVisible(true);
            if (!string.IsNullOrWhiteSpace(currentLocation))
                Engine.GetService<IQuestInteractionService>()?.Show(currentLocation);
        }

        public void Hide()
        {
            if (!navigationView) return;
            navigationView.SetVisible(false);
            Engine.GetService<IQuestInteractionService>()?.Hide();
        }

        public void NavigateTo(string locationLabel)
        {
            if (isNavigating || string.IsNullOrWhiteSpace(locationLabel)) return;
            NavigateToInternal(locationLabel).Forget();
        }

        private async UniTaskVoid NavigateToInternal(string locationLabel)
        {
            isNavigating = true;
            Hide();

            if (!scriptPlayer.PlayedScript)
            {
                Engine.Warn("Can't navigate between locations: no played script in script player.");
                isNavigating = false;
                return;
            }

            var target = locationLabel.StartsWith(".") ? locationLabel.Substring(1) : locationLabel;
            var scriptPath = scriptPlayer.PlayedScript.Path;
            await UniTask.Yield();
            scriptPlayer.PlayAtLabel(scriptPath, target);

            isNavigating = false;
        }

        private static LocationNavigationView FindOrCreateView()
        {
            var existingViews = Object.FindObjectsOfType<LocationNavigationView>(true);
            if (existingViews is { Length: > 0 })
                return existingViews[0];

            var prefab = Resources.Load<LocationNavigationView>(FallbackPrefabPath);
            if (prefab)
            {
                var instance = Object.Instantiate(prefab);
                Object.DontDestroyOnLoad(instance.gameObject);
                return instance;
            }

            return CreateRuntimeFallbackView();
        }

        private static LocationNavigationView CreateRuntimeFallbackView()
        {
            var canvasGO = new GameObject("LocationNavigationView_Runtime", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var rootGO = new GameObject("Root", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            rootGO.transform.SetParent(canvasGO.transform, false);
            var rootRect = (RectTransform)rootGO.transform;
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 40f);
            rootRect.sizeDelta = new Vector2(900f, 100f);

            var rootImage = rootGO.GetComponent<Image>();
            rootImage.color = new Color(0f, 0f, 0f, 0.6f);

            var layout = rootGO.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;

            var corridorButton = CreateButton(rootGO.transform, "Corridor");
            var officeButton = CreateButton(rootGO.transform, "Office");
            var classroomButton = CreateButton(rootGO.transform, "Classroom");

            var view = canvasGO.AddComponent<LocationNavigationView>();
            view.ConfigureRuntime(rootGO, corridorButton, officeButton, classroomButton);
            Object.DontDestroyOnLoad(canvasGO);
            return view;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            var buttonGO = new GameObject($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGO.transform.SetParent(parent, false);

            var image = buttonGO.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.9f);

            var textGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(buttonGO.transform, false);
            var textRect = (RectTransform)textGO.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            text.fontSize = 30;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return buttonGO.GetComponent<Button>();
        }
    }
}
