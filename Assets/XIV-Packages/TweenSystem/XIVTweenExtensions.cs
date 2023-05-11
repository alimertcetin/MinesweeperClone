using UnityEngine;

namespace XIV.TweenSystem
{
    public static class XIVTweenExtensions
    {
        public static bool HasTween(this Component component)
        {
            int instanceID = component.gameObject.GetInstanceID();
            return XIVTweenSystem.HasTween(instanceID);
        }

        public static void CancelTween(this Component component)
        {
            int instanceID = component.gameObject.GetInstanceID();
            XIVTweenSystem.CancelTween(instanceID);
        }

        public static XIVTweenFactory XIVTween(this Component component)
        {
            return XIVTweenFactory.GetTween(component);
        }
    }
}