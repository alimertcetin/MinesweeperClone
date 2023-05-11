using UnityEngine;

namespace XIV.TweenSystem
{
    internal sealed class MoveTween : TweenDriver<Vector3, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.position = Vector3.Lerp(startValue, endValue, normalizedEasedTime);
        }
    }
}