using QuestPrototype.Interaction;
using Naninovel;
using UnityEngine;

namespace QuestPrototype.Interaction
{
    public class LocationInteractionsView : MonoBehaviour
    {
        [SerializeField] private string locationId;
        [SerializeField] private GameObject root;
        [SerializeField] private LocationInteractionHotspot[] hotspots;

        public string LocationId => locationId;

        public void Bind(LocationInteractionService service, ICustomVariableManager variableManager)
        {
            var boundHotspots = GetHotspots();
            for (var i = 0; i < boundHotspots.Length; i++)
                if (boundHotspots[i]) boundHotspots[i].Bind(service, variableManager);
        }

        public void ConfigureRuntime(string id, GameObject rootObject, LocationInteractionHotspot[] runtimeHotspots)
        {
            locationId = id;
            root = rootObject;
            hotspots = runtimeHotspots;
        }

        public void SetVisible(bool visible)
        {
            var target = root ? root : gameObject;
            target.SetActive(visible);
        }

        public void RefreshVisibility()
        {
            var boundHotspots = GetHotspots();
            for (var i = 0; i < boundHotspots.Length; i++)
                if (boundHotspots[i]) boundHotspots[i].RefreshVisibility();
        }

        private LocationInteractionHotspot[] GetHotspots()
        {
            if (hotspots is { Length: > 0 }) return hotspots;
            hotspots = GetComponentsInChildren<LocationInteractionHotspot>(true);
            return hotspots;
        }
    }
}