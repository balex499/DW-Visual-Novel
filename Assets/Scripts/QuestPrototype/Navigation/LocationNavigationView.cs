using UnityEngine;
using UnityEngine.UI;

namespace QuestPrototype.Navigation
{
    public class LocationNavigationView : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Buttons")]
        [SerializeField] private Button corridorButton;
        [SerializeField] private Button officeButton;
        [SerializeField] private Button classroomButton;

        private IQuestNavigationService navigationService;

        public void ConfigureRuntime(GameObject rootObject, Button corridor, Button office, Button classroom)
        {
            root = rootObject;
            corridorButton = corridor;
            officeButton = office;
            classroomButton = classroom;
        }

        public void Bind(IQuestNavigationService service)
        {
            navigationService = service;
            BindButtons();
        }

        public void SetVisible(bool visible)
        {
            var target = root ? root : gameObject;
            target.SetActive(visible);
        }

        public void SetCurrentLocation(string currentLocation)
        {
            SetButtonState(corridorButton, !IsCurrent(currentLocation, "Corridor"));
            SetButtonState(officeButton, !IsCurrent(currentLocation, "Office"));
            SetButtonState(classroomButton, !IsCurrent(currentLocation, "Classroom"));
        }

        private void BindButtons()
        {
            BindButton(corridorButton, "Corridor");
            BindButton(officeButton, "Office");
            BindButton(classroomButton, "Classroom");
        }

        private void BindButton(Button button, string targetLabel)
        {
            if (!button) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => navigationService?.NavigateTo(targetLabel));
        }

        private static void SetButtonState(Button button, bool interactable)
        {
            if (!button) return;
            button.interactable = interactable;
        }

        private static bool IsCurrent(string currentLocation, string expected)
        {
            return !string.IsNullOrEmpty(currentLocation) && currentLocation.Equals(expected, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
