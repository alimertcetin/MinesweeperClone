using UnityEngine;

namespace XIV.TweenSystem
{
    internal sealed class ScaleTween : TweenDriver<Vector3, Transform>
    {
        protected override void OnUpdate(float normalizedEasedTime)
        {
            component.localScale = Vector3.Lerp(startValue, endValue, normalizedEasedTime);
        }
    }
}